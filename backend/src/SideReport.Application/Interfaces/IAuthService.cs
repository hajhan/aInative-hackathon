using SideReport.Application.Auth.Commands;
using SideReport.Application.Auth.Results;

namespace SideReport.Application.Interfaces;

/// <summary>
/// 인증 서비스 인터페이스
/// </summary>
public interface IAuthService
{
    /// <summary>회원가입</summary>
    Task<RegisterResult> RegisterAsync(RegisterCommand command, CancellationToken cancellationToken = default);

    /// <summary>로그인</summary>
    Task<LoginResult> LoginAsync(LoginCommand command, CancellationToken cancellationToken = default);

    /// <summary>Access Token 갱신</summary>
    Task<RefreshTokenResult> RefreshTokenAsync(RefreshTokenCommand command, CancellationToken cancellationToken = default);

    /// <summary>로그아웃 (Refresh Token 폐기)</summary>
    Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);
}
