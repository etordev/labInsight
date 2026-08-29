import { Component, input, output } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { GRAPH_DATA_TYPE_UI_CONFIG } from '../../config/graph-data-type-ui.config';
import { GraphDataTypeTechnicalName } from '../../models/graph-data-type-technical-name';

@Component({
  selector: 'app-select-data-step',
  imports: [MatIconModule],
  templateUrl: './select-data-step.component.html',
  styleUrl: './select-data-step.component.scss'
})
export class SelectDataStepComponent {
  readonly selected = input<GraphDataTypeTechnicalName | null>(null);
  readonly selectedChange = output<GraphDataTypeTechnicalName>();

  readonly options = GRAPH_DATA_TYPE_UI_CONFIG;

  select(technicalName: GraphDataTypeTechnicalName): void {
    this.selectedChange.emit(technicalName);
  }

  onCardKeydown(event: KeyboardEvent, technicalName: GraphDataTypeTechnicalName): void {
    if (event.key === 'Enter' || event.key === ' ') {
      event.preventDefault();
      this.select(technicalName);
    }
  }
}
