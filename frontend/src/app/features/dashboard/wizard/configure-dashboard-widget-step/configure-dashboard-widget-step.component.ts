import { Component, computed, inject } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { DASHBOARD_WIDGET_CONFIG_FIELDS, DashboardWidgetConfigField, isConfigFieldVisible } from '../../config/dashboard-widget-config-fields';
import { ANALYSIS_PRIORITY_OPTIONS } from '../../models/analysis-priority';
import { ANALYSIS_STATUS_OPTIONS } from '../../models/analysis-status';
import { GROUP_BY_OPTIONS } from '../../models/group-by';
import { DashboardWidgetWizardCatalog } from '../dashboard-widget-wizard-catalog';
import {
  DASHBOARD_WIDGET_DESCRIPTION_MAX_LENGTH,
  DASHBOARD_WIDGET_NAME_MAX_LENGTH
} from '../dashboard-widget-wizard-form';
import { DashboardWidgetWizardState } from '../dashboard-widget-wizard-state';
import { WizardSelectionSummaryComponent } from '../wizard-selection-summary.component';

@Component({
  selector: 'app-configure-dashboard-widget-step',
  imports: [
    ReactiveFormsModule,
    MatDatepickerModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatSelectModule,
    WizardSelectionSummaryComponent
  ],
  templateUrl: './configure-dashboard-widget-step.component.html',
  styleUrl: './configure-dashboard-widget-step.component.scss'
})
export class ConfigureDashboardWidgetStepComponent {
  private readonly state = inject(DashboardWidgetWizardState);
  readonly catalog = inject(DashboardWidgetWizardCatalog);

  readonly form = this.state.form;
  readonly fields = DASHBOARD_WIDGET_CONFIG_FIELDS;
  readonly nameMaxLength = DASHBOARD_WIDGET_NAME_MAX_LENGTH;
  readonly descriptionMaxLength = DASHBOARD_WIDGET_DESCRIPTION_MAX_LENGTH;
  readonly priorityOptions = ANALYSIS_PRIORITY_OPTIONS;
  readonly statusOptions = ANALYSIS_STATUS_OPTIONS;
  readonly groupByOptions = GROUP_BY_OPTIONS;

  readonly visible = computed(() => {
    const dataType = this.state.selectedMetricDefinition();
    return (field: DashboardWidgetConfigField) => (dataType ? isConfigFieldVisible(dataType, field) : false);
  });

  isVisible(field: DashboardWidgetConfigField): boolean {
    return this.visible()(field);
  }
}
