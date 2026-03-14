namespace SideReport.Application.Medications.Results;

/// <summary>
/// 복용약 결과 DTO
/// </summary>
public record MedicationResult(
    Guid Id,
    string DrugName,
    string? Dosage,
    string? Frequency,
    DateOnly StartDate,
    DateOnly? EndDate,
    DateTime CreatedAt
);
