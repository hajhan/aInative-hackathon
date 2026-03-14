using SideReport.Domain.Enums;

namespace SideReport.Application.Reports.Results;

/// <summary>
/// 부작용 보고 결과 DTO
/// </summary>
public record ReportResult(
    Guid Id,
    DateTime ReportedAt,
    Severity Severity,
    string? Notes,
    List<SymptomResult> Symptoms,
    List<ReportMedicationResult> Medications
);

public record SymptomResult(Guid Id, string SymptomName, bool IsOfficial);

public record ReportMedicationResult(Guid MedicationId, string DrugName);
