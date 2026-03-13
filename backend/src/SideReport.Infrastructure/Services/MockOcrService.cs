using SideReport.Application.Interfaces;

namespace SideReport.Infrastructure.Services;

/// <summary>
/// Mock OCR 서비스 — 개발/테스트 환경에서 샘플 약봉투 텍스트 반환
/// </summary>
public class MockOcrService : IOcrService
{
    private static readonly string[] SampleTexts =
    [
        """
        [처방전]
        환자명: 홍길동
        처방일: 2024-01-15

        1. 타이레놀정500mg 500mg 1일 3회 식후 30분
        2. 아목시실린캡슐250mg 250mg 1일 2회 아침저녁
        3. 위염에 좋은 알마겔정 1일 3회 식전
        4. 비타민C정500mg 500mg 1일 1회

        복약기간: 7일분
        """,
        """
        처방의약품

        세티리진정10mg 10mg 1일 1회 취침전
        판토프라졸정40mg 40mg 1일 1회 아침식전
        이부프로펜정400mg 400mg 1일 3회 식후

        조제약국: 행복약국
        """,
        """
        복약지도

        1) 아스피린장용정100mg - 하루 1번 아침
        2) 메트포르민정500mg - 하루 2번 아침저녁 식후
        3) 아토르바스타틴정20mg - 하루 1번 저녁
        4) 암로디핀정5mg - 하루 1번 아침
        """
    ];

    public Task<OcrTextResult> ExtractTextAsync(Stream imageStream, string mimeType)
    {
        // 스트림 길이로 샘플 선택 (결정론적 Mock)
        var index = (int)(imageStream.Length % SampleTexts.Length);
        var text = SampleTexts[index];
        return Task.FromResult(new OcrTextResult(text, true));
    }
}
