import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { AnalysisCategory } from '../models/analysis-category.model';
import { withIsDeletedParam } from '../models/deleted-filter';

@Injectable({
  providedIn: 'root'
})
export class AnalysisCategoryService {
  private readonly http = inject(HttpClient);

  getAnalysisCategories(isDeleted = false): Observable<AnalysisCategory[]> {
    return this.http.get<AnalysisCategory[]>(
      `${environment.apiBaseUrl}/api/getAnalysisCategories`,
      withIsDeletedParam(isDeleted)
    );
  }
}
