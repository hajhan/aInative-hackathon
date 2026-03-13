using Microsoft.AspNetCore.Mvc;

namespace SideReport.Api.Controllers;

/// <summary>
/// 헬스체크 컨트롤러
/// </summary>
[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// 서비스 상태 확인 (Docker healthcheck용)
    /// </summary>
    /// <returns>200 OK</returns>
    [HttpGet]
    [ProducesResponseType(200)]
    public IActionResult Get()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }
}
