"use client";

import Link from "next/link";
import { useAuthStore } from "@/store/authStore";

export default function HomePage() {
  const { user } = useAuthStore();

  return (
    <div className="px-4 py-6">
      <div className="max-w-md mx-auto">
        {/* 환영 메시지 */}
        <div className="mb-6">
          <h1 className="text-2xl font-bold text-gray-900">
            안녕하세요{user?.name ? `, ${user.name}님` : ""}!
          </h1>
          <p className="mt-1 text-base text-gray-600">
            증상을 기록하면 AI가 원인 약물을 분석해 드려요.
          </p>
        </div>

        {/* 빠른 실행 카드 */}
        <div className="space-y-4">
          <div className="card">
            <h2 className="text-lg font-bold text-gray-900 mb-2">증상 기록 · AI 분석</h2>
            <p className="text-sm text-gray-600 mb-4">
              증상을 입력하면 AI가 원인 약물을 찾아 의사·약사와 공유해요.
            </p>
            <button className="btn-primary" disabled>
              준비 중
            </button>
          </div>

          <div className="card">
            <h2 className="text-lg font-bold text-gray-900 mb-2">복용약 관리</h2>
            <p className="text-sm text-gray-600 mb-4">
              복용 중인 약을 등록하면 AI 분석 정확도가 높아져요.
            </p>
            <button className="btn-secondary" disabled>
              준비 중
            </button>
          </div>

          <div className="card">
            <h2 className="text-lg font-bold text-gray-900 mb-2">약봉투 스캔</h2>
            <p className="text-sm text-gray-600 mb-4">
              약봉투를 찍으면 복용약을 자동으로 인식하고 등록해요.
            </p>
            <Link href="/ocr">
              <button className="btn-secondary w-full">
                📷 사진 촬영 / 갤러리 선택
              </button>
            </Link>
          </div>
        </div>
      </div>
    </div>
  );
}
