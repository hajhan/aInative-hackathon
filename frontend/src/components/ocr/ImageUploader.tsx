"use client";

import { useRef, useState } from "react";

interface Props {
  onImageSelected: (file: File) => void;
  disabled?: boolean;
}

export default function ImageUploader({ onImageSelected, disabled }: Props) {
  const fileInputRef = useRef<HTMLInputElement>(null);
  const cameraInputRef = useRef<HTMLInputElement>(null);
  const [preview, setPreview] = useState<string | null>(null);

  const handleFile = (file: File) => {
    if (!file.type.startsWith("image/")) return;
    const url = URL.createObjectURL(file);
    setPreview(url);
    onImageSelected(file);
  };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) handleFile(file);
  };

  return (
    <div className="space-y-4">
      {preview ? (
        <div className="relative rounded-xl overflow-hidden border-2 border-blue-200">
          {/* eslint-disable-next-line @next/next/no-img-element */}
          <img
            src={preview}
            alt="선택된 약봉투 이미지"
            className="w-full max-h-72 object-contain bg-gray-50"
          />
          {!disabled && (
            <button
              onClick={() => {
                setPreview(null);
                if (fileInputRef.current) fileInputRef.current.value = "";
              }}
              className="absolute top-2 right-2 bg-white rounded-full p-1 shadow text-gray-500 hover:text-red-500"
              aria-label="이미지 제거"
            >
              ✕
            </button>
          )}
        </div>
      ) : (
        <div
          onClick={() => fileInputRef.current?.click()}
          className="flex flex-col items-center justify-center border-2 border-dashed border-gray-300 rounded-xl p-10 cursor-pointer hover:border-blue-400 transition-colors bg-gray-50"
          role="button"
          tabIndex={0}
          onKeyDown={(e) => e.key === "Enter" && fileInputRef.current?.click()}
        >
          <span className="text-4xl mb-3">🖼️</span>
          <p className="text-base font-medium text-gray-700">사진을 선택하세요</p>
          <p className="text-sm text-gray-400 mt-1">JPEG, PNG, WebP (최대 10MB)</p>
        </div>
      )}

      <div className="grid grid-cols-2 gap-3">
        <button
          type="button"
          onClick={() => cameraInputRef.current?.click()}
          disabled={disabled}
          className="btn-primary disabled:opacity-50 disabled:cursor-not-allowed"
        >
          📷 카메라 촬영
        </button>
        <button
          type="button"
          onClick={() => fileInputRef.current?.click()}
          disabled={disabled}
          className="btn-secondary disabled:opacity-50 disabled:cursor-not-allowed"
        >
          🗂️ 갤러리 선택
        </button>
      </div>

      {/* 갤러리 input */}
      <input
        ref={fileInputRef}
        type="file"
        accept="image/*"
        className="hidden"
        onChange={handleChange}
      />
      {/* 카메라 input */}
      <input
        ref={cameraInputRef}
        type="file"
        accept="image/*"
        capture="environment"
        className="hidden"
        onChange={handleChange}
      />
    </div>
  );
}
