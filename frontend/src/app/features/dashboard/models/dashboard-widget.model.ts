import { MetricDefinition } from './metric-definition.model';
import { VisualizationType } from './visualization-type.model';

export interface DashboardWidget {
  id: number;
  name: string;
  description: string | null;
  content: string | null;
  visualizationTypeId: number;
  metricDefinitionId: number;
  ordering: number;
  visualizationType: VisualizationType;
  metricDefinition: MetricDefinition;
}
