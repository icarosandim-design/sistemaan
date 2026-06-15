import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Pet, PetResumo, SalvarPetRequest } from './pet.model';

interface FaixaConsumo {
  gramasPorDia: number;
}

@Injectable({ providedIn: 'root' })
export class PetService {
  private readonly http = inject(HttpClient);
  private readonly api = environment.apiUrl;

  listarPorCliente(clienteId: number): Observable<Pet[]> {
    return this.http.get<Pet[]>(`${this.api}/clientes/${clienteId}/pets`);
  }

  /** Visão geral de todos os pets (com tutor, plano/receita e próxima entrega). */
  listarTodos(busca?: string, ativo?: boolean | null, tipo?: string): Observable<PetResumo[]> {
    let params = new HttpParams();
    if (busca) params = params.set('busca', busca);
    if (ativo !== null && ativo !== undefined) params = params.set('ativo', String(ativo));
    if (tipo) params = params.set('tipo', tipo);
    return this.http.get<PetResumo[]>(`${this.api}/pets`, { params });
  }

  obter(id: number): Observable<Pet> {
    return this.http.get<Pet>(`${this.api}/pets/${id}`);
  }

  criar(clienteId: number, req: SalvarPetRequest): Observable<Pet> {
    return this.http.post<Pet>(`${this.api}/clientes/${clienteId}/pets`, req);
  }

  atualizar(id: number, req: SalvarPetRequest): Observable<Pet> {
    return this.http.put<Pet>(`${this.api}/pets/${id}`, req);
  }

  inativar(id: number): Observable<void> {
    return this.http.put<void>(`${this.api}/pets/${id}/inativar`, {});
  }

  reativar(id: number): Observable<void> {
    return this.http.put<void>(`${this.api}/pets/${id}/reativar`, {});
  }

  /** Sugestão de gramas/dia para um peso, vinda da Tabela de Consumo (204 = sem faixa). */
  sugestaoPorPeso(peso: number): Observable<number | null> {
    return this.http
      .get<FaixaConsumo>(`${this.api}/faixas-consumo/aplica`, { params: { peso } })
      .pipe(map((f) => (f ? f.gramasPorDia : null)));
  }
}
