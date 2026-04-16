// frontend/src/app/core/services/contracts.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Contract, ImportResult } from '../models/contract.model';

@Injectable({ providedIn: 'root' })
export class ContractsService {
  private readonly api = 'http://localhost:5000/api/contracts';

  constructor(private http: HttpClient) {}

  getAll(): Observable<Contract[]> {
    return this.http.get<Contract[]>(this.api);
  }

  importFromExcel(file: File, delaiPreavisMois: number): Observable<ImportResult> {
    const form = new FormData();
    form.append('file', file);
    form.append('delaiPreavisMois', String(delaiPreavisMois));
    return this.http.post<ImportResult>(`${this.api}/import/excel`, form);
  }
}
