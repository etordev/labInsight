import { Component, computed, inject, input, output } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { METRIC_DEFINITION_UI_CONFIG } from '../../config/metric-definition-ui.config';
import { getAllowedVisualizationTypes } from '../../config/visualization-type-compatibility';
import { VISUALIZATION_TYPE_UI_BY_NAME, VisualizationTypeUiConfig } from '../../config/visualization-type-ui.config';
import { MetricDefinitionTechnicalName } from '../../models/metric-definition-technical-name';
import { VisualizationTypeTechnicalName } from '../../models/visualization-type-technical-name';
import { DashboardWidgetWizardCatalog } from '../dashboard-widget-wizard-catalog';

@Component({
  selector: 'app-choose-content-step',
  imports: [MatIconModule, MatProgressSpinnerModule],
  templateUrl: './choose-content-step.component.html',
  styleUrl: './choose-content-step.component.scss'
})
export class ChooseContentStepComponent {
  private readonly catalog = inject(DashboardWidgetWizardCatalog);

  readonly selectedMetricDefinition = input<MetricDefinitionTechnicalName | null>(null);
  readonly selectedVisualizationType = input<VisualizationTypeTechnicalName | null>(null);
  readonly selectedMetricDefinitionChange = output<MetricDefinitionTechnicalName>();
  readonly selectedVisualizationTypeChange = output<VisualizationTypeTechnicalName>();

  readonly metricOptions = METRIC_DEFINITION_UI_CONFIG;
  readonly isLoadingVisualizations = this.catalog.isLoading;

  readonly visualizationOptions = computed<VisualizationTypeUiConfig[]>(() => {
    const metricDefinition = this.selectedMetricDefinition();
    if (!metricDefinition || this.catalog.isLoading()) {
      return [];
    }

    const allowed = getAllowedVisualizationTypes(metricDefinition);
    const catalogTypes = this.catalog.visualizationTypes();
    const names = catalogTypes.length
      ? allowed.filter((technicalName) =>
          catalogTypes.some((type) => type.technicalName === technicalName)
        )
      : allowed;

    return names.map((technicalName) => VISUALIZATION_TYPE_UI_BY_NAME[technicalName]);
  });

  selectMetric(technicalName: MetricDefinitionTechnicalName): void {
    this.selectedMetricDefinitionChange.emit(technicalName);
  }

  selectVisualization(technicalName: VisualizationTypeTechnicalName): void {
    this.selectedVisualizationTypeChange.emit(technicalName);
  }

  onMetricKeydown(event: KeyboardEvent, technicalName: MetricDefinitionTechnicalName): void {
    if (event.key === 'Enter' || event.key === ' ') {
      event.preventDefault();
      this.selectMetric(technicalName);
    }
  }

  onVisualizationKeydown(event: KeyboardEvent, technicalName: VisualizationTypeTechnicalName): void {
    if (event.key === 'Enter' || event.key === ' ') {
      event.preventDefault();
      this.selectVisualization(technicalName);
    }
  }
}
