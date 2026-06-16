import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { SalvarVendaAvulsaRequest, VendaAvulsaResultado } from './venda-avulsa.model';

/** Venda avulsa PF: registra uma venda única e gera a entrega correspondente. */
@Injectable({ providedIn: 'root' })
export class VendaAvulsaService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/vendas-avulsas`;

  criar(req: SalvarVendaAvulsaRequest): Observable<VendaAvulsaResultado> {
    return this.http.post<VendaAvulsaResultado>(this.base, req);
  }
}
