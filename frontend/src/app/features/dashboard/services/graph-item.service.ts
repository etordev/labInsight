import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { GraphItem } from '../models/graph-item.model';
import { GraphItemAnalytics } from '../models/graph-item-analytics.model';
import { withIsDeletedParam } from '../models/deleted-filter';
import { UpsertGraphItemRequest } from '../models/upsert-graph-item-request.model';

@Injectable({
  providedIn: 'root'
})
export class GraphItemService {
  private readonly http = inject(HttpClient);
  private readonly apiBaseUrl = environment.apiBaseUrl;

  getGraphItems(isDeleted = false): Observable<GraphItem[]> {
    return this.http.get<GraphItem[]>(
      `${this.apiBaseUrl}/api/getGraphItems`,
      withIsDeletedParam(isDeleted)
    );
  }

  upsertGraphItem(request: UpsertGraphItemRequest): Observable<GraphItem> {
    return this.http.post<GraphItem>(`${this.apiBaseUrl}/api/upsertGraphItem`, request);
  }

  getGraphItemData(id: number, isDeleted = false): Observable<GraphItemAnalytics> {
    return this.http.get<GraphItemAnalytics>(
      `${this.apiBaseUrl}/api/getGraphItemData/${id}`,
      withIsDeletedParam(isDeleted)
    );
  }

  deleteGraphItem(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiBaseUrl}/api/deleteGraphItem/${id}`);
  }
}
