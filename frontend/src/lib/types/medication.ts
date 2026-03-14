export interface MedicationResult {
  id: string;
  drugName: string;
  dosage: string | null;
  frequency: string | null;
  startDate: string; // ISO date (YYYY-MM-DD)
  endDate: string | null;
  createdAt: string;
}

export interface CreateMedicationRequest {
  drugName: string;
  dosage?: string;
  frequency?: string;
  startDate: string;
  endDate?: string;
}
