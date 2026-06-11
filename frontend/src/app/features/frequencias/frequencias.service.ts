import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { FrequenciaEntrega, SalvarFrequenciaRequest } from './frequencias.model';

@Injectable({ providedIn: 'root' })
export class FrequenciasService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/frequencias-entrega`;

  listar(): Observable<FrequenciaEntrega[]> {
    return this.http.get<FrequenciaEntrega[]>(this.base);
  }

  criar(req: SalvarFrequenciaRequest): Observable<FrequenciaEntrega> {
    return this.http.post<FrequenciaEntrega>(this.base, req);
  }

  atualizar(id: number, req: SalvarFrequenciaRequest): Observable<FrequenciaEntrega> {
    return this.http.put<FrequenciaEntrega>(`${this.base}/${id}`, req);
  }

  alternarStatus(id: number, ativo: boolean): Observable<void> {
    return this.http.put<void>(`${this.base}/${id}/status`, { ativo });
  }
}
