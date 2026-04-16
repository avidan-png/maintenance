import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AlertSettings, AlertSummary } from '../models/alert.model';

@Injectable({ providedIn: 'root' })
export class AlertsService {
  private readonly api = 'http://localhost:5000/api/alerts';

  constructor(private http: HttpClient) {}

  getSummary(): Observable<AlertSummary> {
    return this.http.get<AlertSummary>(this.api);
  }

  getSettings(): Observable<AlertSettings> {
    return this.http.get<AlertSettings>(`${this.api}/settings`);
  }

  updateSettings(settings: AlertSettings): Observable<AlertSettings> {
    return this.http.patch<AlertSettings>(`${this.api}/settings`, settings);
  }
}
