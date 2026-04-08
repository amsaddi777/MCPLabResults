// Types matching the LabResultMcpServer models

export interface PatientInfo {
  id: string;
  name: string;
  nda: string;
  sampleDate: string;
}

export interface LabResult {
  category: string;
  subcategory: string;
  testName: string;
  value: string;
  unit: string | null;
  normalRange: string | null;
  status: string; // "H", "L", or ""
  datePerformed: string;
  validatedBy: string;
}

export interface LabResultResponse {
  patient: PatientInfo;
  results: LabResult[];
}
