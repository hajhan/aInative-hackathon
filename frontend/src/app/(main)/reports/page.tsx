"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { useReportStore } from "@/store/reportStore";
import { ReportResult, Severity } from "@/lib/types/report";

const SEVERITY_LABEL: Record<Severity, { label: string; className: string }> = {
  Mild: { label: "경미함", className: "bg-green-100 text-green-800" },
  Moderate: { label: "보통", className: "bg-yellow-100 text-yellow-800" },
  Severe: { label: "심각함", className: "bg-red-100 text-red-800" },
};

function formatDate(iso: string) {
  return new Date(iso).toLocaleDateString("ko-KR", {
    year: "numeric", month: "long", day: "numeric",
  });
}

function ReportCard({ report, onDelete }: { report: ReportResult; onDelete: () => void }) {
  const [expanded, setExpanded] = useState(false);
  const sev = SEVERITY_LABEL[report.severity];

  return (
    <div className="card">
      <div className="flex items-start justify-between">
        <div className="flex-1">
          <div className="flex items-center gap-2 mb-1">
            <span className={`text-xs font-semibold px-2 py-0.5 rounded-full ${sev.className}`}>
              {sev.label}
            </span>
            <span className="text-sm text-gray-500">{formatDate(report.reportedAt)}</span>
          </div>
          <div className="flex flex-wrap gap-1">
            {report.symptoms.slice(0, 3).map((s) => (
              <span key={s.id} className={`text-sm px-2 py-0.5 rounded-full ${s.isOfficial ? "bg-orange-100 text-orange-800" : "bg-gray-100 text-gray-700"}`}>
                {s.symptomName}{s.isOfficial && " ⚠️"}
              </span>
            ))}
            {report.symptoms.length > 3 && (
              <span className="text-sm text-gray-500">+{report.symptoms.length - 3}개</span>
            )}
          </div>
        </div>
        <button
          onClick={() => setExpanded(!expanded)}
          className="ml-3 text-sm text-blue-600 min-w-[44px] min-h-[44px] flex items-center justify-center"
        >
          {expanded ? "접기" : "상세"}
        </button>
      </div>

      {expanded && (
        <div className="mt-3 pt-3 border-t border-gray-100 space-y-2">
          {report.medications.length > 0 && (
            <div>
              <p className="text-sm font-medium text-gray-700">복용약</p>
              <p className="text-sm text-gray-600">{report.medications.map((m) => m.drugName).join(", ")}</p>
            </div>
          )}
          <div>
            <p className="text-sm font-medium text-gray-700">전체 증상</p>
            <p className="text-sm text-gray-600">{report.symptoms.map((s) => s.symptomName).join(", ")}</p>
          </div>
          {report.notes && (
            <div>
              <p className="text-sm font-medium text-gray-700">메모</p>
              <p className="text-sm text-gray-600">{report.notes}</p>
            </div>
          )}
          <button
            onClick={onDelete}
            className="text-sm text-red-500 hover:text-red-700 mt-2"
          >
            삭제
          </button>
        </div>
      )}
    </div>
  );
}

export default function ReportsPage() {
  const { reports, loading, fetchReports, deleteReport } = useReportStore();

  useEffect(() => {
    fetchReports();
  }, [fetchReports]);

  const handleDelete = async (id: string) => {
    if (!confirm("보고서를 삭제하시겠습니까?")) return;
    try {
      await deleteReport(id);
    } catch {
      alert("삭제 중 오류가 발생했습니다.");
    }
  };

  return (
    <div className="px-4 py-6 max-w-md mx-auto">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900">내 보고서</h1>
        <Link href="/report">
          <button className="btn-primary text-sm px-4 py-2">+ 새 보고서</button>
        </Link>
      </div>

      {loading && <p className="text-center text-gray-500 py-8">불러오는 중...</p>}

      {!loading && reports.length === 0 && (
        <div className="text-center py-12">
          <p className="text-gray-500 text-base mb-4">아직 보고서가 없습니다.</p>
          <Link href="/report">
            <button className="btn-secondary">첫 번째 보고서 작성하기</button>
          </Link>
        </div>
      )}

      <div className="space-y-3">
        {reports.map((report) => (
          <ReportCard key={report.id} report={report} onDelete={() => handleDelete(report.id)} />
        ))}
      </div>
    </div>
  );
}
