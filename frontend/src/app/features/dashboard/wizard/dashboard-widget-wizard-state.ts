import { computed, Injectable, signal } from '@angular/core';
import { getAllowedVisualizationTypes } from '../config/visualization-type-compatibility';
import { supportsDateRange } from '../config/dashboard-widget-config-fields';
import { parseWizardFormValueFromContent } from '../models/build-dashboard-widget-content';
import {
  MetricDefinitionTechnicalName,
  isMetricDefinitionTechnicalName
} from '../models/metric-definition-technical-name';
import { DashboardWidget } from '../models/dashboard-widget.model';
import { VisualizationTypeTechnicalName, isVisualizationTypeTechnicalName } from '../models/visualization-type-technical-name';
import { createDashboardWidgetWizardForm, DashboardWidgetWizardForm } from './dashboard-widget-wizard-form';

@Injectable()
export class DashboardWidgetWizardState {
  readonly dashboardWidgetId = signal<number | null>(null);
  readonly selectedMetricDefinition = signal<MetricDefinitionTechnicalName | null>(null);
  readonly selectedVisualizationType = signal<VisualizationTypeTechnicalName | null>(null);
  readonly form: DashboardWidgetWizardForm = createDashboardWidgetWizardForm();
  readonly isEditing = computed(() => this.dashboardWidgetId() !== null);

  hydrate(item: DashboardWidget): void {
    this.dashboardWidgetId.set(item.id);

    const dataTypeName = item.metricDefinition.technicalName;
    const visualizationTypeName = item.visualizationType.technicalName;
    const dataType = isMetricDefinitionTechnicalName(dataTypeName) ? dataTypeName : null;
    const visualizationType = isVisualizationTypeTechnicalName(visualizationTypeName) ? visualizationTypeName : null;

    this.selectedMetricDefinition.set(dataType);
    this.selectedVisualizationType.set(
      dataType && visualizationType && getAllowedVisualizationTypes(dataType).includes(visualizationType) ? visualizationType : null
    );

    const contentFields = parseWizardFormValueFromContent(item.content);
    this.form.reset({
      name: item.name,
      description: item.description ?? '',
      ...contentFields
    });
    this.clearDatesIfUnsupported(dataType);
  }

  setMetricDefinition(metricDefinition: MetricDefinitionTechnicalName): void {
    this.selectedMetricDefinition.set(metricDefinition);
    this.clearDatesIfUnsupported(metricDefinition);

    const selectedVisualizationType = this.selectedVisualizationType();
    if (selectedVisualizationType && !getAllowedVisualizationTypes(metricDefinition).includes(selectedVisualizationType)) {
      this.selectedVisualizationType.set(null);
    }
  }

  setVisualizationType(visualizationType: VisualizationTypeTechnicalName | null): void {
    const selectedDataType = this.selectedMetricDefinition();
    if (
      visualizationType !== null &&
      selectedDataType !== null &&
      !getAllowedVisualizationTypes(selectedDataType).includes(visualizationType)
    ) {
      this.selectedVisualizationType.set(null);
      return;
    }

    this.selectedVisualizationType.set(visualizationType);
  }

  private clearDatesIfUnsupported(metricDefinition: MetricDefinitionTechnicalName | null): void {
    if (metricDefinition && supportsDateRange(metricDefinition)) {
      return;
    }

    this.form.patchValue({ dateFrom: null, dateTo: null });
  }
}
