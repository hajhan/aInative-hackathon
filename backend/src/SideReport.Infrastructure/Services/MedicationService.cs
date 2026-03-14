using Microsoft.EntityFrameworkCore;
using SideReport.Application.Interfaces;
using SideReport.Application.Medications.Commands;
using SideReport.Application.Medications.Results;
using SideReport.Domain.Entities;
using SideReport.Domain.Exceptions;
using SideReport.Infrastructure.Persistence;

namespace SideReport.Infrastructure.Services;

/// <summary>
/// 복용약 관리 서비스
/// </summary>
public class MedicationService : IMedicationService
{
    private readonly ApplicationDbContext _db;

    public MedicationService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<MedicationResult> CreateAsync(CreateMedicationCommand command, string userId, CancellationToken ct = default)
    {
        var medication = new Medication
        {
            UserId = userId,
            DrugName = command.DrugName,
            Dosage = command.Dosage,
            Frequency = command.Frequency,
            StartDate = command.StartDate,
            EndDate = command.EndDate
        };

        _db.Medications.Add(medication);
        await _db.SaveChangesAsync(ct);

        return ToResult(medication);
    }

    public async Task<IReadOnlyList<MedicationResult>> GetByUserAsync(string userId, CancellationToken ct = default)
    {
        var list = await _db.Medications
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(ct);

        return list.Select(ToResult).ToList();
    }

    public async Task<MedicationResult> UpdateAsync(Guid id, UpdateMedicationCommand command, string userId, CancellationToken ct = default)
    {
        var medication = await _db.Medications
            .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId, ct)
            ?? throw new NotFoundException($"복용약 {id}를 찾을 수 없습니다.");

        medication.DrugName = command.DrugName;
        medication.Dosage = command.Dosage;
        medication.Frequency = command.Frequency;
        medication.StartDate = command.StartDate;
        medication.EndDate = command.EndDate;

        await _db.SaveChangesAsync(ct);

        return ToResult(medication);
    }

    public async Task DeleteAsync(Guid id, string userId, CancellationToken ct = default)
    {
        var medication = await _db.Medications
            .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId, ct)
            ?? throw new NotFoundException($"복용약 {id}를 찾을 수 없습니다.");

        _db.Medications.Remove(medication);
        await _db.SaveChangesAsync(ct);
    }

    private static MedicationResult ToResult(Medication m) => new(
        m.Id, m.DrugName, m.Dosage, m.Frequency,
        m.StartDate, m.EndDate, m.CreatedAt
    );
}
