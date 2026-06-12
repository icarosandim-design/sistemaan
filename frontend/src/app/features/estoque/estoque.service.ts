import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AtualizarItemEstoqueRequest,
  CriarItemInsumoRequest,
  CriarItemProdutoAcabadoRequest,
  Fornecedor,
  ItemEstoque,
  LoteEstoque,
  MovimentacaoEstoque,
  OpcaoSimples,
  RegistrarAjusteRequest,
  RegistrarEntradaRequest,
  RegistrarSaidaRequest,
  SalvarFornecedorRequest,
} from './estoque.model';

@Injectable({ providedIn: 'root' })
export class EstoqueService {
  private readonly http = inject(HttpClient);
  private readonly api = environment.apiUrl;

  // ===== Fornecedores =====
  listarFornecedores(apenasAtivos = false): Observable<Fornecedor[]> {
    return this.http.get<Fornecedor[]>(`${this.api}/fornecedores`, { params: { apenasAtivos } });
  }

  criarFornecedor(req: SalvarFornecedorRequest): Observable<Fornecedor> {
    return this.http.post<Fornecedor>(`${this.api}/fornecedores`, req);
  }

  atualizarFornecedor(id: number, req: SalvarFornecedorRequest): Observable<Fornecedor> {
    return this.http.put<Fornecedor>(`${this.api}/fornecedores/${id}`, req);
  }

  alternarStatusFornecedor(id: number, ativo: boolean): Observable<void> {
    return this.http.put<void>(`${this.api}/fornecedores/${id}/status`, { ativo });
  }

  // ===== Itens de estoque =====
  listarItens(abaixoMinimo = false): Observable<ItemEstoque[]> {
    return this.http.get<ItemEstoque[]>(`${this.api}/estoque/itens`, { params: { abaixoMinimo } });
  }

  obterItem(id: number): Observable<ItemEstoque> {
    return this.http.get<ItemEstoque>(`${this.api}/estoque/itens/${id}`);
  }

  criarInsumo(req: CriarItemInsumoRequest): Observable<ItemEstoque> {
    return this.http.post<ItemEstoque>(`${this.api}/estoque/itens/insumo`, req);
  }

  criarProdutoAcabado(req: CriarItemProdutoAcabadoRequest): Observable<ItemEstoque> {
    return this.http.post<ItemEstoque>(`${this.api}/estoque/itens/produto-acabado`, req);
  }

  atualizarItem(id: number, req: AtualizarItemEstoqueRequest): Observable<ItemEstoque> {
    return this.http.put<ItemEstoque>(`${this.api}/estoque/itens/${id}`, req);
  }

  alternarStatusItem(id: number, ativo: boolean): Observable<void> {
    return this.http.put<void>(`${this.api}/estoque/itens/${id}/status`, { ativo });
  }

  // ===== Operações =====
  registrarEntrada(req: RegistrarEntradaRequest): Observable<ItemEstoque> {
    return this.http.post<ItemEstoque>(`${this.api}/estoque/entradas`, req);
  }

  registrarSaida(req: RegistrarSaidaRequest): Observable<ItemEstoque> {
    return this.http.post<ItemEstoque>(`${this.api}/estoque/saidas`, req);
  }

  registrarAjuste(req: RegistrarAjusteRequest): Observable<ItemEstoque> {
    return this.http.post<ItemEstoque>(`${this.api}/estoque/ajustes`, req);
  }

  listarLotes(itemId: number): Observable<LoteEstoque[]> {
    return this.http.get<LoteEstoque[]>(`${this.api}/estoque/itens/${itemId}/lotes`);
  }

  listarMovimentacoes(itemId: number): Observable<MovimentacaoEstoque[]> {
    return this.http.get<MovimentacaoEstoque[]>(`${this.api}/estoque/itens/${itemId}/movimentacoes`);
  }

  // ===== Opções para selects =====
  listarIngredientes(): Observable<OpcaoSimples[]> {
    return this.http
      .get<{ id: number; nome: string; ativo: boolean }[]>(`${this.api}/ingredientes`)
      .pipe(map((xs) => xs.filter((x) => x.ativo).map((x) => ({ id: x.id, nome: x.nome }))));
  }

  listarReceitasCasa(): Observable<OpcaoSimples[]> {
    return this.http
      .get<{ id: number; codigo: string; nome: string; ativo: boolean }[]>(`${this.api}/receitas-casa`)
      .pipe(map((xs) => xs.filter((x) => x.ativo).map((x) => ({ id: x.id, nome: `${x.codigo} — ${x.nome}` }))));
  }

  listarTamanhos(): Observable<OpcaoSimples[]> {
    return this.http
      .get<{ id: number; nome: string; pesoGramas: number; ativo: boolean }[]>(`${this.api}/tamanhos-pacote`)
      .pipe(map((xs) => xs.filter((x) => x.ativo).map((x) => ({ id: x.id, nome: x.nome }))));
  }
}
