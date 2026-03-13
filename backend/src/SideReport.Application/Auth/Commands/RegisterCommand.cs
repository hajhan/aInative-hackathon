using System.ComponentModel.DataAnnotations;

namespace SideReport.Application.Auth.Commands;

/// <summary>
/// 회원가입 커맨드
/// </summary>
public class RegisterCommand
{
    /// <summary>사용자 이름 (실명)</summary>
    [Required(ErrorMessage = "이름을 입력해 주세요.")]
    [MaxLength(100, ErrorMessage = "이름은 100자를 초과할 수 없습니다.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>이메일</summary>
    [Required(ErrorMessage = "이메일을 입력해 주세요.")]
    [EmailAddress(ErrorMessage = "올바른 이메일 형식이 아닙니다.")]
    public string Email { get; set; } = string.Empty;

    /// <summary>비밀번호</summary>
    [Required(ErrorMessage = "비밀번호를 입력해 주세요.")]
    [MinLength(8, ErrorMessage = "비밀번호는 8자 이상이어야 합니다.")]
    public string Password { get; set; } = string.Empty;
}
