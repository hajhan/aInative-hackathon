"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import apiClient from "@/lib/apiClient";
import { useOcrStore } from "@/store/ocrStore";
import ImageUploader from "@/components/ocr/ImageUploader";
import ProcessingSpinner from "@/components/ocr/ProcessingSpinner";
import type { OcrUploadResult } from "@/lib/types/ocr";

export default function OcrPage() {
  const router = useRouter();
  const { setResult, setProcessing, setError, isProcessing } = useOcrStore();
  const [selectedFile, setSelectedFile] = useState<File | null>(null);

  const handleUpload = async () => {
    if (!selectedFile) return;

    setProcessing(true);
    const formData = new FormData();
    formData.append("image", selectedFile);
    formData.append("deleteAfterProcessing", "true");

    try {
      const res = await apiClient.post<OcrUploadResult>("/api/ocr/upload", formData, {
        headers: { "Content-Type": "multipart/form-data" },
      });
      setResult(res.data);
      router.push("/ocr/result");
    } catch {
      setError("약봉투 인식에 실패했습니다. 다시 시도해 주세요.");
      setProcessing(false);
    }
  };

  return (
    <div className="px-4 py-6">
      <div className="max-w-md mx-auto">
        <div className="mb-6">
          <h1 className="text-2xl font-bold text-gray-900">약봉투 인식</h1>
          <p className="mt-1 text-base text-gray-600">
            약봉투 사진을 촬영하거나 갤러리에서 선택하세요.
          </p>
        </div>

        {isProcessing ? (
          <ProcessingSpinner />
        ) : (
          <div className="space-y-6">
            <ImageUploader
              onImageSelected={setSelectedFile}
              disabled={isProcessing}
            />

            {selectedFile && (
              <button
                onClick={handleUpload}
                disabled={isProcessing}
                className="btn-primary w-full text-lg"
              >
                인식하기
              </button>
            )}

            <div className="text-center text-sm text-gray-400">
              <p>개인정보 보호를 위해 이미지는 처리 후 즉시 삭제됩니다.</p>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
