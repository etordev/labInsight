import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { DashboardWidget } from '../models/dashboard-widget.model';
import { DashboardWidgetAnalytics } from '../models/dashboard-widget-analytics.model';
import { DashboardWidgetOrderingUpdate } from '../models/dashboard-widget-ordering-update.model';
import { withIsDeletedParam } from '../models/deleted-filter';
import { UpsertDashboardWidgetRequest } from '../models/upsert-dashboard-widget-request.model';

@Injectable({
  providedIn: 'root'
})
export class DashboardWidgetService {
  private readonly http = inject(HttpClient);
  private readonly apiBaseUrl = environment.apiBaseUrl;

  getDashboardWidgets(isDeleted = false): Observable<DashboardWidget[]> {
    return this.http.get<DashboardWidget[]>(
      `${this.apiBaseUrl}/api/getDashboardWidgets`,
      withIsDeletedParam(isDeleted)
    );
  }

  upsertDashboardWidget(request: UpsertDashboardWidgetRequest): Observable<DashboardWidget> {
    return this.http.post<DashboardWidget>(`${this.apiBaseUrl}/api/upsertDashboardWidget`, request);
  }

  getDashboardWidgetData(id: number, isDeleted = false): Observable<DashboardWidgetAnalytics> {
    return this.http.get<DashboardWidgetAnalytics>(
      `${this.apiBaseUrl}/api/getDashboardWidgetData/${id}`,
      withIsDeletedParam(isDeleted)
    );
  }

  deleteDashboardWidget(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiBaseUrl}/api/deleteDashboardWidget/${id}`);
  }

  updateDashboardWidgetOrdering(items: DashboardWidgetOrderingUpdate[]): Observable<void> {
    return this.http.post<void>(`${this.apiBaseUrl}/api/updateDashboardWidgetOrdering`, items);
  }
}
