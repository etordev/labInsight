import { GRAPH_CONFIG_FIELDS, isConfigFieldVisible } from '../config/graph-config-fields';
import { ANALYSIS_PRIORITY_OPTIONS } from './analysis-priority';
import { ANALYSIS_STATUS_OPTIONS } from './analysis-status';
import { isGraphDataTypeTechnicalName } from './graph-data-type-technical-name';
import { parseGraphItemContent } from './build-graph-item-content';

export interface GraphScopeChip {
  label: string;
  value: string;
}

export function buildGraphScopeChips(
  content: string | null | undefined,
  graphDataTypeName: string,
  lookup: {
    laboratoryName: (id: number) => string | null;
    analysisCategoryName: (id: number) => string | null;
  }
): GraphScopeChip[] {
  if (!isGraphDataTypeTechnicalName(graphDataTypeName)) {
    return [];
  }

  const filters = parseGraphItemContent(content).filters ?? {};
  const chips: GraphScopeChip[] = [];
  const visible = (field: (typeof GRAPH_CONFIG_FIELDS)[keyof typeof GRAPH_CONFIG_FIELDS]) =>
    isConfigFieldVisible(graphDataTypeName, field);

  if (visible(GRAPH_CONFIG_FIELDS.laboratoryId) && filters.laboratoryId != null) {
    const name = lookup.laboratoryName(filters.laboratoryId);
    if (name) {
      chips.push({ label: 'Laboratory', value: name });
    }
  }

  if (visible(GRAPH_CONFIG_FIELDS.analysisCategoryId) && filters.analysisCategoryId != null) {
    const name = lookup.analysisCategoryName(filters.analysisCategoryId);
    if (name) {
      chips.push({ label: 'Category', value: name });
    }
  }

  if (visible(GRAPH_CONFIG_FIELDS.priority) && filters.priority) {
    const label =
      ANALYSIS_PRIORITY_OPTIONS.find((option) => option.value === filters.priority)?.label ??
      filters.priority;
    chips.push({ label: 'Priority', value: label });
  }

  if (visible(GRAPH_CONFIG_FIELDS.status) && filters.status) {
    const label =
      ANALYSIS_STATUS_OPTIONS.find((option) => option.value === filters.status)?.label ??
      filters.status;
    chips.push({ label: 'Status', value: label });
  }

  return chips;
}
