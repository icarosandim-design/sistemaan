import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CategoriaIngrediente, SalvarCategoriaIngredienteRequest } from './categorias.model';

@Injectable({ providedIn: 'root' })
export class CategoriasIngredientesService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/categorias-ingredientes`;

  listar(incluirInativas = false): Observable<CategoriaIngrediente[]> {
    return this.http.get<CategoriaIngrediente[]>(this.base, { params: { incluirInativas } });
  }

  criar(req: SalvarCategoriaIngredienteRequest): Observable<CategoriaIngrediente> {
    return this.http.post<CategoriaIngrediente>(this.base, req);
  }

  atualizar(id: number, req: SalvarCategoriaIngredienteRequest): Observable<CategoriaIngrediente> {
    return this.http.put<CategoriaIngrediente>(`${this.base}/${id}`, req);
  }

  alternarStatus(id: number, ativo: boolean): Observable<void> {
    return this.http.put<void>(`${this.base}/${id}/status`, { ativo });
  }
}
