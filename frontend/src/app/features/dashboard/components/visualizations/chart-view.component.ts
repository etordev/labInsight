import { Component, computed, input } from '@angular/core';
import { ChartConfiguration, ChartData, ChartType } from 'chart.js';
import { BaseChartDirective } from 'ng2-charts';
import { AnalyticsPoint } from '../../models/graph-item-analytics.model';
import { CHART_COLORS } from './chart-colors';

@Component({
  selector: 'app-chart-view',
  imports: [BaseChartDirective],
  host: { style: 'display:block;width:100%;min-width:0;max-width:100%;height:100%;flex:1' },
  template: `
    <div class="chart-scroll">
      <div class="chart-host" [style.min-width.px]="minWidthPx()">
        <canvas
          baseChart
          [type]="type()"
          [data]="chartData()"
          [options]="chartOptions()"
        ></canvas>
      </div>
    </div>
  `,
  styles: `
    .chart-scroll {
      width: 100%;
      height: 100%;
      min-width: 0;
      min-height: 0;
    }

    .chart-host {
      position: relative;
      box-sizing: border-box;
      min-width: 35rem;
      height: 100%;
    }
  `
})
export class ChartViewComponent {
  readonly type = input.required<ChartType>();
  readonly points = input.required<AnalyticsPoint[]>();
  readonly unit = input('analyses');

  readonly isCircular = computed(() => {
    const chartType = this.type();
    return chartType === 'pie' || chartType === 'doughnut';
  });

  readonly minWidthPx = computed(() => Math.max(560, this.points().length * 56));

  readonly chartData = computed<ChartData>(() => {
    const points = this.points();
    const labels = points.map((point) => point.label);
    const values = points.map((point) => point.value);
    const circular = this.isCircular();

    return {
      labels,
      datasets: [
        {
          data: values,
          label: this.unit(),
          backgroundColor: circular ? CHART_COLORS.slice(0, values.length) : 'rgba(21, 101, 192, 0.18)',
          borderColor: circular ? CHART_COLORS.slice(0, values.length) : '#1565c0',
          borderWidth: circular ? 1 : 2,
          fill: this.type() === 'line',
          tension: 0.25,
          pointRadius: this.type() === 'line' ? 3 : 0,
          borderRadius: this.type() === 'bar' ? 4 : 0
        }
      ]
    };
  });

  readonly chartOptions = computed<ChartConfiguration['options']>(() => {
    const circular = this.isCircular();
    const unit = this.unit();

    return {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: {
          display: circular,
          position: 'bottom'
        },
        tooltip: {
          callbacks: {
            label: (context) => {
              const parsed = context.parsed as number | { y?: number };
              const value = typeof parsed === 'number' ? parsed : parsed.y;
              return `${context.label}: ${value} ${unit}`;
            }
          }
        }
      },
      scales: circular
        ? {}
        : {
            x: {
              ticks: { maxRotation: 45, minRotation: 0 },
              grid: { display: false }
            },
            y: {
              beginAtZero: true,
              suggestedMax: unit === 'percent' ? 100 : undefined
            }
          }
    };
  });
}
