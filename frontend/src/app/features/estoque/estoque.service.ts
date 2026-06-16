import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AtualizarItemEstoqueRequest,
  CompraResultado,
  CriarItemInsumoRequest,
  CriarItemProdutoAcabadoRequest,
  EntradaCompra,
  FiltroEntradas,
  FiltroMovimentacoes,
  Fornecedor,
  ItemEstoque,
  LoteEstoque,
  MovimentacaoEstoque,
  MovimentacaoPagina,
  OpcaoSimples,
  PersonalizadaPronta,
  RegistrarAjusteRequest,
  RegistrarCompraRequest,
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

  /** Pacotes de Receita Personalizada prontos e ainda não entregues. */
  listarPersonalizadasProntas(): Observable<PersonalizadaPronta[]> {
    return this.http.get<PersonalizadaPronta[]>(`${this.api}/estoque/itens/personalizadas-prontas`);
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

  registrarCompra(req: RegistrarCompraRequest): Observable<CompraResultado> {
    return this.http.post<CompraResultado>(`${this.api}/estoque/compras`, req);
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

  listarMovimentacoesGeral(filtro: FiltroMovimentacoes): Observable<MovimentacaoPagina> {
    return this.http.get<MovimentacaoPagina>(`${this.api}/estoque/movimentacoes`, { params: montarParams(filtro) });
  }

  listarEntradas(filtro: FiltroEntradas): Observable<EntradaCompra[]> {
    return this.http.get<EntradaCompra[]>(`${this.api}/estoque/entradas`, { params: montarParams(filtro) });
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

  listarTamanhosComPeso(): Observable<{ id: number; pesoGramas: number }[]> {
    return this.http
      .get<{ id: number; pesoGramas: number }[]>(`${this.api}/tamanhos-pacote`)
      .pipe(map((xs) => xs.map((x) => ({ id: x.id, pesoGramas: x.pesoGramas }))));
  }
}

/** Monta HttpParams a partir de um objeto de filtros, ignorando null/undefined/''. */
function montarParams(filtro: object): HttpParams {
  let params = new HttpParams();
  for (const [chave, valor] of Object.entries(filtro)) {
    if (valor !== null && valor !== undefined && valor !== '') {
      params = params.set(chave, String(valor));
    }
  }
  return params;
}
