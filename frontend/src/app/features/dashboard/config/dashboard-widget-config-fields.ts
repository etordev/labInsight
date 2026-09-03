import {
  METRIC_DEFINITION_TECHNICAL_NAMES,
  MetricDefinitionTechnicalName
} from '../models/metric-definition-technical-name';

export const DASHBOARD_WIDGET_CONFIG_FIELDS = {
  name: 'name',
  description: 'description',
  dateFrom: 'dateFrom',
  dateTo: 'dateTo',
  laboratoryId: 'laboratoryId',
  analysisCategoryId: 'analysisCategoryId',
  priority: 'priority',
  status: 'status',
  groupBy: 'groupBy'
} as const;

export type DashboardWidgetConfigField = (typeof DASHBOARD_WIDGET_CONFIG_FIELDS)[keyof typeof DASHBOARD_WIDGET_CONFIG_FIELDS];

export const METRIC_DEFINITION_CONFIG_FIELDS: Record<
  MetricDefinitionTechnicalName,
  readonly DashboardWidgetConfigField[]
> = {
  [METRIC_DEFINITION_TECHNICAL_NAMES.ANALYSIS_VOLUME]: [
    DASHBOARD_WIDGET_CONFIG_FIELDS.name,
    DASHBOARD_WIDGET_CONFIG_FIELDS.description,
    DASHBOARD_WIDGET_CONFIG_FIELDS.dateFrom,
    DASHBOARD_WIDGET_CONFIG_FIELDS.dateTo,
    DASHBOARD_WIDGET_CONFIG_FIELDS.laboratoryId,
    DASHBOARD_WIDGET_CONFIG_FIELDS.analysisCategoryId,
    DASHBOARD_WIDGET_CONFIG_FIELDS.priority,
    DASHBOARD_WIDGET_CONFIG_FIELDS.status,
    DASHBOARD_WIDGET_CONFIG_FIELDS.groupBy
  ],
  [METRIC_DEFINITION_TECHNICAL_NAMES.ANALYSIS_STATUS]: [
    DASHBOARD_WIDGET_CONFIG_FIELDS.name,
    DASHBOARD_WIDGET_CONFIG_FIELDS.description,
    DASHBOARD_WIDGET_CONFIG_FIELDS.laboratoryId,
    DASHBOARD_WIDGET_CONFIG_FIELDS.analysisCategoryId,
    DASHBOARD_WIDGET_CONFIG_FIELDS.priority
  ],
  [METRIC_DEFINITION_TECHNICAL_NAMES.PROCESSING_TIME]: [
    DASHBOARD_WIDGET_CONFIG_FIELDS.name,
    DASHBOARD_WIDGET_CONFIG_FIELDS.description,
    DASHBOARD_WIDGET_CONFIG_FIELDS.dateFrom,
    DASHBOARD_WIDGET_CONFIG_FIELDS.dateTo,
    DASHBOARD_WIDGET_CONFIG_FIELDS.laboratoryId,
    DASHBOARD_WIDGET_CONFIG_FIELDS.analysisCategoryId,
    DASHBOARD_WIDGET_CONFIG_FIELDS.priority,
    DASHBOARD_WIDGET_CONFIG_FIELDS.groupBy
  ],
  [METRIC_DEFINITION_TECHNICAL_NAMES.ANALYSIS_CATEGORY]: [
    DASHBOARD_WIDGET_CONFIG_FIELDS.name,
    DASHBOARD_WIDGET_CONFIG_FIELDS.description,
    DASHBOARD_WIDGET_CONFIG_FIELDS.laboratoryId,
    DASHBOARD_WIDGET_CONFIG_FIELDS.priority
  ],
  [METRIC_DEFINITION_TECHNICAL_NAMES.LABORATORY_WORKLOAD]: [
    DASHBOARD_WIDGET_CONFIG_FIELDS.name,
    DASHBOARD_WIDGET_CONFIG_FIELDS.description,
    DASHBOARD_WIDGET_CONFIG_FIELDS.laboratoryId,
    DASHBOARD_WIDGET_CONFIG_FIELDS.analysisCategoryId,
    DASHBOARD_WIDGET_CONFIG_FIELDS.priority,
    DASHBOARD_WIDGET_CONFIG_FIELDS.status
  ],
  [METRIC_DEFINITION_TECHNICAL_NAMES.PRIORITY_DISTRIBUTION]: [
    DASHBOARD_WIDGET_CONFIG_FIELDS.name,
    DASHBOARD_WIDGET_CONFIG_FIELDS.description,
    DASHBOARD_WIDGET_CONFIG_FIELDS.laboratoryId,
    DASHBOARD_WIDGET_CONFIG_FIELDS.analysisCategoryId
  ],
  [METRIC_DEFINITION_TECHNICAL_NAMES.COMPLETION_RATE]: [
    DASHBOARD_WIDGET_CONFIG_FIELDS.name,
    DASHBOARD_WIDGET_CONFIG_FIELDS.description,
    DASHBOARD_WIDGET_CONFIG_FIELDS.dateFrom,
    DASHBOARD_WIDGET_CONFIG_FIELDS.dateTo,
    DASHBOARD_WIDGET_CONFIG_FIELDS.laboratoryId,
    DASHBOARD_WIDGET_CONFIG_FIELDS.analysisCategoryId
  ],
  [METRIC_DEFINITION_TECHNICAL_NAMES.DELAYED_ANALYSES]: [
    DASHBOARD_WIDGET_CONFIG_FIELDS.name,
    DASHBOARD_WIDGET_CONFIG_FIELDS.description,
    DASHBOARD_WIDGET_CONFIG_FIELDS.laboratoryId,
    DASHBOARD_WIDGET_CONFIG_FIELDS.analysisCategoryId,
    DASHBOARD_WIDGET_CONFIG_FIELDS.priority
  ]
};

export function getVisibleConfigFields(
  metricDefinition: MetricDefinitionTechnicalName
): readonly DashboardWidgetConfigField[] {
  return METRIC_DEFINITION_CONFIG_FIELDS[metricDefinition];
}

export function isConfigFieldVisible(
  metricDefinition: MetricDefinitionTechnicalName,
  field: DashboardWidgetConfigField
): boolean {
  return getVisibleConfigFields(metricDefinition).includes(field);
}

export function supportsDateRange(metricDefinition: MetricDefinitionTechnicalName): boolean {
  return (
    isConfigFieldVisible(metricDefinition, DASHBOARD_WIDGET_CONFIG_FIELDS.dateFrom) &&
    isConfigFieldVisible(metricDefinition, DASHBOARD_WIDGET_CONFIG_FIELDS.dateTo)
  );
}
