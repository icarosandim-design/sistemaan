import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CentralResumo } from './central.model';

/**
 * Fornece o resumo da Central Operacional a partir da API real
 * (`GET /api/central/resumo`).
 *
 * A Central NÃO contém regra de negócio: o backend apenas agrega o que os
 * módulos donos (Clientes, Pets, Produção, Estoque, Entregas, Rotas) calculam.
 */
@Injectable({ providedIn: 'root' })
export class CentralService {
  private readonly http = inject(HttpClient);
  private readonly api = `${environment.apiUrl}/central`;

  /** Resumo do dia informado (ISO yyyy-MM-dd) ou de hoje quando omitido. */
  obterResumo(data?: string): Observable<CentralResumo> {
    let params = new HttpParams();
    if (data) {
      params = params.set('data', data);
    }
    return this.http.get<CentralResumo>(`${this.api}/resumo`, { params });
  }
}
