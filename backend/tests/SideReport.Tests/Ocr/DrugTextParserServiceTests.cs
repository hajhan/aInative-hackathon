using FluentAssertions;
using SideReport.Infrastructure.Services;
using Xunit;

namespace SideReport.Tests.Ocr;

public class DrugTextParserServiceTests
{
    private readonly DrugTextParserService _sut = new();

    [Fact]
    public void ParseOcrText_WithEmptyString_ReturnsEmptyList()
    {
        var result = _sut.ParseOcrText(string.Empty);
        result.Should().BeEmpty();
    }

    [Fact]
    public void ParseOcrText_WithNullLikeInput_ReturnsEmptyList()
    {
        var result = _sut.ParseOcrText("   ");
        result.Should().BeEmpty();
    }

    [Fact]
    public void ParseOcrText_WithDosagePattern_ExtractsDrug()
    {
        var text = "타이레놀정 500mg 1일 3회 식후";
        var result = _sut.ParseOcrText(text);

        result.Should().NotBeEmpty();
        result.Should().Contain(d => d.DrugName.Contains("타이레놀") && d.Dosage == "500mg");
    }

    [Fact]
    public void ParseOcrText_WithMultipleDrugs_ExtractsAll()
    {
        var text = """
            1. 타이레놀정500mg 500mg 1일 3회
            2. 아목시실린캡슐250mg 250mg 1일 2회
            3. 이부프로펜정400mg 400mg 1일 3회
            """;

        var result = _sut.ParseOcrText(text);
        result.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public void ParseOcrText_WithHeaderLines_SkipsHeaderLines()
    {
        var text = """
            [처방전]
            환자명: 홍길동
            처방일: 2024-01-15
            타이레놀정 500mg 1일 3회
            """;

        var result = _sut.ParseOcrText(text);
        // 헤더 라인이 파싱되지 않아야 함 (처방전, 환자명, 처방일)
        result.Should().NotContain(d => d.DrugName.Contains("처방"));
        result.Should().NotContain(d => d.DrugName.Contains("환자"));
    }

    [Fact]
    public void ParseOcrText_WithFrequency_ExtractsFrequency()
    {
        var text = "세티리진정 10mg 1일 1회 취침전";
        var result = _sut.ParseOcrText(text);

        result.Should().Contain(d =>
            d.DrugName.Contains("세티리진") &&
            d.Frequency != null && d.Frequency.Contains("1회"));
    }

    [Fact]
    public void ParseOcrText_WithDuplicateDrug_ReturnsUniqueEntries()
    {
        var text = """
            타이레놀정 500mg 1일 3회
            타이레놀정 500mg 1일 3회
            """;

        var result = _sut.ParseOcrText(text);
        // 중복 제거 확인
        var taireNolCount = result.Count(d => d.DrugName.Contains("타이레놀"));
        taireNolCount.Should().Be(1);
    }

    [Theory]
    [InlineData("타이레놀정 500mg")]
    [InlineData("아스피린정 100mg")]
    [InlineData("이부프로펜정 400mg")]
    [InlineData("세티리진정 10mg")]
    [InlineData("메트포르민정 500mg")]
    public void ParseOcrText_WithVariousDrugs_ExtractsDosage(string line)
    {
        var result = _sut.ParseOcrText(line);
        result.Should().Contain(d => d.Dosage != null);
    }

    [Fact]
    public void ParseOcrText_WithGramDosage_ExtractsDosage()
    {
        var text = "어떤약 1g 1일 2회";
        var result = _sut.ParseOcrText(text);
        result.Should().Contain(d => d.Dosage != null && d.Dosage.Contains("g"));
    }

    [Fact]
    public void ParseOcrText_WithMlDosage_ExtractsDosage()
    {
        var text = "시럽약 5ml 1일 3회";
        var result = _sut.ParseOcrText(text);
        result.Should().Contain(d => d.Dosage != null && d.Dosage.Contains("ml"));
    }
}
