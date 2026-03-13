using System.Text.RegularExpressions;
using SideReport.Application.Interfaces;
using SideReport.Application.Ocr.Results;

namespace SideReport.Infrastructure.Services;

/// <summary>
/// 정규식 기반 약봉투 텍스트 파서
/// </summary>
public partial class DrugTextParserService : IDrugParserService
{
    // 약품명 패턴: 한글/영문 + 약품 접미사 (선택)
    [GeneratedRegex(
        @"(?<name>[가-힣a-zA-Z]+(?:정|캡슐|시럽|주사|연고|크림|액|겔|산|주|mg|MG)?\s*(?:\d+mg)?)",
        RegexOptions.Compiled)]
    private static partial Regex DrugNamePattern();

    // 용량 패턴
    [GeneratedRegex(
        @"(?<dosage>\d+(?:\.\d+)?(?:mg|g|ml|mcg|밀리그램|그램|미리그램))",
        RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex DosagePattern();

    // 복용횟수 패턴
    [GeneratedRegex(
        @"(?<freq>1일\s*\d+회|하루\s*\d+번|하루에\s*\d+번|매일\s*\d+회|\d+회/일)",
        RegexOptions.Compiled)]
    private static partial Regex FrequencyPattern();

    // 약품 라인 전체 패턴 (번호 + 약품명 + 용량 + 횟수)
    [GeneratedRegex(
        @"(?:^\s*\d+[.)]\s*|^\s*[-·•]\s*)?(?<name>[가-힣a-zA-Z][가-힣a-zA-Z\s]*?(?:정|캡슐|시럽|주사|연고|크림|액|겔|산)?)\s+(?<dosage>\d+(?:\.\d+)?(?:mg|g|ml|mcg))\s+(?<freq>1일\s*\d+회|하루\s*\d+번|\d+회/일)?",
        RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase)]
    private static partial Regex DrugLinePattern();

    public List<ParsedDrugItem> ParseOcrText(string rawOcrText)
    {
        if (string.IsNullOrWhiteSpace(rawOcrText))
            return [];

        var results = new List<ParsedDrugItem>();
        var seenDrugs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // 라인별 파싱 시도 (구조화된 약봉투)
        var lineMatches = DrugLinePattern().Matches(rawOcrText);
        foreach (Match m in lineMatches)
        {
            var drugName = m.Groups["name"].Value.Trim();
            if (string.IsNullOrWhiteSpace(drugName) || drugName.Length < 2)
                continue;
            if (!seenDrugs.Add(drugName))
                continue;

            results.Add(new ParsedDrugItem
            {
                DrugName = drugName,
                Dosage = m.Groups["dosage"].Success ? m.Groups["dosage"].Value.Trim() : null,
                Frequency = m.Groups["freq"].Success ? m.Groups["freq"].Value.Trim() : null
            });
        }

        // 라인 파싱 실패 시 폴백: 줄 단위 개별 파싱
        if (results.Count == 0)
        {
            foreach (var line in rawOcrText.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                var trimmed = line.Trim();
                if (trimmed.Length < 3 || IsHeaderOrFooterLine(trimmed))
                    continue;

                var dosageMatch = DosagePattern().Match(trimmed);
                var freqMatch = FrequencyPattern().Match(trimmed);

                // 용량이 있는 줄만 약품으로 간주
                if (!dosageMatch.Success) continue;

                // 약품명: 용량 앞부분 추출
                var nameCandidate = trimmed[..dosageMatch.Index].Trim();
                nameCandidate = Regex.Replace(nameCandidate, @"^\s*\d+[.)]\s*", "").Trim();

                if (string.IsNullOrWhiteSpace(nameCandidate) || nameCandidate.Length < 2)
                    continue;
                if (!seenDrugs.Add(nameCandidate))
                    continue;

                results.Add(new ParsedDrugItem
                {
                    DrugName = nameCandidate,
                    Dosage = dosageMatch.Value,
                    Frequency = freqMatch.Success ? freqMatch.Value : null
                });
            }
        }

        return results;
    }

    private static bool IsHeaderOrFooterLine(string line)
    {
        var keywords = new[] { "처방전", "환자명", "처방일", "복약기간", "조제", "약국", "의원", "병원", "[" };
        return keywords.Any(k => line.Contains(k));
    }
}
