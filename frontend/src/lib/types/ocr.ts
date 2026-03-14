export type OcrStatus = 'Pending' | 'Processing' | 'Completed' | 'Failed';

export interface ParsedDrugItem {
  drugName: string;
  dosage?: string | null;
  frequency?: string | null;
  officialName?: string | null;
  isVerified: boolean;
  knownSideEffects: string[];
}

export interface OcrUploadResult {
  id: string;
  status: OcrStatus;
  rawText?: string | null;
  drugs: ParsedDrugItem[];
  processedAt?: string | null;
}
