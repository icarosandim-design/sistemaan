import { Injectable } from '@angular/core';
import { AuthResult, UsuarioAutenticado } from './auth.models';

const ACCESS_KEY = 'access_token';
const REFRESH_KEY = 'refresh_token';
const EXPIRES_KEY = 'access_token_expira';
const USER_KEY = 'usuario';
const REMEMBER_EMAIL_KEY = 'remember_email';

/**
 * Persistência da sessão no localStorage (sobrevive a reloads/abas).
 */
@Injectable({ providedIn: 'root' })
export class TokenStorageService {
  salvarSessao(auth: AuthResult): void {
    localStorage.setItem(ACCESS_KEY, auth.accessToken);
    localStorage.setItem(REFRESH_KEY, auth.refreshToken);
    localStorage.setItem(EXPIRES_KEY, auth.accessTokenExpiraEm);
    localStorage.setItem(USER_KEY, JSON.stringify(auth.usuario));
  }

  get accessToken(): string | null {
    return localStorage.getItem(ACCESS_KEY);
  }

  get refreshToken(): string | null {
    return localStorage.getItem(REFRESH_KEY);
  }

  get expiraEm(): Date | null {
    const raw = localStorage.getItem(EXPIRES_KEY);
    return raw ? new Date(raw) : null;
  }

  get usuario(): UsuarioAutenticado | null {
    const raw = localStorage.getItem(USER_KEY);
    return raw ? (JSON.parse(raw) as UsuarioAutenticado) : null;
  }

  limpar(): void {
    // Não remove o e-mail lembrado: ele deve sobreviver ao logout.
    [ACCESS_KEY, REFRESH_KEY, EXPIRES_KEY, USER_KEY].forEach((k) => localStorage.removeItem(k));
  }

  // ---- "Lembrar meu e-mail" (independente da sessão) ----

  get emailLembrado(): string | null {
    return localStorage.getItem(REMEMBER_EMAIL_KEY);
  }

  lembrarEmail(email: string): void {
    localStorage.setItem(REMEMBER_EMAIL_KEY, email);
  }

  esquecerEmail(): void {
    localStorage.removeItem(REMEMBER_EMAIL_KEY);
  }
}
