import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { VisualizationType } from '../models/visualization-type.model';
import { withIsDeletedParam } from '../models/deleted-filter';

@Injectable({
  providedIn: 'root'
})
export class VisualizationTypeService {
  private readonly http = inject(HttpClient);

  getVisualizationTypes(isDeleted = false): Observable<VisualizationType[]> {
    return this.http.get<VisualizationType[]>(
      `${environment.apiBaseUrl}/api/getVisualizationTypes`,
      withIsDeletedParam(isDeleted)
    );
  }
}
