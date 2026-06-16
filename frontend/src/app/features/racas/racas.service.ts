import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Raca, SalvarRacaRequest } from './racas.model';

@Injectable({ providedIn: 'root' })
export class RacasService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/racas`;

  listar(incluirInativas = false): Observable<Raca[]> {
    return this.http.get<Raca[]>(this.base, { params: { incluirInativas } });
  }

  criar(req: SalvarRacaRequest): Observable<Raca> {
    return this.http.post<Raca>(this.base, req);
  }

  atualizar(id: number, req: SalvarRacaRequest): Observable<Raca> {
    return this.http.put<Raca>(`${this.base}/${id}`, req);
  }

  alternarStatus(id: number, ativo: boolean): Observable<void> {
    return this.http.put<void>(`${this.base}/${id}/status`, { ativo });
  }
}
