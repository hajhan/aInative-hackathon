namespace SideReport.Domain.Exceptions;

/// <summary>
/// 입력값 유효성 검사 실패 시 발생하는 예외 (400)
/// </summary>
public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(string message) : base(message)
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("유효성 검사에 실패했습니다.")
    {
        Errors = errors;
    }
}
