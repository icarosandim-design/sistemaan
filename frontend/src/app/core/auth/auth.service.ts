import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, finalize, shareReplay, tap, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthResult, LoginRequest, UsuarioAutenticado } from './auth.models';
import { TokenStorageService } from './token-storage.service';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly storage = inject(TokenStorageService);
  private readonly baseUrl = `${environment.apiUrl}/auth`;

  private readonly usuarioSignal = signal<UsuarioAutenticado | null>(this.storage.usuario);

  /** Refresh em andamento compartilhado (evita múltiplas renovações simultâneas). */
  private refreshEmAndamento: Observable<AuthResult> | null = null;

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

  /**
   * Renova o access token usando o refresh token (rotação no servidor).
   * Requisições simultâneas compartilham a mesma renovação.
   */
  refresh(): Observable<AuthResult> {
    if (this.refreshEmAndamento) {
      return this.refreshEmAndamento;
    }

    const refreshToken = this.storage.refreshToken;
    if (!refreshToken) {
      return throwError(() => new Error('Sessão expirada.'));
    }

    this.refreshEmAndamento = this.http.post<AuthResult>(`${this.baseUrl}/refresh`, { refreshToken }).pipe(
      tap((auth) => {
        this.storage.salvarSessao(auth);
        this.usuarioSignal.set(auth.usuario);
      }),
      finalize(() => {
        this.refreshEmAndamento = null;
      }),
      shareReplay(1),
    );

    return this.refreshEmAndamento;
  }

  /** Encerra a sessão localmente (sem chamada ao servidor). */
  sessaoExpirada(): void {
    this.storage.limpar();
    this.usuarioSignal.set(null);
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
