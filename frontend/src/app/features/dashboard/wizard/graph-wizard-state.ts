import { Injectable, signal } from '@angular/core';
import { getAllowedGraphTypes } from '../config/graph-type-compatibility';
import { GraphDataTypeTechnicalName } from '../models/graph-data-type-technical-name';
import { GraphTypeTechnicalName } from '../models/graph-type-technical-name';

@Injectable()
export class GraphWizardState {
  readonly selectedGraphDataType = signal<GraphDataTypeTechnicalName | null>(null);
  readonly selectedGraphType = signal<GraphTypeTechnicalName | null>(null);

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
