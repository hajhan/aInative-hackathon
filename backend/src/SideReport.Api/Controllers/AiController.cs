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
    private readonly IAiAnalysisService? _aiService;
    private readonly ILogger<AiController> _logger;

    public AiController(ILogger<AiController> logger, IAiAnalysisService? aiService = null)
    {
        _logger = logger;
        _aiService = aiService;
    }

    /// <summary>
    /// 복용약 + 증상 기반 AI 분석 (약물 상호작용, 공식 부작용 판별)
    /// </summary>
    [HttpPost("analyze")]
    [ProducesResponseType(typeof(AiAnalysisResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Analyze([FromBody] AnalyzeRequest req, CancellationToken ct)
    {
        if (_aiService is null)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { message = "AI 분석 서비스가 설정되지 않았습니다. ANTHROPIC_API_KEY를 확인하세요." });
        }

        if (req.DrugNames == null || req.DrugNames.Count == 0)
            return BadRequest(new { message = "약품명을 하나 이상 입력해야 합니다." });

        if (req.Symptoms == null || req.Symptoms.Count == 0)
            return BadRequest(new { message = "증상을 하나 이상 입력해야 합니다." });

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedException("사용자 인증 정보를 찾을 수 없습니다.");

        _logger.LogInformation("AI 분석 요청 (UserId: {UserId}, 약품: {Drugs})", userId, string.Join(", ", req.DrugNames));

        var result = await _aiService.AnalyzeAsync(req.DrugNames, req.Symptoms, ct);
        return Ok(result);
    }
}

public record AnalyzeRequest(List<string> DrugNames, List<string> Symptoms);
