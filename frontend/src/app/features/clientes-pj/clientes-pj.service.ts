import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { SituacaoEstoqueItem } from '../entregas/entregas.model';
import {
  ClientePj,
  ClientePjResumo,
  Pedido,
  PedidoResumo,
  SalvarClientePjRequest,
  SalvarPedidoRequest,
  TipoPjOpcao,
} from './clientes-pj.model';

@Injectable({ providedIn: 'root' })
export class ClientesPjService {
  private readonly http = inject(HttpClient);
  private readonly api = environment.apiUrl;

  // ===== Clientes PJ =====
  listar(busca?: string, ativo?: boolean | null): Observable<ClientePjResumo[]> {
    let params = new HttpParams();
    if (busca) params = params.set('busca', busca);
    if (ativo !== null && ativo !== undefined) params = params.set('ativo', String(ativo));
    return this.http.get<ClientePjResumo[]>(`${this.api}/clientes-pj`, { params });
  }

  tipos(): Observable<TipoPjOpcao[]> {
    return this.http.get<TipoPjOpcao[]>(`${this.api}/clientes-pj/tipos`);
  }

  obter(id: number): Observable<ClientePj> {
    return this.http.get<ClientePj>(`${this.api}/clientes-pj/${id}`);
  }

  criar(req: SalvarClientePjRequest): Observable<ClientePj> {
    return this.http.post<ClientePj>(`${this.api}/clientes-pj`, req);
  }

  atualizar(id: number, req: SalvarClientePjRequest): Observable<ClientePj> {
    return this.http.put<ClientePj>(`${this.api}/clientes-pj/${id}`, req);
  }

  alternarStatus(id: number, ativo: boolean): Observable<ClientePj> {
    return this.http.put<ClientePj>(`${this.api}/clientes-pj/${id}/status`, { ativo });
  }

  // ===== Pedidos =====
  listarPedidos(clienteId: number): Observable<PedidoResumo[]> {
    return this.http.get<PedidoResumo[]>(`${this.api}/pedidos`, { params: new HttpParams().set('clienteId', clienteId) });
  }

  obterPedido(id: number): Observable<Pedido> {
    return this.http.get<Pedido>(`${this.api}/pedidos/${id}`);
  }

  criarPedido(req: SalvarPedidoRequest): Observable<Pedido> {
    return this.http.post<Pedido>(`${this.api}/pedidos`, req);
  }

  atualizarPedido(id: number, req: SalvarPedidoRequest): Observable<Pedido> {
    return this.http.put<Pedido>(`${this.api}/pedidos/${id}`, req);
  }

  confirmarPedido(id: number): Observable<Pedido> {
    return this.http.post<Pedido>(`${this.api}/pedidos/${id}/confirmar`, {});
  }

  cancelarPedido(id: number, motivo?: string): Observable<Pedido> {
    return this.http.post<Pedido>(`${this.api}/pedidos/${id}/cancelar`, { motivo: motivo ?? null });
  }

  situacaoPedido(id: number): Observable<SituacaoEstoqueItem[]> {
    return this.http.get<SituacaoEstoqueItem[]>(`${this.api}/pedidos/${id}/estoque`);
  }
}
