namespace SideReport.Application.Ocr.Commands;

/// <summary>
/// OCR 이미지 업로드 커맨드
/// </summary>
public class UploadOcrImageCommand
{
    /// <summary>요청 사용자 ID</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>업로드된 이미지 스트림</summary>
    public Stream ImageStream { get; set; } = Stream.Null;

    /// <summary>원본 파일명</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>이미지 MIME 타입</summary>
    public string ContentType { get; set; } = "image/jpeg";

    /// <summary>처리 후 이미지 삭제 여부</summary>
    public bool DeleteAfterProcessing { get; set; } = true;
}
