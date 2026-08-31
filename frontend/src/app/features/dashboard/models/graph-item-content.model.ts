import { AnalysisPriorityValue } from './analysis-priority';
import { AnalysisStatusValue } from './analysis-status';
import { GroupByValue } from './group-by';

export interface GraphItemFilters {
  dateFrom?: string;
  dateTo?: string;
  timeFrom?: string;
  timeTo?: string;
  laboratoryId?: number;
  analysisCategoryId?: number;
  priority?: AnalysisPriorityValue;
  status?: AnalysisStatusValue;
}

export interface GraphItemContent {
  filters?: GraphItemFilters;
  groupBy?: GroupByValue;
}
