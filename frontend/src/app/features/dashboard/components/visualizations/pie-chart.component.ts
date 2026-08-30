import { Component, input } from '@angular/core';
import { AnalyticsPoint } from '../../models/graph-item-analytics.model';
import { ChartViewComponent } from './chart-view.component';

@Component({
  selector: 'app-pie-chart',
  imports: [ChartViewComponent],
  host: { style: 'display:block;width:100%;min-width:0;max-width:100%' },
  template: `<app-chart-view type="pie" [points]="points()" [unit]="unit()" />`
})
export class PieChartComponent {
  readonly points = input.required<AnalyticsPoint[]>();
  readonly unit = input('analyses');
}
