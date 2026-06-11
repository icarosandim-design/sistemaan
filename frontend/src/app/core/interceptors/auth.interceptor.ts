import { HttpInterceptorFn } from '@angular/common/http';

/**
 * Anexa o token JWT (quando presente) ao cabeçalho Authorization.
 * A emissão/armazenamento do token será implementada no módulo de autenticação.
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = localStorage.getItem('access_token');

  if (token) {
    req = req.clone({
      setHeaders: { Authorization: `Bearer ${token}` },
    });
  }

  return next(req);
};
