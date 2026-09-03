import { DASHBOARD_WIDGET_CONFIG_FIELDS, isConfigFieldVisible } from '../config/dashboard-widget-config-fields';
import { ANALYSIS_PRIORITY_OPTIONS } from './analysis-priority';
import { ANALYSIS_STATUS_OPTIONS } from './analysis-status';
import { isMetricDefinitionTechnicalName } from './metric-definition-technical-name';
import { parseDashboardWidgetContent } from './build-dashboard-widget-content';

export interface DashboardWidgetScopeChip {
  label: string;
  value: string;
}

export function buildDashboardWidgetScopeChips(
  content: string | null | undefined,
  metricDefinitionName: string,
  lookup: {
    laboratoryName: (id: number) => string | null;
    analysisCategoryName: (id: number) => string | null;
  }
): DashboardWidgetScopeChip[] {
  if (!isMetricDefinitionTechnicalName(metricDefinitionName)) {
    return [];
  }

  const filters = parseDashboardWidgetContent(content).filters ?? {};
  const chips: DashboardWidgetScopeChip[] = [];
  const visible = (field: (typeof DASHBOARD_WIDGET_CONFIG_FIELDS)[keyof typeof DASHBOARD_WIDGET_CONFIG_FIELDS]) =>
    isConfigFieldVisible(metricDefinitionName, field);

  if (visible(DASHBOARD_WIDGET_CONFIG_FIELDS.laboratoryId) && filters.laboratoryId != null) {
    const name = lookup.laboratoryName(filters.laboratoryId);
    if (name) {
      chips.push({ label: 'Laboratory', value: name });
    }
  }

  if (visible(DASHBOARD_WIDGET_CONFIG_FIELDS.analysisCategoryId) && filters.analysisCategoryId != null) {
    const name = lookup.analysisCategoryName(filters.analysisCategoryId);
    if (name) {
      chips.push({ label: 'Category', value: name });
    }
  }

  if (visible(DASHBOARD_WIDGET_CONFIG_FIELDS.priority) && filters.priority) {
    const label =
      ANALYSIS_PRIORITY_OPTIONS.find((option) => option.value === filters.priority)?.label ??
      filters.priority;
    chips.push({ label: 'Priority', value: label });
  }

  if (visible(DASHBOARD_WIDGET_CONFIG_FIELDS.status) && filters.status) {
    const label =
      ANALYSIS_STATUS_OPTIONS.find((option) => option.value === filters.status)?.label ??
      filters.status;
    chips.push({ label: 'Status', value: label });
  }

  return chips;
}
