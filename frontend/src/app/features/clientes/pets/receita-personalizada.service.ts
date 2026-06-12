import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { ReceitaPersonalizada, SalvarReceitaPersonalizadaRequest } from './plano.model';

@Injectable({ providedIn: 'root' })
export class ReceitaPersonalizadaService {
  private readonly http = inject(HttpClient);
  private readonly api = environment.apiUrl;

  listarPorPet(petId: number): Observable<ReceitaPersonalizada[]> {
    return this.http.get<ReceitaPersonalizada[]>(`${this.api}/pets/${petId}/receitas-personalizadas`);
  }

  criar(petId: number, req: SalvarReceitaPersonalizadaRequest): Observable<ReceitaPersonalizada> {
    return this.http.post<ReceitaPersonalizada>(`${this.api}/pets/${petId}/receitas-personalizadas`, req);
  }

  atualizar(id: number, req: SalvarReceitaPersonalizadaRequest): Observable<ReceitaPersonalizada> {
    return this.http.put<ReceitaPersonalizada>(`${this.api}/receitas-personalizadas/${id}`, req);
  }
}
