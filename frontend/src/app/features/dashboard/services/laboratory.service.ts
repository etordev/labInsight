import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Laboratory } from '../models/laboratory.model';
import { withIsDeletedParam } from '../models/deleted-filter';

@Injectable({
  providedIn: 'root'
})
export class LaboratoryService {
  private readonly http = inject(HttpClient);

  getLaboratories(isDeleted = false): Observable<Laboratory[]> {
    return this.http.get<Laboratory[]>(
      `${environment.apiBaseUrl}/api/getLaboratories`,
      withIsDeletedParam(isDeleted)
    );
  }
}
