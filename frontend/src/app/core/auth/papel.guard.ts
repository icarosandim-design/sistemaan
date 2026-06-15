import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';
import { rotaInicial } from './perfis';

/**
 * Restringe a rota aos papéis declarados em `data.papeis`.
 * Sem `data.papeis`, libera qualquer usuário autenticado.
 * Sem permissão, redireciona para a rota inicial do perfil.
 */
export const papelGuard: CanActivateFn = (route) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  const requeridos = route.data?.['papeis'] as string[] | undefined;
  const meus = auth.usuario()?.papeis ?? [];

  if (!requeridos || requeridos.length === 0 || requeridos.some((p) => meus.includes(p))) {
    return true;
  }

  return router.createUrlTree([rotaInicial(meus)]);
};
