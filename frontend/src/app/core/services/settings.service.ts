// frontend/src/app/core/services/settings.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class SettingsService {
  private readonly api = 'http://localhost:5000/api/settings';

  constructor(private http: HttpClient) {}

  getPreavisDefault(): Observable<{ mois: number }> {
    return this.http.get<{ mois: number }>(`${this.api}/preavis-default`);
  }

  updatePreavisDefault(mois: number): Observable<{ mois: number }> {
    return this.http.patch<{ mois: number }>(
      `${this.api}/preavis-default`,
      { mois },
    );
  }
}
