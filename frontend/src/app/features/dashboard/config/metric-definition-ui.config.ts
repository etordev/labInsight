import {
  METRIC_DEFINITION_TECHNICAL_NAMES,
  MetricDefinitionTechnicalName
} from '../models/metric-definition-technical-name';

export interface MetricDefinitionUiConfig {
  technicalName: MetricDefinitionTechnicalName;
  label: string;
  description: string;
  icon: string;
}

export const METRIC_DEFINITION_UI_CONFIG: readonly MetricDefinitionUiConfig[] = [
  {
    technicalName: METRIC_DEFINITION_TECHNICAL_NAMES.ANALYSIS_VOLUME,
    label: 'Analysis Volume',
    description: 'Number of laboratory analyses received over time.',
    icon: 'show_chart'
  },
  {
    technicalName: METRIC_DEFINITION_TECHNICAL_NAMES.ANALYSIS_STATUS,
    label: 'Analysis Status',
    description: 'Distribution of analyses by operational status.',
    icon: 'donut_large'
  },
  {
    technicalName: METRIC_DEFINITION_TECHNICAL_NAMES.PROCESSING_TIME,
    label: 'Processing Time',
    description: 'Average laboratory processing duration.',
    icon: 'schedule'
  },
  {
    technicalName: METRIC_DEFINITION_TECHNICAL_NAMES.ANALYSIS_CATEGORY,
    label: 'Analysis Category',
    description: 'Distribution of analyses across laboratory categories.',
    icon: 'category'
  },
  {
    technicalName: METRIC_DEFINITION_TECHNICAL_NAMES.LABORATORY_WORKLOAD,
    label: 'Laboratory Workload',
    description: 'Current workload across laboratories.',
    icon: 'science'
  },
  {
    technicalName: METRIC_DEFINITION_TECHNICAL_NAMES.PRIORITY_DISTRIBUTION,
    label: 'Priority Distribution',
    description: 'Distribution of Normal, High and Urgent analyses.',
    icon: 'priority_high'
  },
  {
    technicalName: METRIC_DEFINITION_TECHNICAL_NAMES.COMPLETION_RATE,
    label: 'Completion Rate',
    description: 'Ratio of completed analyses to total analyses.',
    icon: 'task_alt'
  },
  {
    technicalName: METRIC_DEFINITION_TECHNICAL_NAMES.DELAYED_ANALYSES,
    label: 'Delayed Analyses',
    description: 'Analyses exceeding their expected processing time.',
    icon: 'warning_amber'
  }
];

export const METRIC_DEFINITION_UI_BY_NAME: Record<
  MetricDefinitionTechnicalName,
  MetricDefinitionUiConfig
> = Object.fromEntries(
  METRIC_DEFINITION_UI_CONFIG.map((item) => [item.technicalName, item])
) as Record<MetricDefinitionTechnicalName, MetricDefinitionUiConfig>;
