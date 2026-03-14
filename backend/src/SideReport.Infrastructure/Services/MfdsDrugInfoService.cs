using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SideReport.Application.Interfaces;
using SideReport.Application.Ocr.Results;
using SideReport.Domain.Entities;
using SideReport.Infrastructure.Persistence;

namespace SideReport.Infrastructure.Services;

/// <summary>
/// 공공데이터포털 식약처 약품 정보 서비스 (캐시 우선)
/// </summary>
public class MfdsDrugInfoService : IDrugInfoService
{
    private readonly HttpClient _httpClient;
    private readonly ApplicationDbContext _db;
    private readonly string _apiKey;
    private readonly ILogger<MfdsDrugInfoService> _logger;

    private const string MfdsApiUrl =
        "https://apis.data.go.kr/1471000/DrbEasyDrugInfoService/getDrbEasyDrugList";

    public MfdsDrugInfoService(
        HttpClient httpClient,
        ApplicationDbContext db,
        IConfiguration configuration,
        ILogger<MfdsDrugInfoService> logger)
    {
        _httpClient = httpClient;
        _db = db;
        _apiKey = configuration["Mfds:ApiKey"] ?? string.Empty;
        _logger = logger;
    }

    public async Task<DrugInfo?> LookupDrugAsync(string drugName)
    {
        // 1. 캐시 조회
        var cached = await _db.DrugCaches
            .Where(c => c.DrugName == drugName && c.ExpiresAt > DateTime.UtcNow)
            .FirstOrDefaultAsync();

        if (cached != null)
            return new DrugInfo(cached.OfficialName, cached.Ingredient, cached.Efficacy, cached.SideEffects);

        // 2. 식약처 API 호출
        try
        {
            var url = $"{MfdsApiUrl}?serviceKey={_apiKey}&itemName={Uri.EscapeDataString(drugName)}&type=json&numOfRows=1";
            var response = await _httpClient.GetStringAsync(url);
            using var doc = JsonDocument.Parse(response);

            var items = doc.RootElement
                .GetProperty("body")
                .GetProperty("items");

            if (items.GetArrayLength() == 0)
                return null;

            var item = items[0];
            var drugInfo = new DrugInfo(
                OfficialName: item.GetProperty("itemName").GetString() ?? drugName,
                Ingredient: item.TryGetProperty("mainIngredEn", out var ing) ? ing.GetString() : null,
                Efficacy: item.TryGetProperty("efcyQesitm", out var eff) ? eff.GetString() : null,
                SideEffects: item.TryGetProperty("seQesitm", out var se) ? se.GetString() : null
            );

            // 캐시 저장
            _db.DrugCaches.Add(new DrugCache
            {
                DrugName = drugName,
                OfficialName = drugInfo.OfficialName,
                Ingredient = drugInfo.Ingredient,
                Efficacy = drugInfo.Efficacy,
                SideEffects = drugInfo.SideEffects,
                ApiSource = "mfds",
                CachedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            });
            await _db.SaveChangesAsync();

            return drugInfo;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "식약처 API 호출 실패: {DrugName}", drugName);
            return null;
        }
    }
}
