"use client";

import { useEffect } from "react";
import { useForm } from "react-hook-form";
import type { ParsedDrugItem } from "@/lib/types/ocr";

interface Props {
  drug: ParsedDrugItem;
  onSave: (updated: ParsedDrugItem) => void;
  onClose: () => void;
}

type FormData = {
  drugName: string;
  dosage: string;
  frequency: string;
};

export default function DrugEditModal({ drug, onSave, onClose }: Props) {
  const { register, handleSubmit, reset } = useForm<FormData>({
    defaultValues: {
      drugName: drug.officialName ?? drug.drugName,
      dosage: drug.dosage ?? "",
      frequency: drug.frequency ?? "",
    },
  });

  useEffect(() => {
    reset({
      drugName: drug.officialName ?? drug.drugName,
      dosage: drug.dosage ?? "",
      frequency: drug.frequency ?? "",
    });
  }, [drug, reset]);

  const onSubmit = (data: FormData) => {
    onSave({
      ...drug,
      drugName: data.drugName,
      officialName: data.drugName,
      dosage: data.dosage || null,
      frequency: data.frequency || null,
    });
    onClose();
  };

  return (
    <div
      className="fixed inset-0 z-50 flex items-end justify-center bg-black/40 px-4 pb-4"
      onClick={(e) => e.target === e.currentTarget && onClose()}
    >
      <div className="w-full max-w-md bg-white rounded-2xl p-6 shadow-xl">
        <h2 className="text-lg font-bold text-gray-900 mb-4">약품 정보 수정</h2>
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">약품명</label>
            <input
              {...register("drugName", { required: true })}
              className="w-full border border-gray-300 rounded-lg px-4 py-3 text-base focus:outline-none focus:ring-2 focus:ring-blue-500"
              placeholder="예: 타이레놀정500mg"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">용량</label>
            <input
              {...register("dosage")}
              className="w-full border border-gray-300 rounded-lg px-4 py-3 text-base focus:outline-none focus:ring-2 focus:ring-blue-500"
              placeholder="예: 500mg"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">복용 횟수</label>
            <input
              {...register("frequency")}
              className="w-full border border-gray-300 rounded-lg px-4 py-3 text-base focus:outline-none focus:ring-2 focus:ring-blue-500"
              placeholder="예: 1일 3회"
            />
          </div>
          <div className="flex gap-3 pt-2">
            <button type="button" onClick={onClose} className="btn-secondary flex-1">
              취소
            </button>
            <button type="submit" className="btn-primary flex-1">
              저장
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
