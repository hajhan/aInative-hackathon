using SideReport.Application.Ocr.Results;

namespace SideReport.Application.Interfaces;

/// <summary>
/// 식약처 약품 정보 조회 서비스 인터페이스
/// </summary>
public interface IDrugInfoService
{
    /// <summary>
    /// 약품명으로 공식 약품 정보를 조회합니다. 캐시 우선 조회.
    /// </summary>
    /// <param name="drugName">OCR 파싱된 약품명</param>
    /// <returns>약품 정보 (없으면 null)</returns>
    Task<DrugInfo?> LookupDrugAsync(string drugName);
}
