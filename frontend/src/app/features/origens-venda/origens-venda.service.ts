import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { OrigemVenda, SalvarOrigemVendaRequest } from './origens-venda.model';

@Injectable({ providedIn: 'root' })
export class OrigensVendaService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/origens-venda`;

  listar(incluirInativas = false): Observable<OrigemVenda[]> {
    return this.http.get<OrigemVenda[]>(this.base, { params: { incluirInativas } });
  }

  criar(req: SalvarOrigemVendaRequest): Observable<OrigemVenda> {
    return this.http.post<OrigemVenda>(this.base, req);
  }

  atualizar(id: number, req: SalvarOrigemVendaRequest): Observable<OrigemVenda> {
    return this.http.put<OrigemVenda>(`${this.base}/${id}`, req);
  }

  alternarStatus(id: number, ativo: boolean): Observable<void> {
    return this.http.put<void>(`${this.base}/${id}/status`, { ativo });
  }
}
