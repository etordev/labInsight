import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Laboratory } from '../models/laboratory.model';

@Injectable({
  providedIn: 'root'
})
export class LaboratoryService {
  private readonly http = inject(HttpClient);

  getLaboratories(): Observable<Laboratory[]> {
    return this.http.get<Laboratory[]>(`${environment.apiBaseUrl}/api/laboratories`);
  }
}
