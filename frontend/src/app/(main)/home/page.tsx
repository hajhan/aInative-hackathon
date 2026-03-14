"use client";

import Link from "next/link";
import { useEffect } from "react";
import { useAuthStore } from "@/store/authStore";
import { useReportStore } from "@/store/reportStore";
import { useMedicationStore } from "@/store/medicationStore";
import { Severity } from "@/lib/types/report";

const SEVERITY_LABEL: Record<Severity, { label: string; className: string }> = {
  Mild: { label: "경미함", className: "bg-green-100 text-green-800" },
  Moderate: { label: "보통", className: "bg-yellow-100 text-yellow-800" },
  Severe: { label: "심각함", className: "bg-red-100 text-red-800" },
};

export default function HomePage() {
  const { user } = useAuthStore();
  const { reports, fetchReports } = useReportStore();
  const { medications, fetchMedications } = useMedicationStore();

  useEffect(() => {
    fetchReports();
    fetchMedications();
  }, [fetchReports, fetchMedications]);

  const recentReports = reports.slice(0, 3);

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
            <Link href="/report">
              <button className="btn-primary w-full">
                📋 부작용 보고서 작성
              </button>
            </Link>
          </div>

          <div className="card">
            <div className="flex items-center justify-between mb-2">
              <h2 className="text-lg font-bold text-gray-900">내 복용약</h2>
              <span className="text-sm text-gray-500">{medications.length}개</span>
            </div>
            <p className="text-sm text-gray-600 mb-4">
              복용 중인 약을 등록하면 AI 분석 정확도가 높아져요.
            </p>
            <Link href="/medications">
              <button className="btn-secondary w-full">
                💊 복용약 관리
              </button>
            </Link>
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

        {/* 최근 보고서 */}
        {recentReports.length > 0 && (
          <div className="mt-6">
            <div className="flex items-center justify-between mb-3">
              <h2 className="text-lg font-bold text-gray-900">최근 보고서</h2>
              <Link href="/reports" className="text-sm text-blue-600">전체 보기</Link>
            </div>
            <div className="space-y-2">
              {recentReports.map((r) => {
                const sev = SEVERITY_LABEL[r.severity];
                return (
                  <div key={r.id} className="card py-3">
                    <div className="flex items-center gap-2">
                      <span className={`text-xs font-semibold px-2 py-0.5 rounded-full ${sev.className}`}>
                        {sev.label}
                      </span>
                      <span className="text-sm text-gray-700">
                        {r.symptoms.slice(0, 2).map((s) => s.symptomName).join(", ")}
                        {r.symptoms.length > 2 && ` 외 ${r.symptoms.length - 2}개`}
                      </span>
                    </div>
                    <p className="text-xs text-gray-500 mt-1">
                      {new Date(r.reportedAt).toLocaleDateString("ko-KR")}
                    </p>
                  </div>
                );
              })}
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
