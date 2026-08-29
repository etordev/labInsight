import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { GraphDataType } from '../models/graph-data-type.model';

@Injectable({
  providedIn: 'root'
})
export class GraphDataTypeService {
  private readonly http = inject(HttpClient);

  getGraphDataTypes(): Observable<GraphDataType[]> {
    return this.http.get<GraphDataType[]>(`${environment.apiBaseUrl}/api/graph-data-types`);
  }
}
