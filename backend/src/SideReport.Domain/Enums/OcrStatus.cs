namespace SideReport.Domain.Enums;

/// <summary>
/// OCR 처리 상태
/// </summary>
public enum OcrStatus
{
    /// <summary>업로드 완료, 처리 대기</summary>
    Pending,

    /// <summary>OCR 처리 중</summary>
    Processing,

    /// <summary>처리 완료</summary>
    Completed,

    /// <summary>처리 실패</summary>
    Failed
}
