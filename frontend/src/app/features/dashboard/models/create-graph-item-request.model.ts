export interface CreateGraphItemRequest {
  name: string;
  description: string | null;
  graphTypeId: number;
  graphDataTypeId: number;
  content: string | null;
}
