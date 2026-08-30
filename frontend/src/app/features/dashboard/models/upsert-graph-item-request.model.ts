export interface UpsertGraphItemRequest {
  id?: number;
  name: string;
  description: string | null;
  graphTypeId: number;
  graphDataTypeId: number;
  content: string | null;
}
