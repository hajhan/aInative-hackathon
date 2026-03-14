using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SideReport.Application.Ai.Results;
using SideReport.Application.Interfaces;
using SideReport.Domain.Exceptions;

namespace SideReport.Api.Controllers;

/// <summary>
/// AI 약물 분석 API
/// </summary>
[ApiController]
[Route("api/ai")]
[Authorize]
public class AiController : ControllerBase
{
    private readonly IAiAnalysisService _aiService;
    private readonly ILogger<AiController> _logger;

    public AiController(IAiAnalysisService aiService, ILogger<AiController> logger)
    {
        _aiService = aiService;
        _logger = logger;
    }

    /// <summary>
    /// 복용약 + 증상 기반 AI 분석 (약물 상호작용, 공식 부작용 판별)
    /// </summary>
    [HttpPost("analyze")]
    [ProducesResponseType(typeof(AiAnalysisResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Analyze([FromBody] AnalyzeRequest req, CancellationToken ct)
    {
        if (req.DrugNames == null || req.DrugNames.Count == 0)
            return BadRequest(new { message = "약품명을 하나 이상 입력해야 합니다." });

        if (req.Symptoms == null || req.Symptoms.Count == 0)
            return BadRequest(new { message = "증상을 하나 이상 입력해야 합니다." });

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedException("사용자 인증 정보를 찾을 수 없습니다.");

        _logger.LogInformation("AI 분석 요청 (UserId: {UserId}, 약품: {Drugs})", userId, string.Join(", ", req.DrugNames));

        try
        {
            var result = await _aiService.AnalyzeAsync(req.DrugNames, req.Symptoms, ct);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "AI 분석 서비스 사용 불가");
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { message = ex.Message });
        }
    }
}

public record AnalyzeRequest(List<string> DrugNames, List<string> Symptoms);
