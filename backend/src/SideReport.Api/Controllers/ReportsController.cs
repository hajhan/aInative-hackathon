using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SideReport.Application.Interfaces;
using SideReport.Application.Reports.Commands;
using SideReport.Application.Reports.Results;
using SideReport.Domain.Enums;
using SideReport.Domain.Exceptions;

namespace SideReport.Api.Controllers;

/// <summary>
/// 부작용 보고서 CRUD API
/// </summary>
[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _service;

    public ReportsController(IReportService service)
    {
        _service = service;
    }

    /// <summary>내 보고서 목록 조회</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ReportResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var userId = GetUserId();
        var list = await _service.GetByUserAsync(userId, ct);
        return Ok(list);
    }

    /// <summary>보고서 상세 조회</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ReportResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _service.GetByIdAsync(id, userId, ct);

        if (result is null)
            return NotFound(new { message = "보고서를 찾을 수 없습니다." });

        return Ok(result);
    }

    /// <summary>부작용 보고서 생성</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ReportResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateReportRequest req, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        if (req.Symptoms == null || req.Symptoms.Count == 0)
            return BadRequest(new { message = "증상을 하나 이상 입력해야 합니다." });

        var userId = GetUserId();
        var command = new CreateReportCommand(
            req.MedicationIds ?? [],
            req.Symptoms,
            req.Severity,
            req.Notes
        );

        var result = await _service.CreateAsync(command, userId, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>보고서 삭제</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        await _service.DeleteAsync(id, userId, ct);
        return NoContent();
    }

    private string GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedException("사용자 인증 정보를 찾을 수 없습니다.");
}

// ─── Request DTO ──────────────────────────────────────────────────────────────

public record CreateReportRequest(
    List<Guid>? MedicationIds,
    List<string> Symptoms,
    Severity Severity,
    string? Notes
);
