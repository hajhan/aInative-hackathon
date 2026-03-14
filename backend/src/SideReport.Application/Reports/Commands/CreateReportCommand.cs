using SideReport.Domain.Enums;

namespace SideReport.Application.Reports.Commands;

/// <summary>
/// 부작용 보고 생성 커맨드
/// </summary>
public record CreateReportCommand(
    List<Guid> MedicationIds,
    List<string> Symptoms,
    Severity Severity,
    string? Notes
);
