import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Pet, SalvarPetRequest } from './pet.model';

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
