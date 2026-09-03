import { DatePipe } from '@angular/common';
import { Component, input } from '@angular/core';
import { MatTableModule } from '@angular/material/table';
import { AnalyticsPoint, DelayedAnalysisRow } from '../../models/dashboard-widget-analytics.model';

@Component({
  selector: 'app-data-grid',
  imports: [DatePipe, MatTableModule],
  host: { style: 'display:block;width:100%;min-width:0;max-width:100%;height:100%' },
  templateUrl: './data-grid.component.html',
  styleUrl: './data-grid.component.scss'
})
export class DataGridComponent {
  readonly points = input<AnalyticsPoint[]>([]);
  readonly rows = input<DelayedAnalysisRow[] | null | undefined>(null);
  readonly unit = input('analyses');

  readonly summaryColumns = ['label', 'value'];
  readonly delayedColumns = [
    'analysisNumber',
    'laboratory',
    'category',
    'receivedAt',
    'priority',
    'status',
    'expectedProcessingHours',
    'elapsedProcessingHours'
  ];
}
