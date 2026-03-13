namespace SideReport.Domain.Exceptions;

/// <summary>
/// 인증/인가 실패 시 발생하는 예외 (401)
/// </summary>
public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message = "인증이 필요합니다.") : base(message) { }
}
