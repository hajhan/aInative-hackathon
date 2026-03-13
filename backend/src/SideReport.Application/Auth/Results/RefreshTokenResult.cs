namespace SideReport.Application.Auth.Results;

/// <summary>
/// Access Token 갱신 결과
/// </summary>
public class RefreshTokenResult
{
    /// <summary>새 Access Token (JWT)</summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>Access Token 만료까지 남은 초</summary>
    public int ExpiresIn { get; set; }
}
