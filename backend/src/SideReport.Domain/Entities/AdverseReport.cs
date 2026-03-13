using SideReport.Domain.Enums;

namespace SideReport.Domain.Entities;

/// <summary>
/// 부작용 보고 엔티티
/// </summary>
public class AdverseReport
{
    /// <summary>고유 ID</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>보고 사용자 ID</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>보고 시각</summary>
    public DateTime ReportedAt { get; set; } = DateTime.UtcNow;

    /// <summary>심각도</summary>
    public Severity Severity { get; set; }

    /// <summary>추가 메모</summary>
    public string? Notes { get; set; }

    /// <summary>증상 목록</summary>
    public ICollection<ReportSymptom> ReportSymptoms { get; set; } = new List<ReportSymptom>();

    /// <summary>연결된 복용약 목록</summary>
    public ICollection<ReportMedication> ReportMedications { get; set; } = new List<ReportMedication>();
}
