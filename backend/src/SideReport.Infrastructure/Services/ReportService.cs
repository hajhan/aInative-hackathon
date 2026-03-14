using Microsoft.EntityFrameworkCore;
using SideReport.Application.Interfaces;
using SideReport.Application.Reports.Commands;
using SideReport.Application.Reports.Results;
using SideReport.Domain.Entities;
using SideReport.Domain.Exceptions;
using SideReport.Infrastructure.Persistence;

namespace SideReport.Infrastructure.Services;

/// <summary>
/// 부작용 보고 서비스 — KAERS 대조로 IsOfficial 자동 설정
/// </summary>
public class ReportService : IReportService
{
    private readonly ApplicationDbContext _db;

    public ReportService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ReportResult> CreateAsync(CreateReportCommand command, string userId, CancellationToken ct = default)
    {
        // 복용약 존재 및 소유 확인
        var medications = await _db.Medications
            .Where(m => command.MedicationIds.Contains(m.Id) && m.UserId == userId)
            .ToListAsync(ct);

        if (medications.Count != command.MedicationIds.Distinct().Count())
            throw new ValidationException("유효하지 않은 복용약 ID가 포함되어 있습니다.");

        // KAERS 대조: 약품명-증상 조합이 공식 부작용인지 확인
        var drugNames = medications.Select(m => m.DrugName).ToList();
        var officialPairs = await _db.KnownSideEffects
            .Where(k => drugNames.Contains(k.DrugName) && command.Symptoms.Contains(k.SymptomName))
            .Select(k => new { k.DrugName, k.SymptomName })
            .ToListAsync(ct);

        var officialSymptoms = officialPairs.Select(p => p.SymptomName).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var report = new AdverseReport
        {
            UserId = userId,
            Severity = command.Severity,
            Notes = command.Notes,
            ReportSymptoms = command.Symptoms.Select(s => new ReportSymptom
            {
                SymptomName = s,
                IsOfficial = officialSymptoms.Contains(s)
            }).ToList(),
            ReportMedications = medications.Select(m => new ReportMedication
            {
                MedicationId = m.Id
            }).ToList()
        };

        _db.AdverseReports.Add(report);
        await _db.SaveChangesAsync(ct);

        return await ToResultAsync(report.Id, medications, ct);
    }

    public async Task<IReadOnlyList<ReportResult>> GetByUserAsync(string userId, CancellationToken ct = default)
    {
        var reports = await _db.AdverseReports
            .Where(ar => ar.UserId == userId)
            .Include(ar => ar.ReportSymptoms)
            .Include(ar => ar.ReportMedications)
            .OrderByDescending(ar => ar.ReportedAt)
            .ToListAsync(ct);

        var medicationIds = reports
            .SelectMany(ar => ar.ReportMedications.Select(rm => rm.MedicationId))
            .Distinct()
            .ToList();

        var medications = await _db.Medications
            .Where(m => medicationIds.Contains(m.Id))
            .ToDictionaryAsync(m => m.Id, ct);

        return reports.Select(ar => ToResult(ar, medications)).ToList();
    }

    public async Task<ReportResult?> GetByIdAsync(Guid id, string userId, CancellationToken ct = default)
    {
        var report = await _db.AdverseReports
            .Where(ar => ar.Id == id && ar.UserId == userId)
            .Include(ar => ar.ReportSymptoms)
            .Include(ar => ar.ReportMedications)
            .FirstOrDefaultAsync(ct);

        if (report is null) return null;

        var medicationIds = report.ReportMedications.Select(rm => rm.MedicationId).ToList();
        var medications = await _db.Medications
            .Where(m => medicationIds.Contains(m.Id))
            .ToDictionaryAsync(m => m.Id, ct);

        return ToResult(report, medications);
    }

    public async Task DeleteAsync(Guid id, string userId, CancellationToken ct = default)
    {
        var report = await _db.AdverseReports
            .FirstOrDefaultAsync(ar => ar.Id == id && ar.UserId == userId, ct)
            ?? throw new NotFoundException($"보고서 {id}를 찾을 수 없습니다.");

        _db.AdverseReports.Remove(report);
        await _db.SaveChangesAsync(ct);
    }

    private async Task<ReportResult> ToResultAsync(Guid reportId, List<Medication> medications, CancellationToken ct)
    {
        var report = await _db.AdverseReports
            .Where(ar => ar.Id == reportId)
            .Include(ar => ar.ReportSymptoms)
            .Include(ar => ar.ReportMedications)
            .FirstAsync(ct);

        var medDict = medications.ToDictionary(m => m.Id);
        return ToResult(report, medDict);
    }

    private static ReportResult ToResult(AdverseReport ar, Dictionary<Guid, Medication> medDict) => new(
        ar.Id,
        ar.ReportedAt,
        ar.Severity,
        ar.Notes,
        ar.ReportSymptoms.Select(rs => new SymptomResult(rs.Id, rs.SymptomName, rs.IsOfficial)).ToList(),
        ar.ReportMedications
            .Where(rm => medDict.ContainsKey(rm.MedicationId))
            .Select(rm => new ReportMedicationResult(rm.MedicationId, medDict[rm.MedicationId].DrugName))
            .ToList()
    );
}
