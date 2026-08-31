import {
  GRAPH_DATA_TYPE_TECHNICAL_NAMES,
  GraphDataTypeTechnicalName
} from '../models/graph-data-type-technical-name';

export const GRAPH_CONFIG_FIELDS = {
  name: 'name',
  description: 'description',
  dateFrom: 'dateFrom',
  dateTo: 'dateTo',
  timeFrom: 'timeFrom',
  timeTo: 'timeTo',
  laboratoryId: 'laboratoryId',
  analysisCategoryId: 'analysisCategoryId',
  priority: 'priority',
  status: 'status',
  groupBy: 'groupBy'
} as const;

export type GraphConfigField = (typeof GRAPH_CONFIG_FIELDS)[keyof typeof GRAPH_CONFIG_FIELDS];

export const GRAPH_DATA_TYPE_CONFIG_FIELDS: Record<
  GraphDataTypeTechnicalName,
  readonly GraphConfigField[]
> = {
  [GRAPH_DATA_TYPE_TECHNICAL_NAMES.ANALYSIS_VOLUME]: [
    GRAPH_CONFIG_FIELDS.name,
    GRAPH_CONFIG_FIELDS.description,
    GRAPH_CONFIG_FIELDS.dateFrom,
    GRAPH_CONFIG_FIELDS.dateTo,
    GRAPH_CONFIG_FIELDS.timeFrom,
    GRAPH_CONFIG_FIELDS.timeTo,
    GRAPH_CONFIG_FIELDS.laboratoryId,
    GRAPH_CONFIG_FIELDS.analysisCategoryId,
    GRAPH_CONFIG_FIELDS.priority,
    GRAPH_CONFIG_FIELDS.status,
    GRAPH_CONFIG_FIELDS.groupBy
  ],
  [GRAPH_DATA_TYPE_TECHNICAL_NAMES.ANALYSIS_STATUS]: [
    GRAPH_CONFIG_FIELDS.name,
    GRAPH_CONFIG_FIELDS.description,
    GRAPH_CONFIG_FIELDS.laboratoryId,
    GRAPH_CONFIG_FIELDS.analysisCategoryId,
    GRAPH_CONFIG_FIELDS.priority
  ],
  [GRAPH_DATA_TYPE_TECHNICAL_NAMES.PROCESSING_TIME]: [
    GRAPH_CONFIG_FIELDS.name,
    GRAPH_CONFIG_FIELDS.description,
    GRAPH_CONFIG_FIELDS.dateFrom,
    GRAPH_CONFIG_FIELDS.dateTo,
    GRAPH_CONFIG_FIELDS.timeFrom,
    GRAPH_CONFIG_FIELDS.timeTo,
    GRAPH_CONFIG_FIELDS.laboratoryId,
    GRAPH_CONFIG_FIELDS.analysisCategoryId,
    GRAPH_CONFIG_FIELDS.priority,
    GRAPH_CONFIG_FIELDS.groupBy
  ],
  [GRAPH_DATA_TYPE_TECHNICAL_NAMES.ANALYSIS_CATEGORY]: [
    GRAPH_CONFIG_FIELDS.name,
    GRAPH_CONFIG_FIELDS.description,
    GRAPH_CONFIG_FIELDS.laboratoryId,
    GRAPH_CONFIG_FIELDS.priority
  ],
  [GRAPH_DATA_TYPE_TECHNICAL_NAMES.LABORATORY_WORKLOAD]: [
    GRAPH_CONFIG_FIELDS.name,
    GRAPH_CONFIG_FIELDS.description,
    GRAPH_CONFIG_FIELDS.laboratoryId,
    GRAPH_CONFIG_FIELDS.analysisCategoryId,
    GRAPH_CONFIG_FIELDS.priority,
    GRAPH_CONFIG_FIELDS.status
  ],
  [GRAPH_DATA_TYPE_TECHNICAL_NAMES.PRIORITY_DISTRIBUTION]: [
    GRAPH_CONFIG_FIELDS.name,
    GRAPH_CONFIG_FIELDS.description,
    GRAPH_CONFIG_FIELDS.laboratoryId,
    GRAPH_CONFIG_FIELDS.analysisCategoryId
  ],
  [GRAPH_DATA_TYPE_TECHNICAL_NAMES.COMPLETION_RATE]: [
    GRAPH_CONFIG_FIELDS.name,
    GRAPH_CONFIG_FIELDS.description,
    GRAPH_CONFIG_FIELDS.dateFrom,
    GRAPH_CONFIG_FIELDS.dateTo,
    GRAPH_CONFIG_FIELDS.timeFrom,
    GRAPH_CONFIG_FIELDS.timeTo,
    GRAPH_CONFIG_FIELDS.laboratoryId,
    GRAPH_CONFIG_FIELDS.analysisCategoryId
  ],
  [GRAPH_DATA_TYPE_TECHNICAL_NAMES.DELAYED_ANALYSES]: [
    GRAPH_CONFIG_FIELDS.name,
    GRAPH_CONFIG_FIELDS.description,
    GRAPH_CONFIG_FIELDS.laboratoryId,
    GRAPH_CONFIG_FIELDS.analysisCategoryId,
    GRAPH_CONFIG_FIELDS.priority
  ]
};

export function getVisibleConfigFields(
  graphDataType: GraphDataTypeTechnicalName
): readonly GraphConfigField[] {
  return GRAPH_DATA_TYPE_CONFIG_FIELDS[graphDataType];
}

export function isConfigFieldVisible(
  graphDataType: GraphDataTypeTechnicalName,
  field: GraphConfigField
): boolean {
  return getVisibleConfigFields(graphDataType).includes(field);
}

export function supportsDateRange(graphDataType: GraphDataTypeTechnicalName): boolean {
  return (
    isConfigFieldVisible(graphDataType, GRAPH_CONFIG_FIELDS.dateFrom) &&
    isConfigFieldVisible(graphDataType, GRAPH_CONFIG_FIELDS.dateTo)
  );
}
