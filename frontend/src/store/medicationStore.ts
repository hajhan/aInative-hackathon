import { create } from "zustand";
import { apiClient } from "@/lib/apiClient";
import { MedicationResult, CreateMedicationRequest } from "@/lib/types/medication";

interface MedicationState {
  medications: MedicationResult[];
  loading: boolean;
  error: string | null;
  fetchMedications: () => Promise<void>;
  addMedication: (req: CreateMedicationRequest) => Promise<MedicationResult>;
  updateMedication: (id: string, req: CreateMedicationRequest) => Promise<void>;
  deleteMedication: (id: string) => Promise<void>;
}

export const useMedicationStore = create<MedicationState>((set, get) => ({
  medications: [],
  loading: false,
  error: null,

  fetchMedications: async () => {
    set({ loading: true, error: null });
    try {
      const res = await apiClient.get<MedicationResult[]>("/api/medications");
      set({ medications: res.data });
    } catch {
      set({ error: "복용약 목록을 불러오지 못했습니다." });
    } finally {
      set({ loading: false });
    }
  },

  addMedication: async (req) => {
    const res = await apiClient.post<MedicationResult>("/api/medications", req);
    set((state) => ({ medications: [res.data, ...state.medications] }));
    return res.data;
  },

  updateMedication: async (id, req) => {
    const res = await apiClient.put<MedicationResult>(`/api/medications/${id}`, req);
    set((state) => ({
      medications: state.medications.map((m) => (m.id === id ? res.data : m)),
    }));
  },

  deleteMedication: async (id) => {
    await apiClient.delete(`/api/medications/${id}`);
    set((state) => ({
      medications: state.medications.filter((m) => m.id !== id),
    }));
  },
}));
