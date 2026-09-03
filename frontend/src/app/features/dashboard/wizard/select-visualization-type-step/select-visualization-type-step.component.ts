import { Component, computed, input, output } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { getAllowedVisualizationTypes } from '../../config/visualization-type-compatibility';
import { VISUALIZATION_TYPE_UI_BY_NAME, VisualizationTypeUiConfig } from '../../config/visualization-type-ui.config';
import { MetricDefinitionTechnicalName } from '../../models/metric-definition-technical-name';
import { VisualizationTypeTechnicalName } from '../../models/visualization-type-technical-name';
import { WizardSelectionSummaryComponent } from '../wizard-selection-summary.component';

@Component({
  selector: 'app-select-visualization-type-step',
  imports: [MatIconModule, WizardSelectionSummaryComponent],
  templateUrl: './select-visualization-type-step.component.html',
  styleUrl: './select-visualization-type-step.component.scss'
})
export class SelectVisualizationTypeStepComponent {
  readonly selectedMetricDefinition = input<MetricDefinitionTechnicalName | null>(null);
  readonly selected = input<VisualizationTypeTechnicalName | null>(null);
  readonly selectedChange = output<VisualizationTypeTechnicalName>();

  readonly options = computed<VisualizationTypeUiConfig[]>(() => {
    const dataType = this.selectedMetricDefinition();
    if (!dataType) {
      return [];
    }

    return getAllowedVisualizationTypes(dataType).map((technicalName) => VISUALIZATION_TYPE_UI_BY_NAME[technicalName]);
  });

  select(technicalName: VisualizationTypeTechnicalName): void {
    this.selectedChange.emit(technicalName);
  }

  onCardKeydown(event: KeyboardEvent, technicalName: VisualizationTypeTechnicalName): void {
    if (event.key === 'Enter' || event.key === ' ') {
      event.preventDefault();
      this.select(technicalName);
    }
  }
}
