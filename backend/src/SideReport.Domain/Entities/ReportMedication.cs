namespace SideReport.Domain.Entities;

/// <summary>
/// 보고서-복용약 연결 엔티티
/// </summary>
public class ReportMedication
{
    /// <summary>고유 ID</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>연결 보고서 ID</summary>
    public Guid ReportId { get; set; }

    /// <summary>연결 복용약 ID</summary>
    public Guid MedicationId { get; set; }

    /// <summary>연결 보고서 (Navigation Property)</summary>
    public AdverseReport Report { get; set; } = null!;

    /// <summary>연결 복용약 (Navigation Property)</summary>
    public Medication Medication { get; set; } = null!;
}
