import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Doenca, SalvarDoencaRequest } from './doencas.model';

@Injectable({ providedIn: 'root' })
export class DoencasService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/doencas`;

  listar(incluirInativas = false): Observable<Doenca[]> {
    return this.http.get<Doenca[]>(this.base, { params: { incluirInativas } });
  }

  criar(req: SalvarDoencaRequest): Observable<Doenca> {
    return this.http.post<Doenca>(this.base, req);
  }

  atualizar(id: number, req: SalvarDoencaRequest): Observable<Doenca> {
    return this.http.put<Doenca>(`${this.base}/${id}`, req);
  }

  alternarStatus(id: number, ativo: boolean): Observable<void> {
    return this.http.put<void>(`${this.base}/${id}/status`, { ativo });
  }
}
