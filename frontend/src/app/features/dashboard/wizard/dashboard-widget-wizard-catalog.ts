import { inject, Injectable, signal } from '@angular/core';
import { forkJoin } from 'rxjs';
import { AnalysisCategory } from '../models/analysis-category.model';
import { MetricDefinition } from '../models/metric-definition.model';
import { MetricDefinitionTechnicalName } from '../models/metric-definition-technical-name';
import { VisualizationType } from '../models/visualization-type.model';
import { VisualizationTypeTechnicalName } from '../models/visualization-type-technical-name';
import { Laboratory } from '../models/laboratory.model';
import { AnalysisCategoryService } from '../services/analysis-category.service';
import { MetricDefinitionService } from '../services/metric-definition.service';
import { VisualizationTypeService } from '../services/visualization-type.service';
import { LaboratoryService } from '../services/laboratory.service';

@Injectable()
export class DashboardWidgetWizardCatalog {
  private readonly laboratoryService = inject(LaboratoryService);
  private readonly analysisCategoryService = inject(AnalysisCategoryService);
  private readonly visualizationTypeService = inject(VisualizationTypeService);
  private readonly metricDefinitionService = inject(MetricDefinitionService);

  readonly laboratories = signal<Laboratory[]>([]);
  readonly analysisCategories = signal<AnalysisCategory[]>([]);
  readonly visualizationTypes = signal<VisualizationType[]>([]);
  readonly metricDefinitions = signal<MetricDefinition[]>([]);
  readonly isLoading = signal(false);
  readonly hasError = signal(false);

  load(): void {
    this.isLoading.set(true);
    this.hasError.set(false);

    forkJoin({
      laboratories: this.laboratoryService.getLaboratories(),
      analysisCategories: this.analysisCategoryService.getAnalysisCategories(),
      visualizationTypes: this.visualizationTypeService.getVisualizationTypes(),
      metricDefinitions: this.metricDefinitionService.getMetricDefinitions()
    }).subscribe({
      next: (catalog) => {
        this.laboratories.set(catalog.laboratories);
        this.analysisCategories.set(catalog.analysisCategories);
        this.visualizationTypes.set(catalog.visualizationTypes);
        this.metricDefinitions.set(catalog.metricDefinitions);
        this.isLoading.set(false);
      },
      error: () => {
        this.hasError.set(true);
        this.isLoading.set(false);
      }
    });
  }

  visualizationTypeId(technicalName: VisualizationTypeTechnicalName): number | null {
    return this.visualizationTypes().find((type) => type.technicalName === technicalName)?.id ?? null;
  }

  metricDefinitionId(technicalName: MetricDefinitionTechnicalName): number | null {
    return (
      this.metricDefinitions().find((type) => type.technicalName === technicalName)?.id ?? null
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
