import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { MotivoCancelamento, SalvarMotivoCancelamentoRequest } from './motivos-cancelamento.model';

@Injectable({ providedIn: 'root' })
export class MotivosCancelamentoService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/motivos-cancelamento`;

  listar(incluirInativos = false): Observable<MotivoCancelamento[]> {
    return this.http.get<MotivoCancelamento[]>(this.base, { params: { incluirInativos } });
  }

  criar(req: SalvarMotivoCancelamentoRequest): Observable<MotivoCancelamento> {
    return this.http.post<MotivoCancelamento>(this.base, req);
  }

  atualizar(id: number, req: SalvarMotivoCancelamentoRequest): Observable<MotivoCancelamento> {
    return this.http.put<MotivoCancelamento>(`${this.base}/${id}`, req);
  }

  alternarStatus(id: number, ativo: boolean): Observable<void> {
    return this.http.put<void>(`${this.base}/${id}/status`, { ativo });
  }
}
