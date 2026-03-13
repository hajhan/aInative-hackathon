using System.Text.Json;
using SideReport.Api.DTOs;
using SideReport.Domain.Exceptions;

namespace SideReport.Api.Middleware;

/// <summary>
/// 전역 예외 처리 미들웨어 — 일관된 에러 응답 포맷 보장
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "처리되지 않은 예외 발생: {Message}", exception.Message);

        var (statusCode, message) = exception switch
        {
            NotFoundException notFoundEx => (StatusCodes.Status404NotFound, notFoundEx.Message),
            Domain.Exceptions.ValidationException validationEx => (StatusCodes.Status400BadRequest, validationEx.Message),
            UnauthorizedException unauthorizedEx => (StatusCodes.Status401Unauthorized, unauthorizedEx.Message),
            _ => (StatusCodes.Status500InternalServerError, "서버 내부 오류가 발생했습니다.")
        };

        var detail = exception is Domain.Exceptions.ValidationException ve && ve.Errors.Any()
            ? JsonSerializer.Serialize(ve.Errors)
            : null;

        var response = ErrorResponse.Create(statusCode, message, detail);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}
