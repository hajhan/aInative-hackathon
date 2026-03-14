namespace SideReport.Application.Medications.Commands;

/// <summary>
/// 복용약 수정 커맨드
/// </summary>
public record UpdateMedicationCommand(
    string DrugName,
    string? Dosage,
    string? Frequency,
    DateOnly StartDate,
    DateOnly? EndDate
);
