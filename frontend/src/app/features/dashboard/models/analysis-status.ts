export const ANALYSIS_STATUS_VALUES = {
  Pending: 'Pending',
  Processing: 'Processing',
  Completed: 'Completed',
  Delayed: 'Delayed',
  Cancelled: 'Cancelled'
} as const;

export type AnalysisStatusValue = (typeof ANALYSIS_STATUS_VALUES)[keyof typeof ANALYSIS_STATUS_VALUES];

export const ANALYSIS_STATUS_OPTIONS: readonly { value: AnalysisStatusValue; label: string }[] = [
  { value: ANALYSIS_STATUS_VALUES.Pending, label: 'Pending' },
  { value: ANALYSIS_STATUS_VALUES.Processing, label: 'Processing' },
  { value: ANALYSIS_STATUS_VALUES.Completed, label: 'Completed' },
  { value: ANALYSIS_STATUS_VALUES.Delayed, label: 'Delayed' },
  { value: ANALYSIS_STATUS_VALUES.Cancelled, label: 'Cancelled' }
];
