import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { PlanoAlimentar, SalvarPlanoRequest } from './plano.model';

@Injectable({ providedIn: 'root' })
export class PlanoService {
  private readonly http = inject(HttpClient);
  private readonly api = environment.apiUrl;

  /** Plano vigente do pet (ou null). */
  obterPorPet(petId: number): Observable<PlanoAlimentar | null> {
    return this.http.get<PlanoAlimentar | null>(`${this.api}/pets/${petId}/plano`);
  }

  salvar(petId: number, req: SalvarPlanoRequest): Observable<PlanoAlimentar> {
    return this.http.put<PlanoAlimentar>(`${this.api}/pets/${petId}/plano`, req);
  }
}
