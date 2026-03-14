export type Severity = "Mild" | "Moderate" | "Severe";

export interface SymptomResult {
  id: string;
  symptomName: string;
  isOfficial: boolean;
}

export interface ReportMedicationResult {
  medicationId: string;
  drugName: string;
}

export interface ReportResult {
  id: string;
  reportedAt: string;
  severity: Severity;
  notes: string | null;
  symptoms: SymptomResult[];
  medications: ReportMedicationResult[];
}

export interface CreateReportRequest {
  medicationIds: string[];
  symptoms: string[];
  severity: Severity;
  notes?: string;
}

export interface AiAnalysisResult {
  officialSideEffects: string[];
  possibleInteractions: string[];
  summary: string;
  recommendation: string;
}
