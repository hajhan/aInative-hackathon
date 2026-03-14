using SideReport.Application.Medications.Commands;
using SideReport.Application.Medications.Results;

namespace SideReport.Application.Interfaces;

/// <summary>
/// 복용약 관리 서비스 인터페이스
/// </summary>
public interface IMedicationService
{
    Task<MedicationResult> CreateAsync(CreateMedicationCommand command, string userId, CancellationToken ct = default);
    Task<IReadOnlyList<MedicationResult>> GetByUserAsync(string userId, CancellationToken ct = default);
    Task<MedicationResult> UpdateAsync(Guid id, UpdateMedicationCommand command, string userId, CancellationToken ct = default);
    Task DeleteAsync(Guid id, string userId, CancellationToken ct = default);
}
