namespace SideReport.Application.Medications.Commands;

/// <summary>
/// 복용약 추가 커맨드
/// </summary>
public record CreateMedicationCommand(
    string DrugName,
    string? Dosage,
    string? Frequency,
    DateOnly StartDate,
    DateOnly? EndDate
);
