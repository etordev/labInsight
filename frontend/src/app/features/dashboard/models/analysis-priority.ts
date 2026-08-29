export const ANALYSIS_PRIORITY_VALUES = {
  Normal: 'Normal',
  High: 'High',
  Urgent: 'Urgent'
} as const;

export type AnalysisPriorityValue =
  (typeof ANALYSIS_PRIORITY_VALUES)[keyof typeof ANALYSIS_PRIORITY_VALUES];

export const ANALYSIS_PRIORITY_OPTIONS: readonly {
  value: AnalysisPriorityValue;
  label: string;
}[] = [
  { value: ANALYSIS_PRIORITY_VALUES.Normal, label: 'Normal' },
  { value: ANALYSIS_PRIORITY_VALUES.High, label: 'High' },
  { value: ANALYSIS_PRIORITY_VALUES.Urgent, label: 'Urgent' }
];
