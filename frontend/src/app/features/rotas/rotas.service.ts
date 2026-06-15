import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AtualizarRotaRequest, CriarRotaRequest, EntregaDisponivel, Rota, RotaResumo } from './rotas.model';

@Injectable({ providedIn: 'root' })
export class RotasService {
  private readonly http = inject(HttpClient);
  private readonly api = `${environment.apiUrl}/rotas`;

  listar(data: string): Observable<RotaResumo[]> {
    return this.http.get<RotaResumo[]>(this.api, { params: new HttpParams().set('data', data) });
  }

  disponiveis(data: string): Observable<EntregaDisponivel[]> {
    return this.http.get<EntregaDisponivel[]>(`${this.api}/disponiveis`, { params: new HttpParams().set('data', data) });
  }

  obter(id: number): Observable<Rota> {
    return this.http.get<Rota>(`${this.api}/${id}`);
  }

  criar(req: CriarRotaRequest): Observable<Rota> {
    return this.http.post<Rota>(this.api, req);
  }

  atualizar(id: number, req: AtualizarRotaRequest): Observable<Rota> {
    return this.http.put<Rota>(`${this.api}/${id}`, req);
  }

  adicionarEntrega(id: number, entregaId: number): Observable<Rota> {
    return this.http.post<Rota>(`${this.api}/${id}/entregas`, { entregaId });
  }

  removerEntrega(id: number, entregaId: number): Observable<Rota> {
    return this.http.delete<Rota>(`${this.api}/${id}/entregas/${entregaId}`);
  }

  reordenar(id: number, entregaIds: number[]): Observable<Rota> {
    return this.http.put<Rota>(`${this.api}/${id}/ordem`, { entregaIds });
  }

  mudarStatus(id: number, status: string): Observable<Rota> {
    return this.http.put<Rota>(`${this.api}/${id}/status`, { status });
  }
}
