import { GraphDataType } from './graph-data-type.model';
import { GraphType } from './graph-type.model';

export interface GraphItem {
  id: number;
  name: string;
  description: string | null;
  content: string | null;
  graphTypeId: number;
  graphDataTypeId: number;
  ordering: number;
  graphType: GraphType;
  graphDataType: GraphDataType;
}
