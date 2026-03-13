using SideReport.Domain.Enums;

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

    /// <summary>로컬 저장 경로 또는 Blob URL</summary>
    public string StorageUrl { get; set; } = string.Empty;

    /// <summary>OCR 추출 원문 텍스트</summary>
    public string? RawOcrText { get; set; }

    /// <summary>파싱 결과 JSON (ParsedDrugItem 목록)</summary>
    public string? ParsedResultJson { get; set; }

    /// <summary>OCR 처리 상태</summary>
    public OcrStatus Status { get; set; } = OcrStatus.Pending;

    /// <summary>OCR 처리 완료 시각 (미완료 시 null)</summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>처리 후 즉시 삭제 여부 (개인정보 보호)</summary>
    public bool DeleteAfterProcessing { get; set; } = true;

    /// <summary>업로드 시각</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
