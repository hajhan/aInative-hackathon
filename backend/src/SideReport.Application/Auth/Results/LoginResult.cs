namespace SideReport.Application.Auth.Results;

/// <summary>
/// 로그인 결과
/// </summary>
public class LoginResult
{
    /// <summary>Access Token (JWT)</summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>Refresh Token</summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>Access Token 만료까지 남은 초</summary>
    public int ExpiresIn { get; set; }
}
