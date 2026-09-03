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
import { supportsDateRange } from '../../config/dashboard-widget-config-fields';
import { METRIC_DEFINITION_UI_BY_NAME } from '../../config/metric-definition-ui.config';
import { VISUALIZATION_TYPE_UI_BY_NAME } from '../../config/visualization-type-ui.config';
import {
  parseDateOnlyString,
  parseDashboardWidgetContent,
  replaceContentDateFilters,
  toDateOnlyString
} from '../../models/build-dashboard-widget-content';
import { isMetricDefinitionTechnicalName } from '../../models/metric-definition-technical-name';
import { DashboardWidgetAnalytics } from '../../models/dashboard-widget-analytics.model';
import { DashboardWidget } from '../../models/dashboard-widget.model';
import { buildDashboardWidgetScopeChips } from '../../models/dashboard-widget-scope-chips';
import { VISUALIZATION_TYPE_TECHNICAL_NAMES, isVisualizationTypeTechnicalName } from '../../models/visualization-type-technical-name';
import { FilterOptionCatalog } from '../../services/filter-option-catalog.service';
import { DashboardWidgetService } from '../../services/dashboard-widget.service';
import { BarChartComponent } from '../visualizations/bar-chart.component';
import { DataGridComponent } from '../visualizations/data-grid.component';
import { DoughnutChartComponent } from '../visualizations/doughnut-chart.component';
import { LineChartComponent } from '../visualizations/line-chart.component';
import { PieChartComponent } from '../visualizations/pie-chart.component';

@Component({
  selector: 'app-dashboard-widget',
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
  templateUrl: './dashboard-widget.component.html',
  styleUrl: './dashboard-widget.component.scss'
})
export class DashboardWidgetComponent {
  private readonly dashboardWidgetService = inject(DashboardWidgetService);
  private readonly filterOptions = inject(FilterOptionCatalog);
  private readonly snackBar = inject(MatSnackBar);

  readonly dashboardWidget = input.required<DashboardWidget>();
  readonly rearrangeMode = input(false);
  readonly edit = output<void>();
  readonly remove = output<void>();
  readonly itemUpdated = output<DashboardWidget>();
  readonly analytics = signal<DashboardWidgetAnalytics | null>(null);
  readonly isLoading = signal(true);
  readonly hasError = signal(false);
  readonly hasInvalidDateRange = signal(false);

  readonly visualizationTypes = VISUALIZATION_TYPE_TECHNICAL_NAMES;
  readonly dateFilters = new FormGroup({
    dateFrom: new FormControl<Date | null>(null),
    dateTo: new FormControl<Date | null>(null)
  });

  readonly showsDateRange = computed(() => {
    const name = this.dashboardWidget().metricDefinition.technicalName;
    return isMetricDefinitionTechnicalName(name) && supportsDateRange(name);
  });

  readonly scopeChips = computed(() => {
    this.filterOptions.laboratories();
    this.filterOptions.analysisCategories();
    const item = this.dashboardWidget();
    return buildDashboardWidgetScopeChips(item.content, item.metricDefinition.technicalName, this.filterOptions);
  });

  readonly description = computed(() => {
    const value = this.dashboardWidget().description?.trim();
    return value ? value : null;
  });

  constructor() {
    effect(() => {
      const item = this.dashboardWidget();
      untracked(() => {
        this.syncDateFilters(item);
        this.loadAnalytics(item.id);
      });
    });
  }

  metricDefinitionLabel(): string {
    const name = this.dashboardWidget().metricDefinition.technicalName;
    return isMetricDefinitionTechnicalName(name) ? METRIC_DEFINITION_UI_BY_NAME[name].label : name;
  }

  visualizationTypeLabel(): string {
    const name = this.dashboardWidget().visualizationType.technicalName;
    return isVisualizationTypeTechnicalName(name) ? VISUALIZATION_TYPE_UI_BY_NAME[name].label : name;
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

    const item = this.dashboardWidget();
    const filters = parseDashboardWidgetContent(item.content).filters ?? {};
    const nextFrom = dateFrom ? toDateOnlyString(dateFrom) : undefined;
    const nextTo = dateTo ? toDateOnlyString(dateTo) : undefined;
    if ((filters.dateFrom ?? undefined) === nextFrom && (filters.dateTo ?? undefined) === nextTo) {
      return;
    }

    const content = replaceContentDateFilters(item.content, dateFrom, dateTo);

    this.dashboardWidgetService
      .upsertDashboardWidget({
        id: item.id,
        name: item.name,
        description: item.description,
        visualizationTypeId: item.visualizationTypeId,
        metricDefinitionId: item.metricDefinitionId,
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

  private syncDateFilters(item: DashboardWidget): void {
    const filters = parseDashboardWidgetContent(item.content).filters ?? {};
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

    this.dashboardWidgetService.getDashboardWidgetData(id).subscribe({
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
