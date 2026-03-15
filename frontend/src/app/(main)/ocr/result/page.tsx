"use client";

import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { useOcrStore } from "@/store/ocrStore";
import OcrResultCard from "@/components/ocr/OcrResultCard";
import DrugEditModal from "@/components/ocr/DrugEditModal";
import ManualDrugForm from "@/components/ocr/ManualDrugForm";
import { apiClient } from "@/lib/apiClient";
import type { ParsedDrugItem } from "@/lib/types/ocr";

export default function OcrResultPage() {
  const router = useRouter();
  const { result, returnPath, updateDrug, removeDrug, addDrug, reset } = useOcrStore();
  const [editingIndex, setEditingIndex] = useState<number | null>(null);
  const [isSaving, setIsSaving] = useState(false);
  const [saveError, setSaveError] = useState<string | null>(null);

  // 렌더 단계가 아닌 effect에서 리다이렉트 처리 (React 18 Strict Mode 대응)
  useEffect(() => {
    if (!result) router.replace("/ocr");
  }, [result, router]);

  if (!result) return null;

  const handleSave = async () => {
    if (result.drugs.length === 0) {
      setSaveError("저장할 약품이 없습니다. 약품을 추가해 주세요.");
      return;
    }
    setIsSaving(true);
    setSaveError(null);
    try {
      await apiClient.post(`/api/ocr/${result.id}/confirm`, result.drugs);
      const destination = returnPath;
      router.push(destination); // reset() 이전 호출 — reset 후 useEffect가 /ocr로 리다이렉트하는 경합 방지
      reset();
    } catch {
      setSaveError("저장 중 오류가 발생했습니다. 다시 시도해 주세요.");
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <div className="px-4 py-6">
      <div className="max-w-md mx-auto">
        <div className="mb-6">
          <h1 className="text-2xl font-bold text-gray-900">인식 결과</h1>
          <p className="mt-1 text-base text-gray-600">
            인식된 약품 목록을 확인하고 필요하면 수정하세요.
          </p>
        </div>

        {result.drugs.length === 0 ? (
          <div className="card text-center text-gray-500 py-8">
            <p className="text-base">인식된 약품이 없습니다.</p>
            <p className="text-sm mt-1">아래에서 직접 약품을 추가할 수 있어요.</p>
          </div>
        ) : (
          <div className="space-y-3 mb-6">
            {result.drugs.map((drug, i) => (
              <OcrResultCard
                key={`${drug.drugName}-${i}`}
                drug={drug}
                index={i}
                onEdit={() => setEditingIndex(i)}
                onRemove={() => removeDrug(i)}
              />
            ))}
          </div>
        )}

        <ManualDrugForm onAdd={addDrug} />

        {saveError && (
          <p className="mt-3 text-sm text-red-500 text-center">{saveError}</p>
        )}

        <div className="mt-6 space-y-3">
          <button
            onClick={handleSave}
            disabled={isSaving}
            className="btn-primary w-full text-lg disabled:opacity-50"
          >
            {isSaving ? "저장 중..." : `약품 ${result.drugs.length}개 저장하기`}
          </button>
          <button
            onClick={() => { reset(); router.push("/ocr"); }}
            className="btn-secondary w-full"
          >
            다시 촬영하기
          </button>
        </div>
      </div>

      {editingIndex !== null && (
        <DrugEditModal
          drug={result.drugs[editingIndex]}
          onSave={(updated: ParsedDrugItem) => updateDrug(editingIndex, updated)}
          onClose={() => setEditingIndex(null)}
        />
      )}
    </div>
  );
}
