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
            오늘 복용하신 약물의 부작용을 기록해 보세요.
          </p>
        </div>

        {/* 빠른 실행 카드 */}
        <div className="space-y-4">
          <div className="card">
            <h2 className="text-lg font-bold text-gray-900 mb-2">부작용 보고</h2>
            <p className="text-sm text-gray-600 mb-4">
              새로운 부작용 증상을 기록하고 보고하세요.
            </p>
            <button className="btn-primary" disabled>
              보고하기 (Sprint 3에서 구현)
            </button>
          </div>

          <div className="card">
            <h2 className="text-lg font-bold text-gray-900 mb-2">복용약 관리</h2>
            <p className="text-sm text-gray-600 mb-4">
              현재 복용 중인 약물을 관리하세요.
            </p>
            <button className="btn-secondary" disabled>
              약물 목록 (Sprint 3에서 구현)
            </button>
          </div>

          <div className="card">
            <h2 className="text-lg font-bold text-gray-900 mb-2">OCR 약봉투 인식</h2>
            <p className="text-sm text-gray-600 mb-4">
              약봉투 사진을 찍어 약물 정보를 자동 입력하세요.
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
