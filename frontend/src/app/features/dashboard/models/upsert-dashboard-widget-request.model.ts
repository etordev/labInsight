export interface UpsertDashboardWidgetRequest {
  id?: number;
  name: string;
  description: string | null;
  visualizationTypeId: number;
  metricDefinitionId: number;
  content: string | null;
}
