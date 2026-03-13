namespace SideReport.Domain.Entities;

/// <summary>
/// JWT Refresh Token 엔티티
/// </summary>
public class RefreshToken
{
    /// <summary>자동 증가 ID</summary>
    public long Id { get; set; }

    /// <summary>소유 사용자 ID</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>Refresh Token 값</summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>만료 시각</summary>
    public DateTime Expires { get; set; }

    /// <summary>폐기 여부</summary>
    public bool IsRevoked { get; set; } = false;

    /// <summary>발급 시각</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>토큰이 유효한지 확인</summary>
    public bool IsActive => !IsRevoked && DateTime.UtcNow < Expires;
}
