namespace SideReport.Application.Ocr.Results;

/// <summary>
/// 식약처 조회 약품 정보
/// </summary>
public record DrugInfo(
    string OfficialName,
    string? Ingredient,
    string? Efficacy,
    string? SideEffects
);
