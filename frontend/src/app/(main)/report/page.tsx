"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useMedicationStore } from "@/store/medicationStore";
import { useReportStore } from "@/store/reportStore";
import { Severity, AiAnalysisResult } from "@/lib/types/report";

type Step = 1 | 2 | 3 | 4;

const SEVERITY_OPTIONS: { value: Severity; label: string; color: string }[] = [
  { value: "Mild", label: "경미함", color: "border-green-400 bg-green-50" },
  { value: "Moderate", label: "보통", color: "border-yellow-400 bg-yellow-50" },
  { value: "Severe", label: "심각함", color: "border-red-400 bg-red-50" },
];

const COMMON_SYMPTOMS = [
  "두통", "어지러움", "메스꺼움", "구토", "설사", "복통",
  "발진", "가려움", "두드러기", "호흡 곤란", "부종", "피로감",
];

export default function ReportPage() {
  const router = useRouter();
  const { medications, fetchMedications } = useMedicationStore();
  const { createReport, analyzeWithAi } = useReportStore();

  const [step, setStep] = useState<Step>(1);
  const [selectedMedIds, setSelectedMedIds] = useState<string[]>([]);
  const [symptoms, setSymptoms] = useState<string[]>([]);
  const [customSymptom, setCustomSymptom] = useState("");
  const [severity, setSeverity] = useState<Severity>("Mild");
  const [notes, setNotes] = useState("");
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [aiResult, setAiResult] = useState<AiAnalysisResult | null>(null);
  const [analyzing, setAnalyzing] = useState(false);

  useEffect(() => {
    fetchMedications();
  }, [fetchMedications]);

  const toggleMed = (id: string) => {
    setSelectedMedIds((prev) =>
      prev.includes(id) ? prev.filter((x) => x !== id) : [...prev, id]
    );
  };

  const toggleSymptom = (s: string) => {
    setSymptoms((prev) =>
      prev.includes(s) ? prev.filter((x) => x !== s) : [...prev, s]
    );
  };

  const addCustomSymptom = () => {
    const trimmed = customSymptom.trim();
    if (trimmed && !symptoms.includes(trimmed)) {
      setSymptoms((prev) => [...prev, trimmed]);
    }
    setCustomSymptom("");
  };

  const handleAnalyze = async () => {
    const drugNames = medications
      .filter((m) => selectedMedIds.includes(m.id))
      .map((m) => m.drugName);
    if (drugNames.length === 0 || symptoms.length === 0) return;

    setAnalyzing(true);
    setAiResult(null);
    try {
      const result = await analyzeWithAi(drugNames, symptoms);
      setAiResult(result);
    } catch {
      // 분석 실패 시 사용자에게 안내 (보고서 제출은 계속 가능)
      setAiResult({
        officialSideEffects: [],
        possibleInteractions: [],
        summary: "AI 분석을 완료할 수 없습니다.",
        recommendation: "전문의 상담을 권장합니다.",
      });
    } finally {
      setAnalyzing(false);
    }
  };

  const handleSubmit = async () => {
    if (symptoms.length === 0) { setError("증상을 하나 이상 선택해 주세요."); return; }
    setSubmitting(true);
    setError(null);
    try {
      await createReport({
        medicationIds: selectedMedIds,
        symptoms,
        severity,
        notes: notes.trim() || undefined,
      });
      router.push("/reports");
    } catch {
      setError("보고서 제출 중 오류가 발생했습니다.");
      setSubmitting(false);
    }
  };

  return (
    <div className="px-4 py-6 max-w-md mx-auto">
      {/* 진행 표시 */}
      <div className="flex items-center mb-6">
        {([1, 2, 3, 4] as Step[]).map((s, i) => (
          <div key={s} className="flex items-center flex-1">
            <div
              className={`w-8 h-8 rounded-full flex items-center justify-center text-sm font-bold transition-colors ${
                step >= s ? "bg-blue-600 text-white" : "bg-gray-200 text-gray-500"
              }`}
            >
              {s}
            </div>
            {i < 3 && (
              <div className={`flex-1 h-1 ${step > s ? "bg-blue-600" : "bg-gray-200"}`} />
            )}
          </div>
        ))}
      </div>

      {/* Step 1: 복용약 선택 */}
      {step === 1 && (
        <div>
          <h2 className="text-xl font-bold text-gray-900 mb-1">복용 중인 약 선택</h2>
          <p className="text-sm text-gray-600 mb-4">부작용이 의심되는 약을 선택하세요. (선택 안 해도 됩니다)</p>

          {medications.length === 0 ? (
            <div className="text-center py-8">
              <p className="text-gray-500 mb-3">등록된 복용약이 없습니다.</p>
              <Link href="/medications" className="text-blue-600 text-sm underline">
                복용약 먼저 등록하기
              </Link>
            </div>
          ) : (
            <div className="space-y-2 mb-6">
              {medications.map((med) => (
                <button
                  key={med.id}
                  onClick={() => toggleMed(med.id)}
                  className={`w-full text-left p-4 rounded-xl border-2 transition-colors ${
                    selectedMedIds.includes(med.id)
                      ? "border-blue-500 bg-blue-50"
                      : "border-gray-200 bg-white"
                  }`}
                >
                  <span className="font-medium text-gray-900">{med.drugName}</span>
                  {med.frequency && (
                    <span className="ml-2 text-sm text-gray-500">{med.frequency}</span>
                  )}
                </button>
              ))}
            </div>
          )}

          <button onClick={() => setStep(2)} className="btn-primary w-full">
            다음
          </button>
        </div>
      )}

      {/* Step 2: 증상 입력 */}
      {step === 2 && (
        <div>
          <h2 className="text-xl font-bold text-gray-900 mb-1">증상 선택</h2>
          <p className="text-sm text-gray-600 mb-4">경험한 증상을 선택하거나 직접 입력하세요.</p>

          <div className="flex flex-wrap gap-2 mb-4">
            {COMMON_SYMPTOMS.map((s) => (
              <button
                key={s}
                onClick={() => toggleSymptom(s)}
                className={`px-3 py-2 rounded-full text-sm border-2 transition-colors min-h-[44px] ${
                  symptoms.includes(s)
                    ? "border-blue-500 bg-blue-50 text-blue-700"
                    : "border-gray-200 bg-white text-gray-700"
                }`}
              >
                {s}
              </button>
            ))}
          </div>

          {/* 직접 입력 */}
          <div className="flex gap-2 mb-4">
            <input
              type="text"
              value={customSymptom}
              onChange={(e) => setCustomSymptom(e.target.value)}
              onKeyDown={(e) => e.key === "Enter" && (e.preventDefault(), addCustomSymptom())}
              placeholder="직접 입력 후 Enter"
              className="input-field flex-1"
            />
            <button onClick={addCustomSymptom} className="btn-secondary px-4">
              추가
            </button>
          </div>

          {symptoms.length > 0 && (
            <div className="bg-blue-50 rounded-xl p-3 mb-4">
              <p className="text-sm font-medium text-blue-800 mb-2">선택된 증상</p>
              <div className="flex flex-wrap gap-2">
                {symptoms.map((s) => (
                  <span
                    key={s}
                    className="px-3 py-1 bg-blue-100 text-blue-800 rounded-full text-sm flex items-center gap-1"
                  >
                    {s}
                    <button onClick={() => toggleSymptom(s)} className="text-blue-500 hover:text-blue-700">
                      ×
                    </button>
                  </span>
                ))}
              </div>
            </div>
          )}

          <div className="flex gap-3">
            <button onClick={() => setStep(1)} className="btn-secondary flex-1">
              이전
            </button>
            <button
              onClick={() => { if (symptoms.length > 0) setStep(3); else setError("증상을 선택해 주세요."); }}
              className="btn-primary flex-1"
            >
              다음
            </button>
          </div>
          {error && <p className="text-sm text-red-600 mt-2">{error}</p>}
        </div>
      )}

      {/* Step 3: 심각도 */}
      {step === 3 && (
        <div>
          <h2 className="text-xl font-bold text-gray-900 mb-1">심각도 선택</h2>
          <p className="text-sm text-gray-600 mb-4">전반적인 증상의 심각도를 선택하세요.</p>

          <div className="space-y-3 mb-6">
            {SEVERITY_OPTIONS.map((opt) => (
              <button
                key={opt.value}
                onClick={() => setSeverity(opt.value)}
                className={`w-full p-4 rounded-xl border-2 text-left transition-colors ${
                  severity === opt.value ? opt.color + " border-2" : "border-gray-200 bg-white"
                }`}
              >
                <span className="font-semibold text-gray-900">{opt.label}</span>
              </button>
            ))}
          </div>

          <div className="flex gap-3">
            <button onClick={() => setStep(2)} className="btn-secondary flex-1">
              이전
            </button>
            <button onClick={() => setStep(4)} className="btn-primary flex-1">
              다음
            </button>
          </div>
        </div>
      )}

      {/* Step 4: 메모 + AI 분석 + 제출 */}
      {step === 4 && (
        <div>
          <h2 className="text-xl font-bold text-gray-900 mb-1">추가 메모 및 제출</h2>
          <p className="text-sm text-gray-600 mb-4">추가로 전달할 내용이 있으면 입력하세요.</p>

          <textarea
            value={notes}
            onChange={(e) => setNotes(e.target.value)}
            placeholder="예: 약 복용 후 1시간 뒤 증상 발생"
            rows={3}
            className="input-field w-full mb-4 resize-none"
          />

          {/* AI 분석 버튼 */}
          {selectedMedIds.length > 0 && (
            <div className="mb-4">
              <button
                onClick={handleAnalyze}
                disabled={analyzing}
                className="btn-secondary w-full"
              >
                {analyzing ? "AI 분석 중..." : "🤖 AI 약물 분석"}
              </button>

              {aiResult && (
                <div className="mt-3 bg-purple-50 rounded-xl p-4 space-y-3">
                  <h3 className="font-bold text-purple-900">AI 분석 결과</h3>
                  {aiResult.officialSideEffects.length > 0 && (
                    <div>
                      <p className="text-sm font-medium text-purple-800">공식 부작용</p>
                      <ul className="text-sm text-purple-700 list-disc list-inside">
                        {aiResult.officialSideEffects.map((e) => <li key={e}>{e}</li>)}
                      </ul>
                    </div>
                  )}
                  {aiResult.possibleInteractions.length > 0 && (
                    <div>
                      <p className="text-sm font-medium text-purple-800">약물 상호작용</p>
                      <ul className="text-sm text-purple-700 list-disc list-inside">
                        {aiResult.possibleInteractions.map((e) => <li key={e}>{e}</li>)}
                      </ul>
                    </div>
                  )}
                  <div>
                    <p className="text-sm text-purple-800">{aiResult.summary}</p>
                    <p className="text-sm font-medium text-purple-900 mt-1">💡 {aiResult.recommendation}</p>
                  </div>
                </div>
              )}
            </div>
          )}

          {error && <p className="text-sm text-red-600 mb-3">{error}</p>}

          <div className="flex gap-3">
            <button onClick={() => setStep(3)} className="btn-secondary flex-1" disabled={submitting}>
              이전
            </button>
            <button onClick={handleSubmit} className="btn-primary flex-1" disabled={submitting}>
              {submitting ? "제출 중..." : "보고서 제출"}
            </button>
          </div>
        </div>
      )}
    </div>
  );
}
