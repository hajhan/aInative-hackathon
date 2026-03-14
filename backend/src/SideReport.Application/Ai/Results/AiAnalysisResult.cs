namespace SideReport.Application.Ai.Results;

/// <summary>
/// AI 분석 결과 DTO
/// </summary>
public record AiAnalysisResult(
    List<string> OfficialSideEffects,
    List<string> PossibleInteractions,
    string Summary,
    string Recommendation
);
