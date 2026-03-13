using System.ComponentModel.DataAnnotations;

namespace SideReport.Application.Auth.Commands;

/// <summary>
/// 로그인 커맨드
/// </summary>
public class LoginCommand
{
    /// <summary>이메일</summary>
    [Required(ErrorMessage = "이메일을 입력해 주세요.")]
    [EmailAddress(ErrorMessage = "올바른 이메일 형식이 아닙니다.")]
    public string Email { get; set; } = string.Empty;

    /// <summary>비밀번호</summary>
    [Required(ErrorMessage = "비밀번호를 입력해 주세요.")]
    public string Password { get; set; } = string.Empty;
}
