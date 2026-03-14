import { create } from 'zustand';
import type { OcrUploadResult, ParsedDrugItem } from '@/lib/types/ocr';

interface OcrState {
  result: OcrUploadResult | null;
  isProcessing: boolean;
  error: string | null;
  setResult: (result: OcrUploadResult) => void;
  setProcessing: (value: boolean) => void;
  setError: (msg: string | null) => void;
  updateDrug: (index: number, drug: ParsedDrugItem) => void;
  removeDrug: (index: number) => void;
  addDrug: (drug: ParsedDrugItem) => void;
  reset: () => void;
}

export const useOcrStore = create<OcrState>((set) => ({
  result: null,
  isProcessing: false,
  error: null,

  setResult: (result) => set({ result, isProcessing: false, error: null }),
  setProcessing: (value) => set({ isProcessing: value }),
  setError: (msg) => set({ error: msg, isProcessing: false }),

  updateDrug: (index, drug) =>
    set((state) => {
      if (!state.result) return state;
      const drugs = [...state.result.drugs];
      drugs[index] = drug;
      return { result: { ...state.result, drugs } };
    }),

  removeDrug: (index) =>
    set((state) => {
      if (!state.result) return state;
      const drugs = state.result.drugs.filter((_, i) => i !== index);
      return { result: { ...state.result, drugs } };
    }),

  addDrug: (drug) =>
    set((state) => {
      if (!state.result) return state;
      return { result: { ...state.result, drugs: [...state.result.drugs, drug] } };
    }),

  reset: () => set({ result: null, isProcessing: false, error: null }),
}));
