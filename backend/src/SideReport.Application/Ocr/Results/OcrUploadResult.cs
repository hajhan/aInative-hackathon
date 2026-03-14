using SideReport.Domain.Enums;

namespace SideReport.Application.Ocr.Results;

/// <summary>
/// OCR 업로드 및 처리 결과
/// </summary>
public class OcrUploadResult
{
    /// <summary>OCR 이미지 레코드 ID</summary>
    public Guid Id { get; set; }

    /// <summary>처리 상태</summary>
    public OcrStatus Status { get; set; }

    /// <summary>OCR 추출 원문 텍스트</summary>
    public string? RawText { get; set; }

    /// <summary>파싱된 약품 목록</summary>
    public List<ParsedDrugItem> Drugs { get; set; } = [];

    /// <summary>처리 완료 시각</summary>
    public DateTime? ProcessedAt { get; set; }
}
