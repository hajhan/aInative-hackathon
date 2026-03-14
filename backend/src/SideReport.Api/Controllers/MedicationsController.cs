using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SideReport.Application.Interfaces;
using SideReport.Application.Medications.Commands;
using SideReport.Application.Medications.Results;
using SideReport.Domain.Exceptions;

namespace SideReport.Api.Controllers;

/// <summary>
/// 내 복용약 관리 API
/// </summary>
[ApiController]
[Route("api/medications")]
[Authorize]
public class MedicationsController : ControllerBase
{
    private readonly IMedicationService _service;

    public MedicationsController(IMedicationService service)
    {
        _service = service;
    }

    /// <summary>내 복용약 목록 조회</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<MedicationResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var userId = GetUserId();
        var list = await _service.GetByUserAsync(userId, ct);
        return Ok(list);
    }

    /// <summary>복용약 추가</summary>
    [HttpPost]
    [ProducesResponseType(typeof(MedicationResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateMedicationRequest req, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = GetUserId();
        var command = new CreateMedicationCommand(req.DrugName, req.Dosage, req.Frequency, req.StartDate, req.EndDate);
        var result = await _service.CreateAsync(command, userId, ct);

        return CreatedAtAction(nameof(GetAll), new { }, result);
    }

    /// <summary>복용약 수정</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(MedicationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMedicationRequest req, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = GetUserId();
        var command = new UpdateMedicationCommand(req.DrugName, req.Dosage, req.Frequency, req.StartDate, req.EndDate);
        var result = await _service.UpdateAsync(id, command, userId, ct);

        return Ok(result);
    }

    /// <summary>복용약 삭제</summary>
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

// ─── Request DTOs ─────────────────────────────────────────────────────────────

public record CreateMedicationRequest(
    string DrugName,
    string? Dosage,
    string? Frequency,
    DateOnly StartDate,
    DateOnly? EndDate
);

public record UpdateMedicationRequest(
    string DrugName,
    string? Dosage,
    string? Frequency,
    DateOnly StartDate,
    DateOnly? EndDate
);
