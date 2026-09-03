import { DASHBOARD_WIDGET_CONFIG_FIELDS, isConfigFieldVisible } from '../config/dashboard-widget-config-fields';
import { DashboardWidgetWizardFormValue } from '../wizard/dashboard-widget-wizard-form';
import { ANALYSIS_PRIORITY_VALUES, AnalysisPriorityValue } from './analysis-priority';
import { ANALYSIS_STATUS_VALUES, AnalysisStatusValue } from './analysis-status';
import { MetricDefinitionTechnicalName } from './metric-definition-technical-name';
import { DashboardWidgetContent, DashboardWidgetFilters } from './dashboard-widget-content.model';
import { GROUP_BY_VALUES, GroupByValue } from './group-by';

export function toDateOnlyString(value: Date): string {
  const year = value.getFullYear();
  const month = String(value.getMonth() + 1).padStart(2, '0');
  const day = String(value.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}

export function buildDashboardWidgetContent(
  metricDefinition: MetricDefinitionTechnicalName,
  value: DashboardWidgetWizardFormValue
): DashboardWidgetContent | null {
  const visible = (field: (typeof DASHBOARD_WIDGET_CONFIG_FIELDS)[keyof typeof DASHBOARD_WIDGET_CONFIG_FIELDS]) =>
    isConfigFieldVisible(metricDefinition, field);

  const filters: DashboardWidgetFilters = {};

  if (visible(DASHBOARD_WIDGET_CONFIG_FIELDS.dateFrom) && value.dateFrom) {
    filters.dateFrom = toDateOnlyString(value.dateFrom);
  }

  if (visible(DASHBOARD_WIDGET_CONFIG_FIELDS.dateTo) && value.dateTo) {
    filters.dateTo = toDateOnlyString(value.dateTo);
  }

  if (visible(DASHBOARD_WIDGET_CONFIG_FIELDS.laboratoryId) && value.laboratoryId != null) {
    filters.laboratoryId = value.laboratoryId;
  }

  if (visible(DASHBOARD_WIDGET_CONFIG_FIELDS.analysisCategoryId) && value.analysisCategoryId != null) {
    filters.analysisCategoryId = value.analysisCategoryId;
  }

  if (visible(DASHBOARD_WIDGET_CONFIG_FIELDS.priority) && value.priority) {
    filters.priority = value.priority;
  }

  if (visible(DASHBOARD_WIDGET_CONFIG_FIELDS.status) && value.status) {
    filters.status = value.status;
  }

  const content: DashboardWidgetContent = {};

  if (Object.keys(filters).length > 0) {
    content.filters = filters;
  }

  if (visible(DASHBOARD_WIDGET_CONFIG_FIELDS.groupBy) && value.groupBy) {
    content.groupBy = value.groupBy;
  }

  return Object.keys(content).length > 0 ? content : null;
}

export function serializeDashboardWidgetContent(
  metricDefinition: MetricDefinitionTechnicalName,
  value: DashboardWidgetWizardFormValue
): string | null {
  const content = buildDashboardWidgetContent(metricDefinition, value);
  return content ? JSON.stringify(content) : null;
}

export function replaceContentDateFilters(
  content: string | null | undefined,
  dateFrom: Date | null,
  dateTo: Date | null
): string | null {
  const parsed = parseDashboardWidgetContent(content);
  const filters: DashboardWidgetFilters = { ...(parsed.filters ?? {}) };
  delete (filters as Record<string, unknown>)['timeFrom'];
  delete (filters as Record<string, unknown>)['timeTo'];

  if (dateFrom) {
    filters.dateFrom = toDateOnlyString(dateFrom);
  } else {
    delete filters.dateFrom;
  }

  if (dateTo) {
    filters.dateTo = toDateOnlyString(dateTo);
  } else {
    delete filters.dateTo;
  }

  const next: DashboardWidgetContent = { ...parsed };
  if (Object.keys(filters).length > 0) {
    next.filters = filters;
  } else {
    delete next.filters;
  }

  return Object.keys(next).length > 0 ? JSON.stringify(next) : null;
}

export function parseDashboardWidgetContent(content: string | null | undefined): DashboardWidgetContent {
  if (!content) {
    return {};
  }

  try {
    const parsed = JSON.parse(content) as DashboardWidgetContent;
    return parsed && typeof parsed === 'object' ? parsed : {};
  } catch {
    return {};
  }
}

export function parseDateOnlyString(value: string | undefined): Date | null {
  if (!value) {
    return null;
  }

  const match = /^(\d{4})-(\d{2})-(\d{2})/.exec(value);
  if (!match) {
    return null;
  }

  return new Date(Number(match[1]), Number(match[2]) - 1, Number(match[3]));
}

export function parseWizardFormValueFromContent(
  content: string | null | undefined
): Pick<
  DashboardWidgetWizardFormValue,
  'dateFrom' | 'dateTo' | 'laboratoryId' | 'analysisCategoryId' | 'priority' | 'status' | 'groupBy'
> {
  const parsed = parseDashboardWidgetContent(content);
  const filters = parsed.filters ?? {};

  return {
    dateFrom: parseDateOnlyString(filters.dateFrom),
    dateTo: parseDateOnlyString(filters.dateTo),
    laboratoryId: typeof filters.laboratoryId === 'number' ? filters.laboratoryId : null,
    analysisCategoryId:
      typeof filters.analysisCategoryId === 'number' ? filters.analysisCategoryId : null,
    priority: isPriority(filters.priority) ? filters.priority : null,
    status: isStatus(filters.status) ? filters.status : null,
    groupBy: isGroupBy(parsed.groupBy) ? parsed.groupBy : null
  };
}

function isPriority(value: string | undefined): value is AnalysisPriorityValue {
  return value != null && Object.values(ANALYSIS_PRIORITY_VALUES).includes(value as AnalysisPriorityValue);
}

function isStatus(value: string | undefined): value is AnalysisStatusValue {
  return value != null && Object.values(ANALYSIS_STATUS_VALUES).includes(value as AnalysisStatusValue);
}

function isGroupBy(value: string | undefined): value is GroupByValue {
  return value != null && Object.values(GROUP_BY_VALUES).includes(value as GroupByValue);
}
