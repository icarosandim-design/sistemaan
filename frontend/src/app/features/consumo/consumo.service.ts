import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { FaixaConsumo, SalvarFaixaConsumoRequest } from './consumo.model';

@Injectable({ providedIn: 'root' })
export class ConsumoService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/faixas-consumo`;

  listar(): Observable<FaixaConsumo[]> {
    return this.http.get<FaixaConsumo[]>(this.base);
  }

  /** Faixa ativa que se aplica ao peso (null quando nenhuma — HTTP 204). */
  consultar(peso: number): Observable<FaixaConsumo | null> {
    return this.http.get<FaixaConsumo | null>(`${this.base}/aplica`, { params: { peso } });
  }

  criar(req: SalvarFaixaConsumoRequest): Observable<FaixaConsumo> {
    return this.http.post<FaixaConsumo>(this.base, req);
  }

  atualizar(id: number, req: SalvarFaixaConsumoRequest): Observable<FaixaConsumo> {
    return this.http.put<FaixaConsumo>(`${this.base}/${id}`, req);
  }
}
