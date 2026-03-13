namespace SideReport.Domain.Entities;

/// <summary>
/// 보고 증상 엔티티
/// </summary>
public class ReportSymptom
{
    /// <summary>고유 ID</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>연결 보고서 ID</summary>
    public Guid ReportId { get; set; }

    /// <summary>증상명</summary>
    public string SymptomName { get; set; } = string.Empty;

    /// <summary>공식 부작용 여부 (KAERS 대조)</summary>
    public bool IsOfficial { get; set; } = false;

    /// <summary>연결 보고서 (Navigation Property)</summary>
    public AdverseReport Report { get; set; } = null!;
}
