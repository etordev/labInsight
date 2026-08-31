import { Component, computed, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { startWith } from 'rxjs';
import { GRAPH_CONFIG_FIELDS, isConfigFieldVisible, supportsDateRange } from '../../config/graph-config-fields';
import { GRAPH_DATA_TYPE_UI_BY_NAME } from '../../config/graph-data-type-ui.config';
import { GRAPH_TYPE_UI_BY_NAME } from '../../config/graph-type-ui.config';
import { ANALYSIS_PRIORITY_OPTIONS } from '../../models/analysis-priority';
import { ANALYSIS_STATUS_OPTIONS } from '../../models/analysis-status';
import { GROUP_BY_OPTIONS } from '../../models/group-by';
import { GraphWizardCatalog } from '../graph-wizard-catalog';
import { GraphWizardState } from '../graph-wizard-state';

export interface ReviewRow {
  label: string;
  value: string;
}

@Component({
  selector: 'app-review-graph-step',
  templateUrl: './review-graph-step.component.html',
  styleUrl: './review-graph-step.component.scss'
})
export class ReviewGraphStepComponent {
  private readonly state = inject(GraphWizardState);
  private readonly catalog = inject(GraphWizardCatalog);
  readonly isEditing = this.state.isEditing;
  private readonly formValue = toSignal(
    this.state.form.valueChanges.pipe(startWith(this.state.form.getRawValue())),
    { initialValue: this.state.form.getRawValue() }
  );

  readonly rows = computed(() => {
    this.formValue();
    this.catalog.laboratories();
    this.catalog.analysisCategories();
    return this.buildRows();
  });

  private buildRows(): ReviewRow[] {
    const dataType = this.state.selectedGraphDataType();
    const graphType = this.state.selectedGraphType();
    const value = this.state.form.getRawValue();
    const rows: ReviewRow[] = [];

    if (!dataType || !graphType) {
      return rows;
    }

    const visible = (field: (typeof GRAPH_CONFIG_FIELDS)[keyof typeof GRAPH_CONFIG_FIELDS]) =>
      isConfigFieldVisible(dataType, field);

    rows.push({ label: 'Name', value: value.name.trim() });

    const description = value.description.trim();
    if (visible(GRAPH_CONFIG_FIELDS.description) && description) {
      rows.push({ label: 'Description', value: description });
    }

    rows.push({ label: 'Data Type', value: GRAPH_DATA_TYPE_UI_BY_NAME[dataType].label });
    rows.push({ label: 'Graph Type', value: GRAPH_TYPE_UI_BY_NAME[graphType].label });

    const dateRange = supportsDateRange(dataType)
      ? this.formatDateRange(value.dateFrom, value.dateTo)
      : null;
    if (dateRange) {
      rows.push({ label: 'Date Range', value: dateRange });
    }

    if (visible(GRAPH_CONFIG_FIELDS.laboratoryId) && value.laboratoryId != null) {
      const name = this.catalog.laboratoryName(value.laboratoryId);
      if (name) {
        rows.push({ label: 'Laboratory', value: name });
      }
    }

    if (visible(GRAPH_CONFIG_FIELDS.analysisCategoryId) && value.analysisCategoryId != null) {
      const name = this.catalog.analysisCategoryName(value.analysisCategoryId);
      if (name) {
        rows.push({ label: 'Analysis Category', value: name });
      }
    }

    if (visible(GRAPH_CONFIG_FIELDS.priority) && value.priority) {
      const label =
        ANALYSIS_PRIORITY_OPTIONS.find((option) => option.value === value.priority)?.label ??
        value.priority;
      rows.push({ label: 'Priority', value: label });
    }

    if (visible(GRAPH_CONFIG_FIELDS.status) && value.status) {
      const label =
        ANALYSIS_STATUS_OPTIONS.find((option) => option.value === value.status)?.label ?? value.status;
      rows.push({ label: 'Status', value: label });
    }

    if (visible(GRAPH_CONFIG_FIELDS.groupBy) && value.groupBy) {
      const label =
        GROUP_BY_OPTIONS.find((option) => option.value === value.groupBy)?.label ?? value.groupBy;
      rows.push({ label: 'Group By', value: label });
    }

    return rows;
  }

  private formatDateRange(from: Date | null, to: Date | null): string | null {
    if (!from && !to) {
      return null;
    }

    if (from && to) {
      return `${this.formatDate(from)} – ${this.formatDate(to)}`;
    }

    if (from) {
      return `From ${this.formatDate(from)}`;
    }

    return `Until ${this.formatDate(to!)}`;
  }

  private formatDate(value: Date): string {
    return new Intl.DateTimeFormat('en-GB', {
      day: '2-digit',
      month: 'short',
      year: 'numeric'
    }).format(value);
  }
}
