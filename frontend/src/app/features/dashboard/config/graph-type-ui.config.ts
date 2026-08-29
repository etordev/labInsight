import {
  GRAPH_TYPE_TECHNICAL_NAMES,
  GraphTypeTechnicalName
} from '../models/graph-type-technical-name';

export interface GraphTypeUiConfig {
  technicalName: GraphTypeTechnicalName;
  label: string;
  description: string;
  icon: string;
}

export const GRAPH_TYPE_UI_CONFIG: readonly GraphTypeUiConfig[] = [
  {
    technicalName: GRAPH_TYPE_TECHNICAL_NAMES.LINE_CHART,
    label: 'Line Chart',
    description: 'Show trends and changes over time.',
    icon: 'show_chart'
  },
  {
    technicalName: GRAPH_TYPE_TECHNICAL_NAMES.BAR_CHART,
    label: 'Bar Chart',
    description: 'Compare values across categories.',
    icon: 'bar_chart'
  },
  {
    technicalName: GRAPH_TYPE_TECHNICAL_NAMES.PIE_CHART,
    label: 'Pie Chart',
    description: 'Show how values contribute to a whole.',
    icon: 'pie_chart'
  },
  {
    technicalName: GRAPH_TYPE_TECHNICAL_NAMES.DOUGHNUT_CHART,
    label: 'Doughnut Chart',
    description: 'Display proportional distribution in a compact format.',
    icon: 'donut_large'
  },
  {
    technicalName: GRAPH_TYPE_TECHNICAL_NAMES.DATA_GRID,
    label: 'Data Grid',
    description: 'Display detailed values in a structured table.',
    icon: 'table_chart'
  }
];

export const GRAPH_TYPE_UI_BY_NAME: Record<GraphTypeTechnicalName, GraphTypeUiConfig> =
  Object.fromEntries(GRAPH_TYPE_UI_CONFIG.map((item) => [item.technicalName, item])) as Record<
    GraphTypeTechnicalName,
    GraphTypeUiConfig
  >;
