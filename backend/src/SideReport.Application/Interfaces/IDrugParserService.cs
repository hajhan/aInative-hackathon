using SideReport.Application.Ocr.Results;

namespace SideReport.Application.Interfaces;

/// <summary>
/// OCR 텍스트에서 약품 정보를 파싱하는 서비스 인터페이스
/// </summary>
public interface IDrugParserService
{
    /// <summary>
    /// OCR 원문 텍스트에서 약품 목록을 파싱합니다.
    /// </summary>
    /// <param name="rawOcrText">OCR 추출 원문 텍스트</param>
    /// <returns>파싱된 약품 항목 목록</returns>
    List<ParsedDrugItem> ParseOcrText(string rawOcrText);
}
