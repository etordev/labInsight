import { computed, Injectable, signal } from '@angular/core';
import { getAllowedGraphTypes } from '../config/graph-type-compatibility';
import { parseWizardFormValueFromContent } from '../models/build-graph-item-content';
import {
  GraphDataTypeTechnicalName,
  isGraphDataTypeTechnicalName
} from '../models/graph-data-type-technical-name';
import { GraphItem } from '../models/graph-item.model';
import { GraphTypeTechnicalName, isGraphTypeTechnicalName } from '../models/graph-type-technical-name';
import { createGraphWizardForm, GraphWizardForm } from './graph-wizard-form';

@Injectable()
export class GraphWizardState {
  readonly graphItemId = signal<number | null>(null);
  readonly selectedGraphDataType = signal<GraphDataTypeTechnicalName | null>(null);
  readonly selectedGraphType = signal<GraphTypeTechnicalName | null>(null);
  readonly form: GraphWizardForm = createGraphWizardForm();
  readonly isEditing = computed(() => this.graphItemId() !== null);

  hydrate(item: GraphItem): void {
    this.graphItemId.set(item.id);

    const dataTypeName = item.graphDataType.technicalName;
    const graphTypeName = item.graphType.technicalName;
    const dataType = isGraphDataTypeTechnicalName(dataTypeName) ? dataTypeName : null;
    const graphType = isGraphTypeTechnicalName(graphTypeName) ? graphTypeName : null;

    this.selectedGraphDataType.set(dataType);
    this.selectedGraphType.set(
      dataType && graphType && getAllowedGraphTypes(dataType).includes(graphType) ? graphType : null
    );

    const contentFields = parseWizardFormValueFromContent(item.content);
    this.form.reset({
      name: item.name,
      description: item.description ?? '',
      ...contentFields
    });
  }

  setGraphDataType(graphDataType: GraphDataTypeTechnicalName): void {
    this.selectedGraphDataType.set(graphDataType);

    const selectedGraphType = this.selectedGraphType();
    if (selectedGraphType && !getAllowedGraphTypes(graphDataType).includes(selectedGraphType)) {
      this.selectedGraphType.set(null);
    }
  }

  setGraphType(graphType: GraphTypeTechnicalName | null): void {
    const selectedDataType = this.selectedGraphDataType();
    if (
      graphType !== null &&
      selectedDataType !== null &&
      !getAllowedGraphTypes(selectedDataType).includes(graphType)
    ) {
      this.selectedGraphType.set(null);
      return;
    }

    this.selectedGraphType.set(graphType);
  }
}
