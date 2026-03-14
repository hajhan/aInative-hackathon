namespace SideReport.Application.Ocr.Results;

/// <summary>
/// OCR 텍스트에서 파싱된 약품 항목
/// </summary>
public class ParsedDrugItem
{
    /// <summary>OCR 파싱된 약품명</summary>
    public string DrugName { get; set; } = string.Empty;

    /// <summary>용량 (예: 500mg)</summary>
    public string? Dosage { get; set; }

    /// <summary>복용 횟수 (예: 1일 3회)</summary>
    public string? Frequency { get; set; }

    /// <summary>식약처 공식 약품명 (검증된 경우)</summary>
    public string? OfficialName { get; set; }

    /// <summary>식약처 검증 여부</summary>
    public bool IsVerified { get; set; }

    /// <summary>알려진 부작용 목록 (KAERS)</summary>
    public List<string> KnownSideEffects { get; set; } = [];
}
