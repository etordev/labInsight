export const GROUP_BY_VALUES = {
  DAY: 'DAY',
  WEEK: 'WEEK',
  MONTH: 'MONTH'
} as const;

export type GroupByValue = (typeof GROUP_BY_VALUES)[keyof typeof GROUP_BY_VALUES];

export const GROUP_BY_OPTIONS: readonly { value: GroupByValue; label: string }[] = [
  { value: GROUP_BY_VALUES.DAY, label: 'Day' },
  { value: GROUP_BY_VALUES.WEEK, label: 'Week' },
  { value: GROUP_BY_VALUES.MONTH, label: 'Month' }
];
