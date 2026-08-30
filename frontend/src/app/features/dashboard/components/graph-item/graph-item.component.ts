import { Component, effect, inject, input, signal, untracked } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { GRAPH_DATA_TYPE_UI_BY_NAME } from '../../config/graph-data-type-ui.config';
import { GRAPH_TYPE_UI_BY_NAME } from '../../config/graph-type-ui.config';
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
    MatCardModule,
    MatProgressSpinnerModule,
    LineChartComponent,
    BarChartComponent,
    PieChartComponent,
    DoughnutChartComponent,
    DataGridComponent
  ],
  templateUrl: './graph-item.component.html',
  styleUrl: './graph-item.component.scss'
})
export class GraphItemComponent {
  private readonly graphItemService = inject(GraphItemService);

  readonly graphItem = input.required<GraphItem>();
  readonly analytics = signal<GraphItemAnalytics | null>(null);
  readonly isLoading = signal(true);
  readonly hasError = signal(false);

  readonly graphTypes = GRAPH_TYPE_TECHNICAL_NAMES;

  constructor() {
    effect(() => {
      const item = this.graphItem();
      untracked(() => this.loadAnalytics(item.id));
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
