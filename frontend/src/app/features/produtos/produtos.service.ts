import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Produto, SalvarProdutoRequest } from './produtos.model';

export interface GerarProdutoTamanho {
  tamanhoPacoteId: number;
  precoVendaAvulsaPF: number;
  precoVendaPJ: number;
}

@Injectable({ providedIn: 'root' })
export class ProdutosService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/produtos`;

  listar(incluirInativos = false, tipo?: string, receitaCasaId?: number, tamanhoPacoteId?: number): Observable<Produto[]> {
    let p = new HttpParams().set('incluirInativos', incluirInativos);
    if (tipo) p = p.set('tipo', tipo);
    if (receitaCasaId) p = p.set('receitaCasaId', receitaCasaId);
    if (tamanhoPacoteId) p = p.set('tamanhoPacoteId', tamanhoPacoteId);
    return this.http.get<Produto[]>(this.base, { params: p });
  }

  criar(req: SalvarProdutoRequest): Observable<Produto> {
    return this.http.post<Produto>(this.base, req);
  }

  atualizar(id: number, req: SalvarProdutoRequest): Observable<Produto> {
    return this.http.put<Produto>(`${this.base}/${id}`, req);
  }

  alternarStatus(id: number, ativo: boolean): Observable<void> {
    return this.http.put<void>(`${this.base}/${id}/status`, { ativo });
  }

  gerarParaReceita(receitaCasaId: number, tamanhos: GerarProdutoTamanho[]): Observable<Produto[]> {
    return this.http.post<Produto[]>(`${this.base}/gerar-receita/${receitaCasaId}`, { tamanhos });
  }
}
