import { Component, input, output } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { METRIC_DEFINITION_UI_CONFIG } from '../../config/metric-definition-ui.config';
import { MetricDefinitionTechnicalName } from '../../models/metric-definition-technical-name';

@Component({
  selector: 'app-select-data-step',
  imports: [MatIconModule],
  templateUrl: './select-data-step.component.html',
  styleUrl: './select-data-step.component.scss'
})
export class SelectDataStepComponent {
  readonly selected = input<MetricDefinitionTechnicalName | null>(null);
  readonly selectedChange = output<MetricDefinitionTechnicalName>();

  readonly options = METRIC_DEFINITION_UI_CONFIG;

  select(technicalName: MetricDefinitionTechnicalName): void {
    this.selectedChange.emit(technicalName);
  }

  onCardKeydown(event: KeyboardEvent, technicalName: MetricDefinitionTechnicalName): void {
    if (event.key === 'Enter' || event.key === ' ') {
      event.preventDefault();
      this.select(technicalName);
    }
  }
}
