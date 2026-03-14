using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SideReport.Application.Interfaces;
using SideReport.Application.Ocr.Commands;
using SideReport.Application.Ocr.Results;
using SideReport.Domain.Entities;
using SideReport.Domain.Enums;
using SideReport.Infrastructure.Persistence;

namespace SideReport.Infrastructure.Services;

/// <summary>
/// OCR 전체 파이프라인 오케스트레이션:
/// 이미지 저장 → OCR 추출 → 약품 파싱 → 식약처 검증 → DB 저장 → 이미지 삭제(옵션)
/// </summary>
public class OcrPipelineService : IOcrPipelineService
{
    private readonly IImageStorageService _storage;
    private readonly IOcrService _ocr;
    private readonly IDrugParserService _parser;
    private readonly IDrugInfoService _drugInfo;
    private readonly ApplicationDbContext _db;
    private readonly ILogger<OcrPipelineService> _logger;

    public OcrPipelineService(
        IImageStorageService storage,
        IOcrService ocr,
        IDrugParserService parser,
        IDrugInfoService drugInfo,
        ApplicationDbContext db,
        ILogger<OcrPipelineService> logger)
    {
        _storage = storage;
        _ocr = ocr;
        _parser = parser;
        _drugInfo = drugInfo;
        _db = db;
        _logger = logger;
    }

    public async Task<OcrUploadResult> ProcessAsync(UploadOcrImageCommand command)
    {
        // 1. 이미지 저장
        string? storagePath = null;
        try
        {
            storagePath = await _storage.UploadAsync(command.ImageStream, command.FileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "이미지 저장 실패");
            return new OcrUploadResult { Status = OcrStatus.Failed };
        }

        // 2. OcrImage 레코드 생성 (Pending)
        var ocrImage = new OcrImage
        {
            UserId = command.UserId,
            StorageUrl = storagePath,
            Status = OcrStatus.Processing,
            DeleteAfterProcessing = command.DeleteAfterProcessing,
            CreatedAt = DateTime.UtcNow
        };
        _db.OcrImages.Add(ocrImage);
        await _db.SaveChangesAsync();

        try
        {
            // 3. OCR 텍스트 추출
            command.ImageStream.Position = 0;
            var ocrResult = await _ocr.ExtractTextAsync(command.ImageStream, command.ContentType);
            if (!ocrResult.IsSuccess)
            {
                ocrImage.Status = OcrStatus.Failed;
                await _db.SaveChangesAsync();
                return new OcrUploadResult { Id = ocrImage.Id, Status = OcrStatus.Failed };
            }

            ocrImage.RawOcrText = ocrResult.RawText;

            // 4. 약품 텍스트 파싱
            var parsedDrugs = _parser.ParseOcrText(ocrResult.RawText);

            // 5. 식약처 정보 검증 (동시 요청 3개 제한)
            using var semaphore = new SemaphoreSlim(3, 3);
            var verifyTasks = parsedDrugs.Select(async drug =>
            {
                await semaphore.WaitAsync();
                try
                {
                    var info = await _drugInfo.LookupDrugAsync(drug.DrugName);
                    if (info != null)
                    {
                        drug.OfficialName = info.OfficialName;
                        drug.IsVerified = true;
                    }
                    return drug;
                }
                finally
                {
                    semaphore.Release();
                }
            });
            var verifiedDrugs = (await Task.WhenAll(verifyTasks)).ToList();

            // 6. KAERS 알려진 부작용 조회
            var drugNames = verifiedDrugs.Select(d => d.DrugName).ToList();
            var knownEffects = await _db.KnownSideEffects
                .Where(k => drugNames.Contains(k.DrugName))
                .GroupBy(k => k.DrugName)
                .ToDictionaryAsync(
                    g => g.Key,
                    g => g.OrderBy(k => k.FrequencyRank).Select(k => k.SymptomName).ToList());

            foreach (var drug in verifiedDrugs)
            {
                if (knownEffects.TryGetValue(drug.DrugName, out var effects))
                    drug.KnownSideEffects = effects;
            }

            // 7. 결과 저장
            ocrImage.ParsedResultJson = JsonSerializer.Serialize(verifiedDrugs);
            ocrImage.Status = OcrStatus.Completed;
            ocrImage.ProcessedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            // 8. 이미지 삭제 (옵션)
            if (command.DeleteAfterProcessing)
                await _storage.DeleteAsync(storagePath);

            return new OcrUploadResult
            {
                Id = ocrImage.Id,
                Status = OcrStatus.Completed,
                RawText = ocrResult.RawText,
                Drugs = verifiedDrugs,
                ProcessedAt = ocrImage.ProcessedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OCR 파이프라인 처리 중 오류 (OcrImageId: {Id})", ocrImage.Id);
            ocrImage.Status = OcrStatus.Failed;
            await _db.SaveChangesAsync();

            // 실패 시에도 DeleteAfterProcessing 옵션에 따라 이미지 정리
            if (command.DeleteAfterProcessing && storagePath != null)
            {
                try { await _storage.DeleteAsync(storagePath); }
                catch (Exception delEx)
                {
                    _logger.LogWarning(delEx, "실패 후 이미지 삭제 중 오류 (StoragePath: {Path})", storagePath);
                }
            }

            return new OcrUploadResult { Id = ocrImage.Id, Status = OcrStatus.Failed };
        }
    }

    public async Task<OcrUploadResult?> GetResultAsync(Guid ocrImageId, string userId)
    {
        var ocrImage = await _db.OcrImages
            .FirstOrDefaultAsync(o => o.Id == ocrImageId && o.UserId == userId);

        if (ocrImage == null) return null;

        var drugs = new List<ParsedDrugItem>();
        if (!string.IsNullOrEmpty(ocrImage.ParsedResultJson))
        {
            drugs = JsonSerializer.Deserialize<List<ParsedDrugItem>>(ocrImage.ParsedResultJson) ?? [];
        }

        return new OcrUploadResult
        {
            Id = ocrImage.Id,
            Status = ocrImage.Status,
            RawText = ocrImage.RawOcrText,
            Drugs = drugs,
            ProcessedAt = ocrImage.ProcessedAt
        };
    }

    public async Task ConfirmMedicationsAsync(Guid ocrImageId, string userId, List<ParsedDrugItem> confirmedDrugs)
    {
        var ocrImage = await _db.OcrImages
            .FirstOrDefaultAsync(o => o.Id == ocrImageId && o.UserId == userId)
            ?? throw new KeyNotFoundException("OCR 결과를 찾을 수 없습니다.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var medications = confirmedDrugs.Select(drug => new Medication
        {
            UserId = userId,
            DrugName = drug.OfficialName ?? drug.DrugName,
            Dosage = drug.Dosage,
            Frequency = drug.Frequency,
            StartDate = today,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        _db.Medications.AddRange(medications);
        await _db.SaveChangesAsync();

        _logger.LogInformation("약품 {Count}개 Medications에 저장 완료 (UserId: {UserId})", medications.Count, userId);
    }
}
