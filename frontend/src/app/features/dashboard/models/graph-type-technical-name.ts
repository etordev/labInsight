export const GRAPH_TYPE_TECHNICAL_NAMES = {
  BAR_CHART: 'BAR_CHART',
  LINE_CHART: 'LINE_CHART',
  PIE_CHART: 'PIE_CHART',
  DOUGHNUT_CHART: 'DOUGHNUT_CHART',
  DATA_GRID: 'DATA_GRID'
} as const;

export type GraphTypeTechnicalName =
  (typeof GRAPH_TYPE_TECHNICAL_NAMES)[keyof typeof GRAPH_TYPE_TECHNICAL_NAMES];

const GRAPH_TYPE_TECHNICAL_NAME_SET = new Set<string>(Object.values(GRAPH_TYPE_TECHNICAL_NAMES));

export function isGraphTypeTechnicalName(value: string): value is GraphTypeTechnicalName {
  return GRAPH_TYPE_TECHNICAL_NAME_SET.has(value);
}
