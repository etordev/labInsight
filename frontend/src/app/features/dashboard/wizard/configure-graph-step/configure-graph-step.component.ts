import { Component, computed, inject } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { GRAPH_CONFIG_FIELDS, GraphConfigField, isConfigFieldVisible } from '../../config/graph-config-fields';
import { ANALYSIS_PRIORITY_OPTIONS } from '../../models/analysis-priority';
import { ANALYSIS_STATUS_OPTIONS } from '../../models/analysis-status';
import { GROUP_BY_OPTIONS } from '../../models/group-by';
import { GraphWizardCatalog } from '../graph-wizard-catalog';
import {
  GRAPH_DESCRIPTION_MAX_LENGTH,
  GRAPH_NAME_MAX_LENGTH
} from '../graph-wizard-form';
import { GraphWizardState } from '../graph-wizard-state';
import { WizardSelectionSummaryComponent } from '../wizard-selection-summary.component';

@Component({
  selector: 'app-configure-graph-step',
  imports: [
    ReactiveFormsModule,
    MatDatepickerModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatSelectModule,
    WizardSelectionSummaryComponent
  ],
  templateUrl: './configure-graph-step.component.html',
  styleUrl: './configure-graph-step.component.scss'
})
export class ConfigureGraphStepComponent {
  private readonly state = inject(GraphWizardState);
  readonly catalog = inject(GraphWizardCatalog);

  readonly form = this.state.form;
  readonly fields = GRAPH_CONFIG_FIELDS;
  readonly nameMaxLength = GRAPH_NAME_MAX_LENGTH;
  readonly descriptionMaxLength = GRAPH_DESCRIPTION_MAX_LENGTH;
  readonly priorityOptions = ANALYSIS_PRIORITY_OPTIONS;
  readonly statusOptions = ANALYSIS_STATUS_OPTIONS;
  readonly groupByOptions = GROUP_BY_OPTIONS;

  readonly visible = computed(() => {
    const dataType = this.state.selectedGraphDataType();
    return (field: GraphConfigField) => (dataType ? isConfigFieldVisible(dataType, field) : false);
  });

  isVisible(field: GraphConfigField): boolean {
    return this.visible()(field);
  }
}
