using SideReport.Application.Ai.Results;

namespace SideReport.Application.Interfaces;

/// <summary>
/// AI 분석 서비스 인터페이스 (약물 상호작용, 부작용 판별)
/// </summary>
public interface IAiAnalysisService
{
    /// <summary>
    /// 복용약 목록과 증상으로 AI 분석 수행
    /// </summary>
    Task<AiAnalysisResult> AnalyzeAsync(
        IReadOnlyList<string> drugNames,
        IReadOnlyList<string> symptoms,
        CancellationToken ct = default);
}
