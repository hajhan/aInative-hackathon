using SideReport.Application.Interfaces;
using SideReport.Application.Ocr.Results;

namespace SideReport.Infrastructure.Services;

/// <summary>
/// Mock 약품 정보 서비스 — 사전 정의된 주요 약품 데이터 반환
/// </summary>
public class MockDrugInfoService : IDrugInfoService
{
    private static readonly Dictionary<string, DrugInfo> PresetDrugs = new(StringComparer.OrdinalIgnoreCase)
    {
        ["타이레놀"] = new("타이레놀정500밀리그램(아세트아미노펜)", "아세트아미노펜 500mg",
            "두통, 치통, 근육통, 발열 완화", "간 독성(과량 복용 시), 피부 발진"),
        ["타이레놀정"] = new("타이레놀정500밀리그램(아세트아미노펜)", "아세트아미노펜 500mg",
            "두통, 치통, 근육통, 발열 완화", "간 독성(과량 복용 시), 피부 발진"),
        ["아스피린"] = new("아스피린프로텍트정100밀리그램", "아세틸살리실산 100mg",
            "혈전 예방, 해열, 진통, 소염", "위장 장애, 출혈, 이명"),
        ["아목시실린"] = new("아목시실린캡슐250밀리그램", "아목시실린 250mg",
            "세균성 감염증 치료", "설사, 구역, 피부 발진, 알레르기 반응"),
        ["아목시실린캡슐"] = new("아목시실린캡슐250밀리그램", "아목시실린 250mg",
            "세균성 감염증 치료", "설사, 구역, 피부 발진, 알레르기 반응"),
        ["이부프로펜"] = new("이부프로펜정400밀리그램", "이부프로펜 400mg",
            "소염, 진통, 해열", "위장 장애, 두통, 어지러움, 부종"),
        ["이부프로펜정"] = new("이부프로펜정400밀리그램", "이부프로펜 400mg",
            "소염, 진통, 해열", "위장 장애, 두통, 어지러움, 부종"),
        ["세티리진"] = new("세티리진염산염정10밀리그램", "세티리진염산염 10mg",
            "알레르기성 비염, 두드러기 치료", "졸음, 구강 건조, 두통"),
        ["세티리진정"] = new("세티리진염산염정10밀리그램", "세티리진염산염 10mg",
            "알레르기성 비염, 두드러기 치료", "졸음, 구강 건조, 두통"),
        ["메트포르민"] = new("메트포르민염산염정500밀리그램", "메트포르민염산염 500mg",
            "제2형 당뇨병 치료", "소화불량, 구역, 설사, 젖산산증(드물게)"),
        ["메트포르민정"] = new("메트포르민염산염정500밀리그램", "메트포르민염산염 500mg",
            "제2형 당뇨병 치료", "소화불량, 구역, 설사, 젖산산증(드물게)"),
        ["아토르바스타틴"] = new("아토르바스타틴칼슘정20밀리그램", "아토르바스타틴칼슘 20mg",
            "고지혈증, 심혈관질환 예방", "근육통, 간효소 상승, 소화불량"),
        ["아토르바스타틴정"] = new("아토르바스타틴칼슘정20밀리그램", "아토르바스타틴칼슘 20mg",
            "고지혈증, 심혈관질환 예방", "근육통, 간효소 상승, 소화불량"),
        ["암로디핀"] = new("암로디핀베실산염정5밀리그램", "암로디핀베실산염 5mg",
            "고혈압, 협심증 치료", "두통, 부종, 얼굴 홍조, 어지러움"),
        ["암로디핀정"] = new("암로디핀베실산염정5밀리그램", "암로디핀베실산염 5mg",
            "고혈압, 협심증 치료", "두통, 부종, 얼굴 홍조, 어지러움"),
        ["판토프라졸"] = new("판토프라졸나트륨정40밀리그램", "판토프라졸나트륨 40mg",
            "위식도역류질환, 소화성 궤양 치료", "두통, 설사, 복통, 구역"),
        ["판토프라졸정"] = new("판토프라졸나트륨정40밀리그램", "판토프라졸나트륨 40mg",
            "위식도역류질환, 소화성 궤양 치료", "두통, 설사, 복통, 구역"),
        ["알마겔"] = new("알마겔정", "수산화알루미늄겔·수산화마그네슘",
            "위산 중화, 소화성 궤양 증상 완화", "변비 또는 설사, 저인산혈증(장기복용)"),
        ["비타민C"] = new("비타민C정500밀리그램", "아스코르브산 500mg",
            "비타민C 보충, 괴혈병 예방 및 치료", "소화불량, 신장결석(고용량)"),
        ["비타민C정"] = new("비타민C정500밀리그램", "아스코르브산 500mg",
            "비타민C 보충, 괴혈병 예방 및 치료", "소화불량, 신장결석(고용량)")
    };

    public Task<DrugInfo?> LookupDrugAsync(string drugName)
    {
        var cleanName = drugName.Trim();

        // 정확히 일치
        if (PresetDrugs.TryGetValue(cleanName, out var exact))
            return Task.FromResult<DrugInfo?>(exact);

        // 부분 일치 (약품명이 키를 포함하는 경우)
        foreach (var (key, info) in PresetDrugs)
        {
            if (cleanName.Contains(key, StringComparison.OrdinalIgnoreCase) ||
                key.Contains(cleanName, StringComparison.OrdinalIgnoreCase))
                return Task.FromResult<DrugInfo?>(info);
        }

        return Task.FromResult<DrugInfo?>(null);
    }
}
