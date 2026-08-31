import { GRAPH_CONFIG_FIELDS, isConfigFieldVisible } from '../config/graph-config-fields';
import { GraphWizardFormValue } from '../wizard/graph-wizard-form';
import { ANALYSIS_PRIORITY_VALUES, AnalysisPriorityValue } from './analysis-priority';
import { ANALYSIS_STATUS_VALUES, AnalysisStatusValue } from './analysis-status';
import { GraphDataTypeTechnicalName } from './graph-data-type-technical-name';
import { GraphItemContent, GraphItemFilters } from './graph-item-content.model';
import { GROUP_BY_VALUES, GroupByValue } from './group-by';

export function toTimeOnlyString(value: Date): string {
  const hours = String(value.getHours()).padStart(2, '0');
  const minutes = String(value.getMinutes()).padStart(2, '0');
  return `${hours}:${minutes}`;
}

export function toDateOnlyString(value: Date): string {
  const year = value.getFullYear();
  const month = String(value.getMonth() + 1).padStart(2, '0');
  const day = String(value.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}

export function buildGraphItemContent(
  graphDataType: GraphDataTypeTechnicalName,
  value: GraphWizardFormValue
): GraphItemContent | null {
  const visible = (field: (typeof GRAPH_CONFIG_FIELDS)[keyof typeof GRAPH_CONFIG_FIELDS]) =>
    isConfigFieldVisible(graphDataType, field);

  const filters: GraphItemFilters = {};

  if (visible(GRAPH_CONFIG_FIELDS.dateFrom) && value.dateFrom) {
    filters.dateFrom = toDateOnlyString(value.dateFrom);
  }

  if (visible(GRAPH_CONFIG_FIELDS.dateTo) && value.dateTo) {
    filters.dateTo = toDateOnlyString(value.dateTo);
  }

  if (visible(GRAPH_CONFIG_FIELDS.timeFrom) && value.timeFrom) {
    filters.timeFrom = toTimeOnlyString(value.timeFrom);
  }

  if (visible(GRAPH_CONFIG_FIELDS.timeTo) && value.timeTo) {
    filters.timeTo = toTimeOnlyString(value.timeTo);
  }

  if (visible(GRAPH_CONFIG_FIELDS.laboratoryId) && value.laboratoryId != null) {
    filters.laboratoryId = value.laboratoryId;
  }

  if (visible(GRAPH_CONFIG_FIELDS.analysisCategoryId) && value.analysisCategoryId != null) {
    filters.analysisCategoryId = value.analysisCategoryId;
  }

  if (visible(GRAPH_CONFIG_FIELDS.priority) && value.priority) {
    filters.priority = value.priority;
  }

  if (visible(GRAPH_CONFIG_FIELDS.status) && value.status) {
    filters.status = value.status;
  }

  const content: GraphItemContent = {};

  if (Object.keys(filters).length > 0) {
    content.filters = filters;
  }

  if (visible(GRAPH_CONFIG_FIELDS.groupBy) && value.groupBy) {
    content.groupBy = value.groupBy;
  }

  return Object.keys(content).length > 0 ? content : null;
}

export function serializeGraphItemContent(
  graphDataType: GraphDataTypeTechnicalName,
  value: GraphWizardFormValue
): string | null {
  const content = buildGraphItemContent(graphDataType, value);
  return content ? JSON.stringify(content) : null;
}

export function replaceContentDateFilters(
  content: string | null | undefined,
  dateFrom: Date | null,
  dateTo: Date | null
): string | null {
  const parsed = parseGraphItemContent(content);
  const filters: GraphItemFilters = { ...(parsed.filters ?? {}) };

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

  const next: GraphItemContent = { ...parsed };
  if (Object.keys(filters).length > 0) {
    next.filters = filters;
  } else {
    delete next.filters;
  }

  return Object.keys(next).length > 0 ? JSON.stringify(next) : null;
}

export function parseGraphItemContent(content: string | null | undefined): GraphItemContent {
  if (!content) {
    return {};
  }

  try {
    const parsed = JSON.parse(content) as GraphItemContent;
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

export function parseTimeOnlyString(value: string | undefined): Date | null {
  if (!value) {
    return null;
  }

  const match = /^(\d{1,2}):(\d{2})/.exec(value);
  if (!match) {
    return null;
  }

  const hours = Number(match[1]);
  const minutes = Number(match[2]);
  if (hours > 23 || minutes > 59) {
    return null;
  }

  const parsed = new Date();
  parsed.setHours(hours, minutes, 0, 0);
  return parsed;
}

export function parseWizardFormValueFromContent(
  content: string | null | undefined
): Pick<
  GraphWizardFormValue,
  | 'dateFrom'
  | 'dateTo'
  | 'timeFrom'
  | 'timeTo'
  | 'laboratoryId'
  | 'analysisCategoryId'
  | 'priority'
  | 'status'
  | 'groupBy'
> {
  const parsed = parseGraphItemContent(content);
  const filters = parsed.filters ?? {};

  return {
    dateFrom: parseDateOnlyString(filters.dateFrom),
    dateTo: parseDateOnlyString(filters.dateTo),
    timeFrom: parseTimeOnlyString(filters.timeFrom),
    timeTo: parseTimeOnlyString(filters.timeTo),
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
