namespace SideReport.Application.Interfaces;

/// <summary>
/// 이미지 저장소 인터페이스 (로컬 파일시스템 또는 클라우드 스토리지)
/// </summary>
public interface IImageStorageService
{
    /// <summary>
    /// 이미지를 저장하고 접근 경로를 반환합니다.
    /// </summary>
    /// <param name="imageStream">업로드할 이미지 스트림</param>
    /// <param name="fileName">저장 파일명</param>
    /// <returns>저장된 파일 경로 또는 URL</returns>
    Task<string> UploadAsync(Stream imageStream, string fileName);

    /// <summary>
    /// 저장된 이미지를 삭제합니다.
    /// </summary>
    /// <param name="storagePath">삭제할 파일 경로</param>
    Task DeleteAsync(string storagePath);
}
