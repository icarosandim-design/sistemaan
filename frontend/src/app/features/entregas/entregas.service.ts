import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AlterarAgendaRequest, EntregaDetalhe, EntregaResumo, SituacaoEstoqueItem } from './entregas.model';

@Injectable({ providedIn: 'root' })
export class EntregasService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/entregas`;

  listar(): Observable<EntregaResumo[]> {
    return this.http.get<EntregaResumo[]>(this.base);
  }

  obter(id: number): Observable<EntregaDetalhe> {
    return this.http.get<EntregaDetalhe>(`${this.base}/${id}`);
  }

  situacaoEstoque(id: number): Observable<SituacaoEstoqueItem[]> {
    return this.http.get<SituacaoEstoqueItem[]>(`${this.base}/${id}/estoque`);
  }

  gerar(horizonteDias = 45): Observable<{ geradas: number; clientes: number }> {
    return this.http.post<{ geradas: number; clientes: number }>(`${this.base}/gerar`, { horizonteDias });
  }

  mudarStatus(id: number, status: string): Observable<EntregaDetalhe> {
    return this.http.put<EntregaDetalhe>(`${this.base}/${id}/status`, { status });
  }

  naoEntregue(id: number, motivo: string): Observable<EntregaDetalhe> {
    return this.http.put<EntregaDetalhe>(`${this.base}/${id}/nao-entregue`, { motivo });
  }

  reagendar(id: number, novaData: string, motivo: string): Observable<EntregaDetalhe> {
    return this.http.put<EntregaDetalhe>(`${this.base}/${id}/reagendar`, { novaData, motivo });
  }

  cancelar(id: number, motivo: string): Observable<EntregaDetalhe> {
    return this.http.put<EntregaDetalhe>(`${this.base}/${id}/cancelar`, { motivo });
  }

  alterarAgenda(id: number, req: AlterarAgendaRequest): Observable<{ geradas: number; clientes: number }> {
    return this.http.put<{ geradas: number; clientes: number }>(`${this.base}/${id}/alterar-agenda`, req);
  }
}
