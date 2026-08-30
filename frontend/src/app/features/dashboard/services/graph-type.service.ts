import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { GraphType } from '../models/graph-type.model';
import { withIsDeletedParam } from '../models/deleted-filter';

@Injectable({
  providedIn: 'root'
})
export class GraphTypeService {
  private readonly http = inject(HttpClient);

  getGraphTypes(isDeleted = false): Observable<GraphType[]> {
    return this.http.get<GraphType[]>(
      `${environment.apiBaseUrl}/api/getGraphTypes`,
      withIsDeletedParam(isDeleted)
    );
  }
}
