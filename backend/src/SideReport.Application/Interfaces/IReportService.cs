using SideReport.Application.Reports.Commands;
using SideReport.Application.Reports.Results;

namespace SideReport.Application.Interfaces;

/// <summary>
/// 부작용 보고 서비스 인터페이스
/// </summary>
public interface IReportService
{
    Task<ReportResult> CreateAsync(CreateReportCommand command, string userId, CancellationToken ct = default);
    Task<IReadOnlyList<ReportResult>> GetByUserAsync(string userId, CancellationToken ct = default);
    Task<ReportResult?> GetByIdAsync(Guid id, string userId, CancellationToken ct = default);
    Task DeleteAsync(Guid id, string userId, CancellationToken ct = default);
}
