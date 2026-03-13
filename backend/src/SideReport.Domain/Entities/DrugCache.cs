namespace SideReport.Domain.Entities;

/// <summary>
/// 식약처 API 약품 정보 캐시
/// </summary>
public class DrugCache
{
    /// <summary>고유 ID</summary>
    public long Id { get; set; }

    /// <summary>OCR 파싱된 약품명 (검색 키)</summary>
    public string DrugName { get; set; } = string.Empty;

    /// <summary>식약처 공식 약품명</summary>
    public string OfficialName { get; set; } = string.Empty;

    /// <summary>주성분</summary>
    public string? Ingredient { get; set; }

    /// <summary>효능·효과</summary>
    public string? Efficacy { get; set; }

    /// <summary>용법·용량</summary>
    public string? UsageInfo { get; set; }

    /// <summary>이상반응 (원문)</summary>
    public string? SideEffects { get; set; }

    /// <summary>API 출처 (mfds, mock)</summary>
    public string ApiSource { get; set; } = "mfds";

    /// <summary>캐시 저장 시각</summary>
    public DateTime CachedAt { get; set; } = DateTime.UtcNow;

    /// <summary>캐시 만료 시각</summary>
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(7);
}
