namespace SideReport.Domain.Exceptions;

/// <summary>
/// 리소스를 찾을 수 없을 때 발생하는 예외 (404)
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }

    public NotFoundException(string resourceName, object key)
        : base($"'{resourceName}' ({key})을(를) 찾을 수 없습니다.") { }
}
