import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  Dashboard,
  RelatorioCancelamentos,
  RelatorioCustosInsumos,
  RelatorioEstoque,
  RelatorioFiltro,
  RelatorioPerdas,
  RelatorioProducao,
  RelatorioVendas,
} from './relatorios.model';

@Injectable({ providedIn: 'root' })
export class RelatoriosService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/relatorios`;

  private params(f: RelatorioFiltro): HttpParams {
    let p = new HttpParams();
    for (const [k, v] of Object.entries(f)) {
      if (v !== undefined && v !== null && v !== '') {
        p = p.set(k, String(v));
      }
    }
    return p;
  }

  dashboard(inicio?: string, fim?: string, ingredienteId?: number): Observable<Dashboard> {
    return this.http.get<Dashboard>(`${this.base}/dashboard`, { params: this.params({ inicio, fim, ingredienteId }) });
  }

  vendas(f: RelatorioFiltro): Observable<RelatorioVendas> {
    return this.http.get<RelatorioVendas>(`${this.base}/vendas`, { params: this.params(f) });
  }

  cancelamentos(f: RelatorioFiltro): Observable<RelatorioCancelamentos> {
    return this.http.get<RelatorioCancelamentos>(`${this.base}/cancelamentos`, { params: this.params(f) });
  }

  producao(f: RelatorioFiltro): Observable<RelatorioProducao> {
    return this.http.get<RelatorioProducao>(`${this.base}/producao-planejado-real`, { params: this.params(f) });
  }

  perdas(f: RelatorioFiltro): Observable<RelatorioPerdas> {
    return this.http.get<RelatorioPerdas>(`${this.base}/perdas-ingredientes`, { params: this.params(f) });
  }

  custosInsumos(f: RelatorioFiltro): Observable<RelatorioCustosInsumos> {
    return this.http.get<RelatorioCustosInsumos>(`${this.base}/evolucao-custos-insumos`, { params: this.params(f) });
  }

  estoque(f: RelatorioFiltro): Observable<RelatorioEstoque> {
    return this.http.get<RelatorioEstoque>(`${this.base}/estoque-produto-acabado`, { params: this.params(f) });
  }
}
