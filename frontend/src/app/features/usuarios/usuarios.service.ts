import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AtualizarUsuarioRequest, CriarUsuarioRequest, PerfilOpcao, Usuario } from './usuarios.model';

@Injectable({ providedIn: 'root' })
export class UsuariosService {
  private readonly http = inject(HttpClient);
  private readonly api = `${environment.apiUrl}/usuarios`;

  listar(busca?: string, perfil?: string, ativo?: boolean | null): Observable<Usuario[]> {
    let params = new HttpParams();
    if (busca) params = params.set('busca', busca);
    if (perfil) params = params.set('perfil', perfil);
    if (ativo !== null && ativo !== undefined) params = params.set('ativo', String(ativo));
    return this.http.get<Usuario[]>(this.api, { params });
  }

  perfis(): Observable<PerfilOpcao[]> {
    return this.http.get<PerfilOpcao[]>(`${this.api}/perfis`);
  }

  criar(req: CriarUsuarioRequest): Observable<Usuario> {
    return this.http.post<Usuario>(this.api, req);
  }

  atualizar(id: number, req: AtualizarUsuarioRequest): Observable<Usuario> {
    return this.http.put<Usuario>(`${this.api}/${id}`, req);
  }

  alternarStatus(id: number, ativo: boolean): Observable<Usuario> {
    return this.http.put<Usuario>(`${this.api}/${id}/status`, { ativo });
  }

  redefinirSenha(id: number, novaSenha: string): Observable<void> {
    return this.http.put<void>(`${this.api}/${id}/senha`, { novaSenha });
  }
}
