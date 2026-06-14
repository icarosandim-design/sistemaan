import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AdicionarFichasRequest,
  ConcluirFichaRequest,
  Demanda,
  FinalizacaoResultado,
  FinalizarProducaoRequest,
  MarcarNaoFeitaRequest,
  MudarStatusFichaRequest,
  OrdemProducao,
  OrdemProducaoResumo,
  RegistrarConsumoRequest,
  StatusFicha,
} from './producao.model';

@Injectable({ providedIn: 'root' })
export class ProducaoService {
  private readonly http = inject(HttpClient);
  private readonly api = `${environment.apiUrl}/producao`;

  /** Demanda do período (personalizadas não prontas + Casa com falta). Datas em yyyy-MM-dd. */
  obterDemanda(inicio: string, fim: string): Observable<Demanda> {
    const params = new HttpParams().set('inicio', inicio).set('fim', fim);
    return this.http.get<Demanda>(`${this.api}/demanda`, { params });
  }

  listarOrdens(): Observable<OrdemProducaoResumo[]> {
    return this.http.get<OrdemProducaoResumo[]>(this.api);
  }

  /** Ordem de um dia (yyyy-MM-dd) ou null (404). */
  obterPorData(data: string): Observable<OrdemProducao> {
    return this.http.get<OrdemProducao>(`${this.api}/dia/${data}`);
  }

  obter(id: number): Observable<OrdemProducao> {
    return this.http.get<OrdemProducao>(`${this.api}/${id}`);
  }

  criarOuObter(data: string): Observable<OrdemProducao> {
    return this.http.post<OrdemProducao>(this.api, { data });
  }

  adicionarFichas(ordemId: number, req: AdicionarFichasRequest): Observable<OrdemProducao> {
    return this.http.post<OrdemProducao>(`${this.api}/${ordemId}/fichas`, req);
  }

  removerFicha(fichaId: number): Observable<OrdemProducao> {
    return this.http.delete<OrdemProducao>(`${this.api}/fichas/${fichaId}`);
  }

  mudarStatusFicha(fichaId: number, status: StatusFicha): Observable<OrdemProducao> {
    const req: MudarStatusFichaRequest = { status };
    return this.http.put<OrdemProducao>(`${this.api}/fichas/${fichaId}/status`, req);
  }

  concluirFicha(fichaId: number, req: ConcluirFichaRequest): Observable<OrdemProducao> {
    return this.http.put<OrdemProducao>(`${this.api}/fichas/${fichaId}/concluir`, req);
  }

  marcarNaoFeita(fichaId: number, motivo: string): Observable<OrdemProducao> {
    const req: MarcarNaoFeitaRequest = { motivo };
    return this.http.put<OrdemProducao>(`${this.api}/fichas/${fichaId}/nao-feita`, req);
  }

  registrarConsumo(ordemId: number, req: RegistrarConsumoRequest): Observable<OrdemProducao> {
    return this.http.put<OrdemProducao>(`${this.api}/${ordemId}/consumo`, req);
  }

  finalizar(ordemId: number, req: FinalizarProducaoRequest): Observable<FinalizacaoResultado> {
    return this.http.post<FinalizacaoResultado>(`${this.api}/${ordemId}/finalizar`, req);
  }
}
