using SideReport.Application.Ocr.Commands;
using SideReport.Application.Ocr.Results;

namespace SideReport.Application.Interfaces;

/// <summary>
/// OCR 전체 파이프라인 오케스트레이션 서비스
/// (이미지 저장 → OCR 추출 → 약품 파싱 → 식약처 검증 → DB 저장)
/// </summary>
public interface IOcrPipelineService
{
    /// <summary>
    /// 이미지를 업로드하고 OCR 파이프라인을 실행합니다.
    /// </summary>
    Task<OcrUploadResult> ProcessAsync(UploadOcrImageCommand command);

    /// <summary>
    /// OCR 결과를 조회합니다.
    /// </summary>
    Task<OcrUploadResult?> GetResultAsync(Guid ocrImageId, string userId);

    /// <summary>
    /// OCR 결과 중 선택한 약품들을 Medications 테이블에 저장합니다.
    /// </summary>
    Task ConfirmMedicationsAsync(Guid ocrImageId, string userId, List<ParsedDrugItem> confirmedDrugs);
}
