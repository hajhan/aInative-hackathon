namespace SideReport.Application.Auth.Results;

/// <summary>
/// 회원가입 결과
/// </summary>
public class RegisterResult
{
    /// <summary>사용자 ID</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>이메일</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>사용자 이름</summary>
    public string Name { get; set; } = string.Empty;
}
