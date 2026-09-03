import { Component, computed, inject, input } from '@angular/core';
import { METRIC_DEFINITION_UI_BY_NAME } from '../config/metric-definition-ui.config';
import { VISUALIZATION_TYPE_UI_BY_NAME } from '../config/visualization-type-ui.config';
import { DashboardWidgetWizardState } from './dashboard-widget-wizard-state';

@Component({
  selector: 'app-wizard-selection-summary',
  template: `
    @if (rows().length > 0) {
      <dl class="selection-summary">
        @for (row of rows(); track row.label) {
          <div class="selection-summary-row">
            <dt>{{ row.label }}</dt>
            <dd>{{ row.value }}</dd>
          </div>
        }
      </dl>
    }
  `,
  styles: `
    .selection-summary {
      display: flex;
      flex-wrap: wrap;
      gap: 0.65rem 1.5rem;
      margin: 0.75rem 0 0;
      padding: 0.7rem 0.85rem;
      border: 1px solid color-mix(in srgb, var(--mat-sys-outline-variant) 80%, transparent);
      border-radius: 0.75rem;
      background: color-mix(in srgb, var(--mat-sys-primary) 6%, var(--mat-sys-surface));
    }

    .selection-summary-row {
      display: grid;
      gap: 0.1rem;
    }

    .selection-summary-row dt {
      color: var(--mat-sys-on-surface-variant);
      font-size: 0.7rem;
      font-weight: 600;
      letter-spacing: 0.02em;
    }

    .selection-summary-row dd {
      margin: 0;
      font-size: 0.875rem;
      font-weight: 500;
      line-height: 1.3;
    }
  `
})
export class WizardSelectionSummaryComponent {
  private readonly state = inject(DashboardWidgetWizardState);

  readonly showVisualizationType = input(false);

  readonly rows = computed(() => {
    const items: { label: string; value: string }[] = [];
    const dataType = this.state.selectedMetricDefinition();
    if (dataType) {
      items.push({
        label: 'Selected data',
        value: METRIC_DEFINITION_UI_BY_NAME[dataType].label
      });
    }

    if (this.showVisualizationType()) {
      const visualizationType = this.state.selectedVisualizationType();
      if (visualizationType) {
        items.push({
          label: 'Selected visualization type',
          value: VISUALIZATION_TYPE_UI_BY_NAME[visualizationType].label
        });
      }
    }

    return items;
  });
}
