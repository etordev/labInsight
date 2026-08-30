import { GRAPH_CONFIG_FIELDS, isConfigFieldVisible } from '../config/graph-config-fields';
import { GraphWizardFormValue } from '../wizard/graph-wizard-form';
import { ANALYSIS_PRIORITY_VALUES, AnalysisPriorityValue } from './analysis-priority';
import { ANALYSIS_STATUS_VALUES, AnalysisStatusValue } from './analysis-status';
import { GraphDataTypeTechnicalName } from './graph-data-type-technical-name';
import { GraphItemContent, GraphItemFilters } from './graph-item-content.model';
import { GROUP_BY_VALUES, GroupByValue } from './group-by';

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

export function parseWizardFormValueFromContent(
  content: string | null | undefined
): Pick<
  GraphWizardFormValue,
  'dateFrom' | 'dateTo' | 'laboratoryId' | 'analysisCategoryId' | 'priority' | 'status' | 'groupBy'
> {
  const parsed = parseGraphItemContent(content);
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
