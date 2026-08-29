import { GRAPH_CONFIG_FIELDS, isConfigFieldVisible } from '../config/graph-config-fields';
import { GraphDataTypeTechnicalName } from './graph-data-type-technical-name';
import { GraphItemContent, GraphItemFilters } from './graph-item-content.model';
import { GraphWizardFormValue } from '../wizard/graph-wizard-form';

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
