import { Component, computed, inject, input } from '@angular/core';
import { GRAPH_DATA_TYPE_UI_BY_NAME } from '../config/graph-data-type-ui.config';
import { GRAPH_TYPE_UI_BY_NAME } from '../config/graph-type-ui.config';
import { GraphWizardState } from './graph-wizard-state';

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
  private readonly state = inject(GraphWizardState);

  readonly showGraphType = input(false);

  readonly rows = computed(() => {
    const items: { label: string; value: string }[] = [];
    const dataType = this.state.selectedGraphDataType();
    if (dataType) {
      items.push({
        label: 'Selected data',
        value: GRAPH_DATA_TYPE_UI_BY_NAME[dataType].label
      });
    }

    if (this.showGraphType()) {
      const graphType = this.state.selectedGraphType();
      if (graphType) {
        items.push({
          label: 'Selected graph type',
          value: GRAPH_TYPE_UI_BY_NAME[graphType].label
        });
      }
    }

    return items;
  });
}
