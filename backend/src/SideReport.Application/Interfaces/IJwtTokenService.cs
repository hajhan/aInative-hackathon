namespace SideReport.Application.Interfaces;

/// <summary>
/// JWT 토큰 서비스 인터페이스
/// </summary>
public interface IJwtTokenService
{
    /// <summary>Access Token 생성 (userId, email, name으로 클레임 구성)</summary>
    string GenerateAccessToken(string userId, string email, string name);

    /// <summary>Refresh Token 생성</summary>
    string GenerateRefreshToken();

    /// <summary>토큰에서 사용자 ID 추출 (만료된 토큰도 허용)</summary>
    string? GetUserIdFromExpiredToken(string token);
}
