import { inject, Injectable, signal } from '@angular/core';
import { forkJoin } from 'rxjs';
import { AnalysisCategory } from '../models/analysis-category.model';
import { GraphDataType } from '../models/graph-data-type.model';
import { GraphDataTypeTechnicalName } from '../models/graph-data-type-technical-name';
import { GraphType } from '../models/graph-type.model';
import { GraphTypeTechnicalName } from '../models/graph-type-technical-name';
import { Laboratory } from '../models/laboratory.model';
import { AnalysisCategoryService } from '../services/analysis-category.service';
import { GraphDataTypeService } from '../services/graph-data-type.service';
import { GraphTypeService } from '../services/graph-type.service';
import { LaboratoryService } from '../services/laboratory.service';

@Injectable()
export class GraphWizardCatalog {
  private readonly laboratoryService = inject(LaboratoryService);
  private readonly analysisCategoryService = inject(AnalysisCategoryService);
  private readonly graphTypeService = inject(GraphTypeService);
  private readonly graphDataTypeService = inject(GraphDataTypeService);

  readonly laboratories = signal<Laboratory[]>([]);
  readonly analysisCategories = signal<AnalysisCategory[]>([]);
  readonly graphTypes = signal<GraphType[]>([]);
  readonly graphDataTypes = signal<GraphDataType[]>([]);
  readonly isLoading = signal(false);
  readonly hasError = signal(false);

  load(): void {
    this.isLoading.set(true);
    this.hasError.set(false);

    forkJoin({
      laboratories: this.laboratoryService.getLaboratories(),
      analysisCategories: this.analysisCategoryService.getAnalysisCategories(),
      graphTypes: this.graphTypeService.getGraphTypes(),
      graphDataTypes: this.graphDataTypeService.getGraphDataTypes()
    }).subscribe({
      next: (catalog) => {
        this.laboratories.set(catalog.laboratories);
        this.analysisCategories.set(catalog.analysisCategories);
        this.graphTypes.set(catalog.graphTypes);
        this.graphDataTypes.set(catalog.graphDataTypes);
        this.isLoading.set(false);
      },
      error: () => {
        this.hasError.set(true);
        this.isLoading.set(false);
      }
    });
  }

  graphTypeId(technicalName: GraphTypeTechnicalName): number | null {
    return this.graphTypes().find((type) => type.technicalName === technicalName)?.id ?? null;
  }

  graphDataTypeId(technicalName: GraphDataTypeTechnicalName): number | null {
    return (
      this.graphDataTypes().find((type) => type.technicalName === technicalName)?.id ?? null
    );
  }

  laboratoryName(id: number): string | null {
    const laboratory = this.laboratories().find((item) => item.id === id);
    return laboratory ? laboratory.name : null;
  }

  analysisCategoryName(id: number): string | null {
    return this.analysisCategories().find((item) => item.id === id)?.name ?? null;
  }
}
