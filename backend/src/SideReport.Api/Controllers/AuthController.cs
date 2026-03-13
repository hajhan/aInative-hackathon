using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SideReport.Application.Auth.Commands;
using SideReport.Application.Interfaces;

namespace SideReport.Api.Controllers;

/// <summary>
/// 인증 컨트롤러 — 회원가입, 로그인, 토큰 갱신, 로그아웃
/// </summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// 회원가입
    /// </summary>
    /// <param name="command">회원가입 정보 (이름, 이메일, 비밀번호)</param>
    /// <returns>생성된 사용자 정보</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command)
    {
        var result = await _authService.RegisterAsync(command);
        return CreatedAtAction(nameof(Register), new
        {
            userId = result.UserId,
            email = result.Email,
            name = result.Name
        });
    }

    /// <summary>
    /// 로그인 — Access Token과 Refresh Token 발급
    /// </summary>
    /// <param name="command">로그인 정보 (이메일, 비밀번호)</param>
    /// <returns>Access Token, Refresh Token, 만료 시간</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var result = await _authService.LoginAsync(command);
        return Ok(new
        {
            accessToken = result.AccessToken,
            refreshToken = result.RefreshToken,
            expiresIn = result.ExpiresIn
        });
    }

    /// <summary>
    /// Access Token 갱신
    /// </summary>
    /// <param name="command">Refresh Token</param>
    /// <returns>새 Access Token, 만료 시간</returns>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenCommand command)
    {
        var result = await _authService.RefreshTokenAsync(command);
        return Ok(new
        {
            accessToken = result.AccessToken,
            expiresIn = result.ExpiresIn
        });
    }

    /// <summary>
    /// 로그아웃 — Refresh Token 폐기
    /// </summary>
    /// <param name="command">Refresh Token</param>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenCommand command)
    {
        await _authService.LogoutAsync(command.RefreshToken);
        return NoContent();
    }
}
