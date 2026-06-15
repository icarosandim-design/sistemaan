/** Perfis fixos do sistema (espelham PapeisDoSistema no backend). */
export const PERFIL = {
  ADMIN: 'Administrador',
  OPERADOR: 'Operador',
  COZINHA: 'Cozinha',
} as const;

/** Conjuntos de acesso reutilizáveis. */
export const ADMIN_OPERADOR = [PERFIL.ADMIN, PERFIL.OPERADOR];
export const TODOS_PERFIS = [PERFIL.ADMIN, PERFIL.OPERADOR, PERFIL.COZINHA];
export const SO_ADMIN = [PERFIL.ADMIN];

/** Rota inicial conforme o perfil (Cozinha cai direto na Produção do dia). */
export function rotaInicial(papeis: readonly string[]): string {
  if (papeis.includes(PERFIL.ADMIN) || papeis.includes(PERFIL.OPERADOR)) {
    return '/central';
  }
  if (papeis.includes(PERFIL.COZINHA)) {
    return '/producao/dia';
  }
  return '/central';
}
