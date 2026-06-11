import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthResult, LoginRequest, UsuarioAutenticado } from './auth.models';
import { TokenStorageService } from './token-storage.service';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly storage = inject(TokenStorageService);
  private readonly baseUrl = `${environment.apiUrl}/auth`;

  private readonly usuarioSignal = signal<UsuarioAutenticado | null>(this.storage.usuario);

  /** Usuário autenticado atual (ou null). */
  readonly usuario = this.usuarioSignal.asReadonly();

  /** Estado reativo de autenticação. */
  readonly autenticado = computed(() => this.usuarioSignal() !== null);

  login(credenciais: LoginRequest): Observable<AuthResult> {
    return this.http.post<AuthResult>(`${this.baseUrl}/login`, credenciais).pipe(
      tap((auth) => {
        this.storage.salvarSessao(auth);
        this.usuarioSignal.set(auth.usuario);
      }),
    );
  }

  logout(): void {
    const refreshToken = this.storage.refreshToken;
    if (refreshToken) {
      // Revoga o refresh token no servidor; ignora erros (logout é best-effort).
      this.http.post(`${this.baseUrl}/logout`, { refreshToken }).subscribe({ error: () => undefined });
    }
    this.storage.limpar();
    this.usuarioSignal.set(null);
  }

  /** Sessão válida = token presente e não expirado. */
  estaAutenticado(): boolean {
    const token = this.storage.accessToken;
    if (!token) {
      return false;
    }

    const expira = this.storage.expiraEm;
    if (expira && expira.getTime() <= Date.now()) {
      return false;
    }

    return true;
  }
}
