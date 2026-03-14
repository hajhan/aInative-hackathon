"use client";

import { useEffect, useState } from "react";
import { useMedicationStore } from "@/store/medicationStore";
import { MedicationResult, CreateMedicationRequest } from "@/lib/types/medication";

export default function MedicationsPage() {
  const { medications, loading, fetchMedications, addMedication, updateMedication, deleteMedication } =
    useMedicationStore();

  const [showForm, setShowForm] = useState(false);
  const [editing, setEditing] = useState<MedicationResult | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [form, setForm] = useState<CreateMedicationRequest>({
    drugName: "",
    dosage: "",
    frequency: "",
    startDate: new Date().toISOString().slice(0, 10),
    endDate: "",
  });

  useEffect(() => {
    fetchMedications();
  }, [fetchMedications]);

  const openAdd = () => {
    setEditing(null);
    setForm({ drugName: "", dosage: "", frequency: "", startDate: new Date().toISOString().slice(0, 10), endDate: "" });
    setShowForm(true);
    setError(null);
  };

  const openEdit = (med: MedicationResult) => {
    setEditing(med);
    setForm({
      drugName: med.drugName,
      dosage: med.dosage ?? "",
      frequency: med.frequency ?? "",
      startDate: med.startDate,
      endDate: med.endDate ?? "",
    });
    setShowForm(true);
    setError(null);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!form.drugName.trim()) { setError("약품명을 입력해 주세요."); return; }
    setSubmitting(true);
    setError(null);
    try {
      const payload: CreateMedicationRequest = {
        drugName: form.drugName.trim(),
        dosage: form.dosage?.trim() || undefined,
        frequency: form.frequency?.trim() || undefined,
        startDate: form.startDate,
        endDate: form.endDate?.trim() || undefined,
      };
      if (editing) {
        await updateMedication(editing.id, payload);
      } else {
        await addMedication(payload);
      }
      setShowForm(false);
    } catch {
      setError("저장 중 오류가 발생했습니다.");
    } finally {
      setSubmitting(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (!confirm("정말 삭제하시겠습니까?")) return;
    try {
      await deleteMedication(id);
    } catch {
      alert("삭제 중 오류가 발생했습니다.");
    }
  };

  return (
    <div className="px-4 py-6 max-w-md mx-auto">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900">내 복용약</h1>
        <button onClick={openAdd} className="btn-primary text-sm px-4 py-2">
          + 추가
        </button>
      </div>

      {loading && <p className="text-center text-gray-500 py-8">불러오는 중...</p>}

      {!loading && medications.length === 0 && (
        <div className="text-center py-12">
          <p className="text-gray-500 text-base mb-4">등록된 복용약이 없습니다.</p>
          <button onClick={openAdd} className="btn-secondary">
            첫 번째 약 등록하기
          </button>
        </div>
      )}

      <div className="space-y-3">
        {medications.map((med) => (
          <div key={med.id} className="card">
            <div className="flex items-start justify-between">
              <div className="flex-1">
                <h3 className="text-lg font-semibold text-gray-900">{med.drugName}</h3>
                {med.dosage && <p className="text-sm text-gray-600">용량: {med.dosage}</p>}
                {med.frequency && <p className="text-sm text-gray-600">복용: {med.frequency}</p>}
                <p className="text-sm text-gray-500 mt-1">
                  {med.startDate} ~ {med.endDate ?? "복용 중"}
                </p>
              </div>
              <div className="flex gap-2 ml-3">
                <button
                  onClick={() => openEdit(med)}
                  className="text-sm text-blue-600 hover:text-blue-800 min-w-[44px] min-h-[44px] flex items-center justify-center"
                >
                  수정
                </button>
                <button
                  onClick={() => handleDelete(med.id)}
                  className="text-sm text-red-500 hover:text-red-700 min-w-[44px] min-h-[44px] flex items-center justify-center"
                >
                  삭제
                </button>
              </div>
            </div>
          </div>
        ))}
      </div>

      {/* 추가/수정 폼 모달 */}
      {showForm && (
        <div className="fixed inset-0 bg-black/50 z-50 flex items-end justify-center">
          <div className="bg-white w-full max-w-md rounded-t-2xl p-6 pb-safe">
            <h2 className="text-xl font-bold text-gray-900 mb-4">
              {editing ? "복용약 수정" : "복용약 추가"}
            </h2>
            <form onSubmit={handleSubmit} className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  약품명 <span className="text-red-500">*</span>
                </label>
                <input
                  type="text"
                  value={form.drugName}
                  onChange={(e) => setForm({ ...form, drugName: e.target.value })}
                  placeholder="예: 타이레놀 500mg"
                  className="input-field"
                  required
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">용량</label>
                <input
                  type="text"
                  value={form.dosage}
                  onChange={(e) => setForm({ ...form, dosage: e.target.value })}
                  placeholder="예: 500mg"
                  className="input-field"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">복용 방법</label>
                <input
                  type="text"
                  value={form.frequency}
                  onChange={(e) => setForm({ ...form, frequency: e.target.value })}
                  placeholder="예: 1일 3회, 식후 30분"
                  className="input-field"
                />
              </div>
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    시작일 <span className="text-red-500">*</span>
                  </label>
                  <input
                    type="date"
                    value={form.startDate}
                    onChange={(e) => setForm({ ...form, startDate: e.target.value })}
                    className="input-field"
                    required
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">종료일</label>
                  <input
                    type="date"
                    value={form.endDate}
                    onChange={(e) => setForm({ ...form, endDate: e.target.value })}
                    className="input-field"
                  />
                </div>
              </div>

              {error && <p className="text-sm text-red-600">{error}</p>}

              <div className="flex gap-3 pt-2">
                <button
                  type="button"
                  onClick={() => setShowForm(false)}
                  className="btn-secondary flex-1"
                  disabled={submitting}
                >
                  취소
                </button>
                <button type="submit" className="btn-primary flex-1" disabled={submitting}>
                  {submitting ? "저장 중..." : "저장"}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
