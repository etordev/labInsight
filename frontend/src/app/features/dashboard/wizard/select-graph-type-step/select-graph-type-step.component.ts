import { Component, computed, input, output } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { GRAPH_DATA_TYPE_UI_BY_NAME } from '../../config/graph-data-type-ui.config';
import { getAllowedGraphTypes } from '../../config/graph-type-compatibility';
import { GRAPH_TYPE_UI_BY_NAME, GraphTypeUiConfig } from '../../config/graph-type-ui.config';
import { GraphDataTypeTechnicalName } from '../../models/graph-data-type-technical-name';
import { GraphTypeTechnicalName } from '../../models/graph-type-technical-name';

@Component({
  selector: 'app-select-graph-type-step',
  imports: [MatIconModule],
  templateUrl: './select-graph-type-step.component.html',
  styleUrl: './select-graph-type-step.component.scss'
})
export class SelectGraphTypeStepComponent {
  readonly selectedGraphDataType = input<GraphDataTypeTechnicalName | null>(null);
  readonly selected = input<GraphTypeTechnicalName | null>(null);
  readonly selectedChange = output<GraphTypeTechnicalName>();

  readonly selectedDataLabel = computed(() => {
    const dataType = this.selectedGraphDataType();
    return dataType ? GRAPH_DATA_TYPE_UI_BY_NAME[dataType].label : null;
  });

  readonly options = computed<GraphTypeUiConfig[]>(() => {
    const dataType = this.selectedGraphDataType();
    if (!dataType) {
      return [];
    }

    return getAllowedGraphTypes(dataType).map((technicalName) => GRAPH_TYPE_UI_BY_NAME[technicalName]);
  });

  select(technicalName: GraphTypeTechnicalName): void {
    this.selectedChange.emit(technicalName);
  }

  onCardKeydown(event: KeyboardEvent, technicalName: GraphTypeTechnicalName): void {
    if (event.key === 'Enter' || event.key === ' ') {
      event.preventDefault();
      this.select(technicalName);
    }
  }
}
