import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { MetricDefinition } from '../models/metric-definition.model';
import { withIsDeletedParam } from '../models/deleted-filter';

@Injectable({
  providedIn: 'root'
})
export class MetricDefinitionService {
  private readonly http = inject(HttpClient);

  getMetricDefinitions(isDeleted = false): Observable<MetricDefinition[]> {
    return this.http.get<MetricDefinition[]>(
      `${environment.apiBaseUrl}/api/getMetricDefinitions`,
      withIsDeletedParam(isDeleted)
    );
  }
}
