using SideReport.Application.Ai.Results;
using SideReport.Application.Interfaces;

namespace SideReport.Infrastructure.Services;

/// <summary>
/// ANTHROPIC_API_KEY 미설정 시 사용하는 No-Op AI 분석 서비스
/// </summary>
public class NoOpAiAnalysisService : IAiAnalysisService
{
    public Task<AiAnalysisResult> AnalyzeAsync(
        IReadOnlyList<string> drugNames,
        IReadOnlyList<string> symptoms,
        CancellationToken ct = default)
    {
        throw new InvalidOperationException("AI 분석 서비스가 설정되지 않았습니다. ANTHROPIC_API_KEY 환경 변수를 설정하세요.");
    }
}
