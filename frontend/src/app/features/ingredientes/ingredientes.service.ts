import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Categoria, Ingrediente, SalvarIngredienteRequest } from './ingredientes.model';

@Injectable({ providedIn: 'root' })
export class IngredientesService {
  private readonly http = inject(HttpClient);
  private readonly base = environment.apiUrl;

  listarCategorias(): Observable<Categoria[]> {
    return this.http.get<Categoria[]>(`${this.base}/categorias-ingredientes`);
  }

  listar(): Observable<Ingrediente[]> {
    return this.http.get<Ingrediente[]>(`${this.base}/ingredientes`);
  }

  criar(req: SalvarIngredienteRequest): Observable<Ingrediente> {
    return this.http.post<Ingrediente>(`${this.base}/ingredientes`, req);
  }

  atualizar(id: number, req: SalvarIngredienteRequest): Observable<Ingrediente> {
    return this.http.put<Ingrediente>(`${this.base}/ingredientes/${id}`, req);
  }

  excluir(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/ingredientes/${id}`);
  }
}
