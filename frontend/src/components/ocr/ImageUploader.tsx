"use client";

import { useRef, useState, useCallback, useEffect } from "react";

interface Props {
  onImageSelected: (file: File) => void;
  disabled?: boolean;
}

export default function ImageUploader({ onImageSelected, disabled }: Props) {
  const fileInputRef = useRef<HTMLInputElement>(null);
  const videoRef = useRef<HTMLVideoElement>(null);
  const [preview, setPreview] = useState<string | null>(null);
  const [cameraOpen, setCameraOpen] = useState(false);
  const [stream, setStream] = useState<MediaStream | null>(null);
  const [cameraError, setCameraError] = useState<string | null>(null);

  const handleFile = useCallback((file: File) => {
    if (!file.type.startsWith("image/")) return;
    setPreview((prev) => {
      if (prev) URL.revokeObjectURL(prev); // 이전 URL 해제
      return URL.createObjectURL(file);
    });
    onImageSelected(file);
  }, [onImageSelected]);

  // 컴포넌트 언마운트 시 Object URL 정리
  useEffect(() => {
    return () => {
      if (preview) URL.revokeObjectURL(preview);
    };
  }, [preview]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) handleFile(file);
  };

  // 카메라 스트림 시작
  const openCamera = useCallback(async () => {
    setCameraError(null);
    setCameraOpen(true);
    try {
      const mediaStream = await navigator.mediaDevices.getUserMedia({
        video: { facingMode: "environment", width: { ideal: 1280 }, height: { ideal: 720 } },
      });
      setStream(mediaStream);
    } catch {
      try {
        // 후면 카메라 없으면 기본 카메라로 재시도
        const mediaStream = await navigator.mediaDevices.getUserMedia({ video: true });
        setStream(mediaStream);
      } catch (err) {
        setCameraError("카메라에 접근할 수 없습니다. 브라우저 권한을 확인해 주세요.");
        console.error(err);
      }
    }
  }, []);

  // stream이 설정되면 video 엘리먼트에 연결
  useEffect(() => {
    if (stream && videoRef.current) {
      videoRef.current.srcObject = stream;
    }
  }, [stream]);

  // 카메라 스트림 종료
  const closeCamera = useCallback(() => {
    stream?.getTracks().forEach((t) => t.stop());
    setStream(null);
    setCameraOpen(false);
    setCameraError(null);
  }, [stream]);

  // 현재 프레임 캡처
  const capture = useCallback(() => {
    if (!videoRef.current) return;
    const video = videoRef.current;
    const canvas = document.createElement("canvas");
    canvas.width = video.videoWidth;
    canvas.height = video.videoHeight;
    canvas.getContext("2d")?.drawImage(video, 0, 0);
    canvas.toBlob((blob) => {
      if (!blob) return;
      const file = new File([blob], `camera-${Date.now()}.jpg`, { type: "image/jpeg" });
      handleFile(file);
      closeCamera();
    }, "image/jpeg", 0.92);
  }, [handleFile, closeCamera]);

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
          onClick={openCamera}
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

      {/* 카메라 모달 */}
      {cameraOpen && (
        <div className="fixed inset-0 z-50 bg-black/80 flex flex-col items-center justify-center p-4">
          <div className="w-full max-w-lg bg-white rounded-2xl overflow-hidden shadow-2xl">
            <div className="flex items-center justify-between px-4 py-3 border-b">
              <h3 className="font-bold text-gray-900">카메라 촬영</h3>
              <button
                onClick={closeCamera}
                className="text-gray-400 hover:text-gray-700 text-xl font-bold"
                aria-label="닫기"
              >
                ✕
              </button>
            </div>

            <div className="bg-black relative" style={{ minHeight: 280 }}>
              {cameraError ? (
                <div className="flex items-center justify-center h-64 px-6 text-center">
                  <p className="text-red-400 text-sm">{cameraError}</p>
                </div>
              ) : (
                <video
                  ref={videoRef}
                  autoPlay
                  playsInline
                  muted
                  className="w-full"
                />
              )}
            </div>

            {!cameraError && (
              <div className="flex gap-3 p-4">
                <button
                  onClick={closeCamera}
                  className="btn-secondary flex-1"
                >
                  취소
                </button>
                <button
                  onClick={capture}
                  className="btn-primary flex-1 text-lg"
                >
                  📸 촬영
                </button>
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
}
