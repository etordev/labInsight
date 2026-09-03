import { Component, input } from '@angular/core';
import { AnalyticsPoint } from '../../models/dashboard-widget-analytics.model';
import { ChartViewComponent } from './chart-view.component';

@Component({
  selector: 'app-bar-chart',
  imports: [ChartViewComponent],
  host: { style: 'display:block;width:100%;min-width:0;max-width:100%;height:100%;flex:1' },
  template: `<app-chart-view type="bar" [points]="points()" [unit]="unit()" />`
})
export class BarChartComponent {
  readonly points = input.required<AnalyticsPoint[]>();
  readonly unit = input('analyses');
}
