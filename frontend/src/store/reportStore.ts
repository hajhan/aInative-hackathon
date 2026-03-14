import { create } from "zustand";
import { apiClient } from "@/lib/apiClient";
import { ReportResult, CreateReportRequest, AiAnalysisResult } from "@/lib/types/report";

interface ReportState {
  reports: ReportResult[];
  loading: boolean;
  error: string | null;
  fetchReports: () => Promise<void>;
  createReport: (req: CreateReportRequest) => Promise<ReportResult>;
  deleteReport: (id: string) => Promise<void>;
  analyzeWithAi: (drugNames: string[], symptoms: string[]) => Promise<AiAnalysisResult>;
}

export const useReportStore = create<ReportState>((set, get) => ({
  reports: [],
  loading: false,
  error: null,

  fetchReports: async () => {
    set({ loading: true, error: null });
    try {
      const res = await apiClient.get<ReportResult[]>("/api/reports");
      set({ reports: res.data });
    } catch {
      set({ error: "보고서 목록을 불러오지 못했습니다." });
    } finally {
      set({ loading: false });
    }
  },

  createReport: async (req) => {
    const res = await apiClient.post<ReportResult>("/api/reports", req);
    set((state) => ({
      reports: [res.data, ...state.reports].sort(
        (a, b) => new Date(b.reportedAt).getTime() - new Date(a.reportedAt).getTime()
      ),
    }));
    return res.data;
  },

  deleteReport: async (id) => {
    await apiClient.delete(`/api/reports/${id}`);
    set((state) => ({
      reports: state.reports.filter((r) => r.id !== id),
    }));
  },

  analyzeWithAi: async (drugNames, symptoms) => {
    const res = await apiClient.post<AiAnalysisResult>("/api/ai/analyze", {
      drugNames,
      symptoms,
    });
    return res.data;
  },
}));
