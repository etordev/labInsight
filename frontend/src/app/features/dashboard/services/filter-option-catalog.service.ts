import { inject, Injectable, signal } from '@angular/core';
import { forkJoin } from 'rxjs';
import { AnalysisCategory } from '../models/analysis-category.model';
import { Laboratory } from '../models/laboratory.model';
import { AnalysisCategoryService } from './analysis-category.service';
import { LaboratoryService } from './laboratory.service';

@Injectable({ providedIn: 'root' })
export class FilterOptionCatalog {
  private readonly laboratoryService = inject(LaboratoryService);
  private readonly analysisCategoryService = inject(AnalysisCategoryService);

  readonly laboratories = signal<Laboratory[]>([]);
  readonly analysisCategories = signal<AnalysisCategory[]>([]);

  constructor() {
    forkJoin({
      laboratories: this.laboratoryService.getLaboratories(),
      analysisCategories: this.analysisCategoryService.getAnalysisCategories()
    }).subscribe({
      next: (catalog) => {
        this.laboratories.set(catalog.laboratories);
        this.analysisCategories.set(catalog.analysisCategories);
      }
    });
  }

  laboratoryName(id: number): string | null {
    return this.laboratories().find((item) => item.id === id)?.name ?? null;
  }

  analysisCategoryName(id: number): string | null {
    return this.analysisCategories().find((item) => item.id === id)?.name ?? null;
  }
}
