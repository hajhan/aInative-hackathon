using System.Text.Json;
using Anthropic.SDK;
using Anthropic.SDK.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SideReport.Application.Ai.Results;
using SideReport.Application.Interfaces;

namespace SideReport.Infrastructure.Services;

/// <summary>
/// Claude AI 기반 약물 상호작용 및 부작용 분석 서비스
/// </summary>
public class ClaudeAiAnalysisService : IAiAnalysisService
{
    private readonly AnthropicClient _client;
    private readonly ILogger<ClaudeAiAnalysisService> _logger;

    private const string Model = "claude-haiku-4-5-20251001";

    public ClaudeAiAnalysisService(IConfiguration configuration, ILogger<ClaudeAiAnalysisService> logger)
    {
        var apiKey = configuration["ANTHROPIC_API_KEY"]
            ?? throw new InvalidOperationException("ANTHROPIC_API_KEY 환경 변수가 설정되지 않았습니다.");
        _client = new AnthropicClient(apiKey);
        _logger = logger;
    }

    public async Task<AiAnalysisResult> AnalyzeAsync(
        IReadOnlyList<string> drugNames,
        IReadOnlyList<string> symptoms,
        CancellationToken ct = default)
    {
        var prompt = BuildPrompt(drugNames, symptoms);

        try
        {
            var request = new MessageParameters
            {
                Model = Model,
                MaxTokens = 1024,
                Messages =
                [
                    new Message
                    {
                        Role = RoleType.User,
                        Content = [new TextContent { Text = prompt }]
                    }
                ]
            };

            var response = await _client.Messages.GetClaudeMessageAsync(request, ct);
            var text = response.Content.OfType<TextContent>().FirstOrDefault()?.Text ?? "{}";

            return ParseResponse(text);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Claude AI 분석 중 오류 발생");
            return new AiAnalysisResult([], [], "AI 분석을 완료할 수 없습니다.", "전문의 상담을 권장합니다.");
        }
    }

    private static string BuildPrompt(IReadOnlyList<string> drugNames, IReadOnlyList<string> symptoms)
    {
        var drugs = string.Join(", ", drugNames);
        var syms = string.Join(", ", symptoms);

        return $$"""
            당신은 약물 부작용 분석 전문가입니다. 다음 정보를 분석하고 JSON으로만 응답하세요.

            복용 중인 약: {{drugs}}
            보고된 증상: {{syms}}

            다음 JSON 형식으로만 응답하세요 (설명 텍스트 없이):
            {
              "officialSideEffects": ["해당 약물의 공식 부작용 목록 (보고된 증상 중 해당하는 것)"],
              "possibleInteractions": ["약물 간 상호작용 설명 (해당하는 경우)"],
              "summary": "전반적인 상황 요약 (1-2문장, 한국어)",
              "recommendation": "권고 사항 (1문장, 한국어, 예: 즉시 의사 상담 권장 또는 경과 관찰 권장)"
            }
            """;
    }

    private static AiAnalysisResult ParseResponse(string text)
    {
        try
        {
            // JSON 블록 추출 (```json ... ``` 형식 대응)
            var start = text.IndexOf('{');
            var end = text.LastIndexOf('}');
            if (start >= 0 && end > start)
                text = text[start..(end + 1)];

            var doc = JsonDocument.Parse(text);
            var root = doc.RootElement;

            var officialSideEffects = root.TryGetProperty("officialSideEffects", out var ose)
                ? ose.EnumerateArray().Select(e => e.GetString() ?? "").Where(s => s.Length > 0).ToList()
                : [];

            var possibleInteractions = root.TryGetProperty("possibleInteractions", out var pi)
                ? pi.EnumerateArray().Select(e => e.GetString() ?? "").Where(s => s.Length > 0).ToList()
                : [];

            var summary = root.TryGetProperty("summary", out var s) ? s.GetString() ?? "" : "";
            var recommendation = root.TryGetProperty("recommendation", out var r) ? r.GetString() ?? "" : "";

            return new AiAnalysisResult(officialSideEffects, possibleInteractions, summary, recommendation);
        }
        catch
        {
            return new AiAnalysisResult([], [], "분석 결과를 파싱할 수 없습니다.", "전문의 상담을 권장합니다.");
        }
    }
}
