import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../auth/auth.service';
import { TokenStorageService } from '../auth/token-storage.service';

/**
 * Anexa o token JWT e, em caso de 401 (token expirado), tenta renovar via
 * refresh token e refaz a requisição. Se a renovação falhar, encerra a sessão.
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const storage = inject(TokenStorageService);
  const auth = inject(AuthService);
  const router = inject(Router);

  const ehAuth = req.url.includes('/auth/login') || req.url.includes('/auth/refresh');
  const token = storage.accessToken;

  const requisicao =
    token && !ehAuth ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : req;

  return next(requisicao).pipe(
    catchError((erro: HttpErrorResponse) => {
      if (erro.status !== 401 || ehAuth || !storage.refreshToken) {
        return throwError(() => erro);
      }

      // Token expirado: renova (compartilhado) e refaz a requisição original.
      return auth.refresh().pipe(
        switchMap((res) =>
          next(req.clone({ setHeaders: { Authorization: `Bearer ${res.accessToken}` } })),
        ),
        catchError((erroRefresh) => {
          auth.sessaoExpirada();
          router.navigateByUrl('/login');
          return throwError(() => erroRefresh);
        }),
      );
    }),
  );
};
