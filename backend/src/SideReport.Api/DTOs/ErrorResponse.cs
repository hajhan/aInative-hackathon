namespace SideReport.Api.DTOs;

/// <summary>
/// 표준 에러 응답 형식
/// </summary>
public class ErrorResponse
{
    /// <summary>HTTP 상태 코드</summary>
    public int StatusCode { get; set; }

    /// <summary>에러 메시지</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>상세 정보 (선택)</summary>
    public string? Detail { get; set; }

    public static ErrorResponse Create(int statusCode, string message, string? detail = null)
        => new() { StatusCode = statusCode, Message = message, Detail = detail };
}
