"use client";

import type { ParsedDrugItem } from "@/lib/types/ocr";

interface Props {
  drug: ParsedDrugItem;
  index: number;
  onEdit: () => void;
  onRemove: () => void;
}

export default function OcrResultCard({ drug, onEdit, onRemove }: Props) {
  return (
    <div className="card border border-gray-200">
      <div className="flex items-start justify-between gap-2">
        <div className="flex-1 min-w-0">
          <div className="flex items-center gap-2 flex-wrap">
            <h3 className="text-base font-bold text-gray-900 truncate">
              {drug.officialName ?? drug.drugName}
            </h3>
            {drug.isVerified && (
              <span className="px-2 py-0.5 text-xs font-medium bg-green-100 text-green-700 rounded-full whitespace-nowrap">
                식약처 확인
              </span>
            )}
          </div>
          {drug.officialName && drug.officialName !== drug.drugName && (
            <p className="text-sm text-gray-500 mt-0.5">인식명: {drug.drugName}</p>
          )}
          <div className="flex gap-3 mt-2 flex-wrap text-sm text-gray-600">
            {drug.dosage && <span>💊 {drug.dosage}</span>}
            {drug.frequency && <span>🕐 {drug.frequency}</span>}
          </div>
          {drug.knownSideEffects.length > 0 && (
            <div className="mt-2">
              <p className="text-xs font-medium text-orange-600 mb-1">⚠️ 알려진 부작용</p>
              <div className="flex flex-wrap gap-1">
                {drug.knownSideEffects.slice(0, 3).map((se) => (
                  <span
                    key={se}
                    className="px-2 py-0.5 text-xs bg-orange-50 text-orange-700 rounded-full"
                  >
                    {se}
                  </span>
                ))}
                {drug.knownSideEffects.length > 3 && (
                  <span className="text-xs text-gray-400 self-center">
                    +{drug.knownSideEffects.length - 3}개
                  </span>
                )}
              </div>
            </div>
          )}
        </div>

        <div className="flex flex-col gap-2 shrink-0">
          <button
            onClick={onEdit}
            className="min-w-[44px] min-h-[44px] flex items-center justify-center rounded-lg bg-blue-50 text-blue-600 hover:bg-blue-100 text-sm font-medium"
            aria-label="수정"
          >
            수정
          </button>
          <button
            onClick={onRemove}
            className="min-w-[44px] min-h-[44px] flex items-center justify-center rounded-lg bg-red-50 text-red-500 hover:bg-red-100 text-sm font-medium"
            aria-label="삭제"
          >
            삭제
          </button>
        </div>
      </div>
    </div>
  );
}
