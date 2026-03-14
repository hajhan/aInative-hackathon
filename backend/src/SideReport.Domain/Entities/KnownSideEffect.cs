namespace SideReport.Domain.Entities;

/// <summary>
/// KAERS 기반 알려진 약품-부작용 쌍
/// </summary>
public class KnownSideEffect
{
    /// <summary>고유 ID</summary>
    public long Id { get; set; }

    /// <summary>약품명 (한국어)</summary>
    public string DrugName { get; set; } = string.Empty;

    /// <summary>증상명</summary>
    public string SymptomName { get; set; } = string.Empty;

    /// <summary>보고 빈도 순위 (낮을수록 빈번)</summary>
    public int FrequencyRank { get; set; }

    /// <summary>데이터 출처 (예: KAERS, MFDS)</summary>
    public string Source { get; set; } = "KAERS";

    /// <summary>등록 시각</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
