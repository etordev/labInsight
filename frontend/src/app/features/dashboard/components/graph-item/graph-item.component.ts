import { Component, computed, effect, inject, input, output, signal, untracked } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { supportsDateRange } from '../../config/graph-config-fields';
import { GRAPH_DATA_TYPE_UI_BY_NAME } from '../../config/graph-data-type-ui.config';
import { GRAPH_TYPE_UI_BY_NAME } from '../../config/graph-type-ui.config';
import {
  parseDateOnlyString,
  parseGraphItemContent,
  replaceContentDateFilters,
  toDateOnlyString
} from '../../models/build-graph-item-content';
import { isGraphDataTypeTechnicalName } from '../../models/graph-data-type-technical-name';
import { GraphItemAnalytics } from '../../models/graph-item-analytics.model';
import { GraphItem } from '../../models/graph-item.model';
import { GRAPH_TYPE_TECHNICAL_NAMES, isGraphTypeTechnicalName } from '../../models/graph-type-technical-name';
import { GraphItemService } from '../../services/graph-item.service';
import { BarChartComponent } from '../visualizations/bar-chart.component';
import { DataGridComponent } from '../visualizations/data-grid.component';
import { DoughnutChartComponent } from '../visualizations/doughnut-chart.component';
import { LineChartComponent } from '../visualizations/line-chart.component';
import { PieChartComponent } from '../visualizations/pie-chart.component';

@Component({
  selector: 'app-graph-item',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatCardModule,
    MatDatepickerModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    LineChartComponent,
    BarChartComponent,
    PieChartComponent,
    DoughnutChartComponent,
    DataGridComponent
  ],
  host: {
    '[class.rearrange-mode]': 'rearrangeMode()'
  },
  templateUrl: './graph-item.component.html',
  styleUrl: './graph-item.component.scss'
})
export class GraphItemComponent {
  private readonly graphItemService = inject(GraphItemService);
  private readonly snackBar = inject(MatSnackBar);

  readonly graphItem = input.required<GraphItem>();
  readonly rearrangeMode = input(false);
  readonly edit = output<void>();
  readonly remove = output<void>();
  readonly itemUpdated = output<GraphItem>();
  readonly analytics = signal<GraphItemAnalytics | null>(null);
  readonly isLoading = signal(true);
  readonly hasError = signal(false);
  readonly hasInvalidDateRange = signal(false);

  readonly graphTypes = GRAPH_TYPE_TECHNICAL_NAMES;
  readonly dateFilters = new FormGroup({
    dateFrom: new FormControl<Date | null>(null),
    dateTo: new FormControl<Date | null>(null)
  });

  readonly showsDateRange = computed(() => {
    const name = this.graphItem().graphDataType.technicalName;
    return isGraphDataTypeTechnicalName(name) && supportsDateRange(name);
  });

  constructor() {
    effect(() => {
      const item = this.graphItem();
      untracked(() => {
        this.syncDateFilters(item);
        this.loadAnalytics(item.id);
      });
    });
  }

  dataTypeLabel(): string {
    const name = this.graphItem().graphDataType.technicalName;
    return isGraphDataTypeTechnicalName(name) ? GRAPH_DATA_TYPE_UI_BY_NAME[name].label : name;
  }

  graphTypeLabel(): string {
    const name = this.graphItem().graphType.technicalName;
    return isGraphTypeTechnicalName(name) ? GRAPH_TYPE_UI_BY_NAME[name].label : name;
  }

  isEmpty(): boolean {
    const payload = this.analytics();
    if (!payload) {
      return true;
    }

    if (payload.rows && payload.rows.length > 0) {
      return false;
    }

    return payload.data.length === 0;
  }

  onDateFilterChange(): void {
    if (!this.showsDateRange() || this.rearrangeMode()) {
      return;
    }

    const dateFrom = this.dateFilters.controls.dateFrom.value;
    const dateTo = this.dateFilters.controls.dateTo.value;
    if (dateFrom && dateTo && this.startOfDay(dateFrom) > this.startOfDay(dateTo)) {
      this.hasInvalidDateRange.set(true);
      return;
    }

    this.hasInvalidDateRange.set(false);

    const item = this.graphItem();
    const filters = parseGraphItemContent(item.content).filters ?? {};
    const nextFrom = dateFrom ? toDateOnlyString(dateFrom) : undefined;
    const nextTo = dateTo ? toDateOnlyString(dateTo) : undefined;
    if ((filters.dateFrom ?? undefined) === nextFrom && (filters.dateTo ?? undefined) === nextTo) {
      return;
    }

    const content = replaceContentDateFilters(item.content, dateFrom, dateTo);

    this.graphItemService
      .upsertGraphItem({
        id: item.id,
        name: item.name,
        description: item.description,
        graphTypeId: item.graphTypeId,
        graphDataTypeId: item.graphDataTypeId,
        content
      })
      .subscribe({
        next: (updated) => this.itemUpdated.emit(updated),
        error: () => {
          this.snackBar.open('Could not save the date range. Please try again.', 'Dismiss', {
            duration: 4000
          });
        }
      });
  }

  private syncDateFilters(item: GraphItem): void {
    const filters = parseGraphItemContent(item.content).filters ?? {};
    this.dateFilters.setValue(
      {
        dateFrom: parseDateOnlyString(filters.dateFrom),
        dateTo: parseDateOnlyString(filters.dateTo)
      },
      { emitEvent: false }
    );
    this.hasInvalidDateRange.set(false);
  }

  private startOfDay(value: Date): number {
    return new Date(value.getFullYear(), value.getMonth(), value.getDate()).getTime();
  }

  private loadAnalytics(id: number): void {
    this.isLoading.set(true);
    this.hasError.set(false);
    this.analytics.set(null);

    this.graphItemService.getGraphItemData(id).subscribe({
      next: (payload) => {
        this.analytics.set(payload);
        this.isLoading.set(false);
      },
      error: () => {
        this.hasError.set(true);
        this.isLoading.set(false);
      }
    });
  }
}
