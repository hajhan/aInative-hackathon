namespace SideReport.Application.Interfaces;

/// <summary>
/// OCR 텍스트 추출 결과
/// </summary>
public record OcrTextResult(string RawText, bool IsSuccess, string? ErrorMessage = null);

/// <summary>
/// OCR 서비스 인터페이스 (Google Vision 또는 Mock)
/// </summary>
public interface IOcrService
{
    /// <summary>
    /// 이미지 스트림에서 텍스트를 추출합니다.
    /// </summary>
    /// <param name="imageStream">이미지 바이트 스트림</param>
    /// <param name="mimeType">이미지 MIME 타입 (예: image/jpeg)</param>
    Task<OcrTextResult> ExtractTextAsync(Stream imageStream, string mimeType);
}
