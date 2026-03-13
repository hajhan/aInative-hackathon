namespace SideReport.Domain.Entities;

/// <summary>
/// 복용약 엔티티
/// </summary>
public class Medication
{
    /// <summary>고유 ID</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>소유 사용자 ID</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>약품명</summary>
    public string DrugName { get; set; } = string.Empty;

    /// <summary>용량 (예: 500mg)</summary>
    public string? Dosage { get; set; }

    /// <summary>복용 횟수 (예: 1일 3회)</summary>
    public string? Frequency { get; set; }

    /// <summary>복용 시작일</summary>
    public DateOnly StartDate { get; set; }

    /// <summary>복용 종료일 (복용 중이면 null)</summary>
    public DateOnly? EndDate { get; set; }

    /// <summary>등록 시각</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>연결된 부작용 보고 목록</summary>
    public ICollection<ReportMedication> ReportMedications { get; set; } = new List<ReportMedication>();
}
