export interface AnalyticsPoint {
  label: string;
  value: number;
}

export interface DelayedAnalysisRow {
  analysisNumber: string;
  laboratory: string;
  category: string;
  receivedAt: string;
  priority: string;
  status: string;
  expectedProcessingHours: number;
  elapsedProcessingHours: number;
}

export interface GraphItemAnalytics {
  graphDataType: string;
  unit: string;
  data: AnalyticsPoint[];
  rows?: DelayedAnalysisRow[] | null;
}
