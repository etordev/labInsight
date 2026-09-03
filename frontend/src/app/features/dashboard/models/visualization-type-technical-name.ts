export const VISUALIZATION_TYPE_TECHNICAL_NAMES = {
  BAR_CHART: 'BAR_CHART',
  LINE_CHART: 'LINE_CHART',
  PIE_CHART: 'PIE_CHART',
  DOUGHNUT_CHART: 'DOUGHNUT_CHART',
  DATA_GRID: 'DATA_GRID'
} as const;

export type VisualizationTypeTechnicalName =
  (typeof VISUALIZATION_TYPE_TECHNICAL_NAMES)[keyof typeof VISUALIZATION_TYPE_TECHNICAL_NAMES];

const VISUALIZATION_TYPE_TECHNICAL_NAME_SET = new Set<string>(Object.values(VISUALIZATION_TYPE_TECHNICAL_NAMES));

export function isVisualizationTypeTechnicalName(value: string): value is VisualizationTypeTechnicalName {
  return VISUALIZATION_TYPE_TECHNICAL_NAME_SET.has(value);
}
