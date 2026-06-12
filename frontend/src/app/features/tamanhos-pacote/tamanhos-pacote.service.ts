import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { SalvarTamanhoPacoteRequest, TamanhoPacote } from './tamanhos-pacote.model';

@Injectable({ providedIn: 'root' })
export class TamanhosPacoteService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/tamanhos-pacote`;

  listar(): Observable<TamanhoPacote[]> {
    return this.http.get<TamanhoPacote[]>(this.base);
  }

  criar(req: SalvarTamanhoPacoteRequest): Observable<TamanhoPacote> {
    return this.http.post<TamanhoPacote>(this.base, req);
  }

  atualizar(id: number, req: SalvarTamanhoPacoteRequest): Observable<TamanhoPacote> {
    return this.http.put<TamanhoPacote>(`${this.base}/${id}`, req);
  }

  alternarStatus(id: number, ativo: boolean): Observable<void> {
    return this.http.put<void>(`${this.base}/${id}/status`, { ativo });
  }
}
