import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { IngredienteAtivo, ReceitaCasa, SalvarReceitaCasaRequest } from './receitas.model';

interface IngredienteApi {
  id: number;
  nome: string;
  categoria: string;
  coeficiente: number;
  custoKg: number;
  ativo: boolean;
}

@Injectable({ providedIn: 'root' })
export class ReceitasService {
  private readonly http = inject(HttpClient);
  private readonly base = environment.apiUrl;

  /** Ingredientes ativos do módulo Ingredientes (sem cadastro paralelo). */
  listarIngredientes(): Observable<IngredienteAtivo[]> {
    return this.http.get<IngredienteApi[]>(`${this.base}/ingredientes`).pipe(
      map((lista) =>
        lista
          .filter((i) => i.ativo)
          .map((i) => ({
            id: i.id,
            nome: i.nome,
            categoria: i.categoria,
            coeficiente: i.coeficiente,
            custoKg: i.custoKg,
          })),
      ),
    );
  }

  listar(): Observable<ReceitaCasa[]> {
    return this.http.get<ReceitaCasa[]>(`${this.base}/receitas-casa`);
  }

  criar(req: SalvarReceitaCasaRequest): Observable<ReceitaCasa> {
    return this.http.post<ReceitaCasa>(`${this.base}/receitas-casa`, req);
  }

  atualizar(id: number, req: SalvarReceitaCasaRequest): Observable<ReceitaCasa> {
    return this.http.put<ReceitaCasa>(`${this.base}/receitas-casa/${id}`, req);
  }

  alternarStatus(id: number, ativo: boolean): Observable<void> {
    return this.http.put<void>(`${this.base}/receitas-casa/${id}/status`, { ativo });
  }
}
