using System.ComponentModel.DataAnnotations;

namespace SideReport.Application.Auth.Commands;

/// <summary>
/// Access Token 갱신 커맨드
/// </summary>
public class RefreshTokenCommand
{
    /// <summary>Refresh Token</summary>
    [Required(ErrorMessage = "Refresh Token이 필요합니다.")]
    public string RefreshToken { get; set; } = string.Empty;
}
