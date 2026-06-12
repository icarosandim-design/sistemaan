import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Cliente, SalvarClienteRequest } from './clientes.model';

@Injectable({ providedIn: 'root' })
export class ClientesService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/clientes`;

  listar(): Observable<Cliente[]> {
    return this.http.get<Cliente[]>(this.base);
  }

  criar(req: SalvarClienteRequest): Observable<Cliente> {
    return this.http.post<Cliente>(this.base, req);
  }

  atualizar(id: number, req: SalvarClienteRequest): Observable<Cliente> {
    return this.http.put<Cliente>(`${this.base}/${id}`, req);
  }

  alternarStatus(id: number, ativo: boolean): Observable<void> {
    return this.http.put<void>(`${this.base}/${id}/status`, { ativo });
  }
}
