"use client";

import { useForm } from "react-hook-form";
import type { ParsedDrugItem } from "@/lib/types/ocr";

interface Props {
  onAdd: (drug: ParsedDrugItem) => void;
}

type FormData = {
  drugName: string;
  dosage: string;
  frequency: string;
};

export default function ManualDrugForm({ onAdd }: Props) {
  const { register, handleSubmit, reset, formState: { errors } } = useForm<FormData>();

  const onSubmit = (data: FormData) => {
    onAdd({
      drugName: data.drugName,
      dosage: data.dosage || null,
      frequency: data.frequency || null,
      officialName: null,
      isVerified: false,
      knownSideEffects: [],
    });
    reset();
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="card border border-dashed border-gray-300 space-y-3">
      <h3 className="text-sm font-bold text-gray-700">✏️ 직접 약품 추가</h3>
      <div>
        <input
          {...register("drugName", { required: "약품명을 입력해 주세요." })}
          className="w-full border border-gray-300 rounded-lg px-4 py-3 text-base focus:outline-none focus:ring-2 focus:ring-blue-500"
          placeholder="약품명 (필수)"
        />
        {errors.drugName && (
          <p className="text-sm text-red-500 mt-1">{errors.drugName.message}</p>
        )}
      </div>
      <div className="grid grid-cols-2 gap-2">
        <input
          {...register("dosage")}
          className="border border-gray-300 rounded-lg px-4 py-3 text-base focus:outline-none focus:ring-2 focus:ring-blue-500"
          placeholder="용량 (예: 500mg)"
        />
        <input
          {...register("frequency")}
          className="border border-gray-300 rounded-lg px-4 py-3 text-base focus:outline-none focus:ring-2 focus:ring-blue-500"
          placeholder="횟수 (예: 1일 3회)"
        />
      </div>
      <button type="submit" className="btn-secondary w-full">
        + 추가
      </button>
    </form>
  );
}
