namespace SideReport.Domain.Entities;

/// <summary>
/// OCR 이미지 엔티티
/// </summary>
public class OcrImage
{
    /// <summary>고유 ID</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>업로드 사용자 ID</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>Blob Storage / S3 URL</summary>
    public string StorageUrl { get; set; } = string.Empty;

    /// <summary>OCR 처리 완료 시각 (미완료 시 null)</summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>처리 후 즉시 삭제 여부 (개인정보 보호)</summary>
    public bool DeleteAfterProcessing { get; set; } = true;

    /// <summary>업로드 시각</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

}
