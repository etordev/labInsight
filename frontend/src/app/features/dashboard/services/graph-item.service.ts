import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { GraphItem } from '../models/graph-item.model';

@Injectable({
  providedIn: 'root'
})
export class GraphItemService {
  private readonly http = inject(HttpClient);
  private readonly apiBaseUrl = environment.apiBaseUrl;

  getGraphItems(): Observable<GraphItem[]> {
    return this.http.get<GraphItem[]>(`${this.apiBaseUrl}/api/graph-items`);
  }
}
