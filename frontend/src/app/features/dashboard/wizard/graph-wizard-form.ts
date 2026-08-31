import { AbstractControl, FormControl, FormGroup, ValidationErrors, Validators } from '@angular/forms';
import { AnalysisPriorityValue } from '../models/analysis-priority';
import { AnalysisStatusValue } from '../models/analysis-status';
import { GroupByValue } from '../models/group-by';

export const GRAPH_NAME_MAX_LENGTH = 200;
export const GRAPH_DESCRIPTION_MAX_LENGTH = 500;

export interface GraphWizardFormControls {
  name: FormControl<string>;
  description: FormControl<string>;
  dateFrom: FormControl<Date | null>;
  dateTo: FormControl<Date | null>;
  laboratoryId: FormControl<number | null>;
  analysisCategoryId: FormControl<number | null>;
  priority: FormControl<AnalysisPriorityValue | null>;
  status: FormControl<AnalysisStatusValue | null>;
  groupBy: FormControl<GroupByValue | null>;
}

export type GraphWizardForm = FormGroup<GraphWizardFormControls>;

export interface GraphWizardFormValue {
  name: string;
  description: string;
  dateFrom: Date | null;
  dateTo: Date | null;
  laboratoryId: number | null;
  analysisCategoryId: number | null;
  priority: AnalysisPriorityValue | null;
  status: AnalysisStatusValue | null;
  groupBy: GroupByValue | null;
}

export function requiredTrimmed(control: AbstractControl): ValidationErrors | null {
  const value = typeof control.value === 'string' ? control.value.trim() : '';
  return value.length === 0 ? { required: true } : null;
}

export function dateRangeValidator(group: AbstractControl): ValidationErrors | null {
  const dateFrom = group.get('dateFrom')?.value as Date | null;
  const dateTo = group.get('dateTo')?.value as Date | null;

  if (!dateFrom || !dateTo) {
    return null;
  }

  const fromTime = startOfDay(dateFrom).getTime();
  const toTime = startOfDay(dateTo).getTime();
  return fromTime > toTime ? { dateRange: true } : null;
}

export function createGraphWizardForm(): GraphWizardForm {
  return new FormGroup<GraphWizardFormControls>(
    {
      name: new FormControl('', {
        nonNullable: true,
        validators: [requiredTrimmed, Validators.maxLength(GRAPH_NAME_MAX_LENGTH)]
      }),
      description: new FormControl('', {
        nonNullable: true,
        validators: [Validators.maxLength(GRAPH_DESCRIPTION_MAX_LENGTH)]
      }),
      dateFrom: new FormControl<Date | null>(null),
      dateTo: new FormControl<Date | null>(null),
      laboratoryId: new FormControl<number | null>(null),
      analysisCategoryId: new FormControl<number | null>(null),
      priority: new FormControl<AnalysisPriorityValue | null>(null),
      status: new FormControl<AnalysisStatusValue | null>(null),
      groupBy: new FormControl<GroupByValue | null>(null)
    },
    { validators: dateRangeValidator }
  );
}

function startOfDay(value: Date): Date {
  return new Date(value.getFullYear(), value.getMonth(), value.getDate());
}
