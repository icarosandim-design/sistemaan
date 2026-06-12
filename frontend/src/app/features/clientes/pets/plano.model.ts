// ============================================================================
// Plano Alimentar do Pet — MOCK (somente frontend, sem backend/persistência).
// Frequências, Receitas da Casa e Ingredientes aqui são dados de exemplo que,
// futuramente, virão dos módulos reais já existentes.
// ============================================================================

export type TipoAlimentacao = 'Casa' | 'Personalizada';
export type TipoConversao = 'perda' | 'ganho' | 'sem_conversao';

export interface FrequenciaMock {
  id: number;
  nome: string;
  diasCiclo: number;
}

export interface ReceitaCasaMock {
  id: number;
  codigo: string;
  nome: string;
}

export interface IngredienteMock {
  id: number;
  nome: string;
  categoria: string;
  tipoConversao: TipoConversao;
  coeficiente: number; // rendimento cozido ÷ cru
  custoKg: number; // custo por kg cru
}

/** Distribuição de uma Receita da Casa dentro do ciclo. */
export interface ItemCasa {
  receitaId: number | null;
  gramasCiclo: number;
  pacotes250: number;
  pacotes500: number;
}

/** Ingrediente (cozido) de uma Receita Personalizada. */
export interface ItemPersonalizado {
  ingredienteId: number | null;
  gramasCozidas: number;
}

/** Receita Personalizada — pertence a um único pet. */
export interface ReceitaPersonalizada {
  uid: number;
  codigo: string;
  observacoesPreparo: string;
  itens: ItemPersonalizado[];
  pacotes250: number;
  pacotes500: number;
}

// ---------------------------------------------------------------------------
// Dados mockados
// ---------------------------------------------------------------------------

export const MOCK_FREQUENCIAS: FrequenciaMock[] = [
  { id: 1, nome: 'Semanal (7 dias)', diasCiclo: 7 },
  { id: 2, nome: 'Quinzenal (14 dias)', diasCiclo: 14 },
  { id: 3, nome: 'Mensal (28 dias)', diasCiclo: 28 },
];

export const MOCK_RECEITAS_CASA: ReceitaCasaMock[] = [
  { id: 1, codigo: 'REC-001', nome: 'Frango com legumes' },
  { id: 2, codigo: 'REC-002', nome: 'Carne bovina com batata-doce' },
  { id: 3, codigo: 'REC-003', nome: 'Peixe com abóbora' },
];

export const MOCK_INGREDIENTES: IngredienteMock[] = [
  { id: 1, nome: 'Abóbora', categoria: 'Vegetal', tipoConversao: 'perda', coeficiente: 0.8, custoKg: 4.8 },
  { id: 2, nome: 'Arroz integral', categoria: 'Carboidrato', tipoConversao: 'ganho', coeficiente: 3.0, custoKg: 7.2 },
  { id: 3, nome: 'Batata-doce', categoria: 'Carboidrato', tipoConversao: 'perda', coeficiente: 0.55, custoKg: 6.5 },
  { id: 4, nome: 'Carne bovina (patinho)', categoria: 'Proteína', tipoConversao: 'perda', coeficiente: 0.65, custoKg: 32.5 },
  { id: 5, nome: 'Cenoura', categoria: 'Vegetal', tipoConversao: 'perda', coeficiente: 0.88, custoKg: 5.5 },
  { id: 6, nome: 'Frango (peito)', categoria: 'Proteína', tipoConversao: 'perda', coeficiente: 0.7, custoKg: 18.9 },
  { id: 7, nome: 'Fígado bovino', categoria: 'Proteína', tipoConversao: 'perda', coeficiente: 0.72, custoKg: 19.0 },
];

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

export function fmtMoeda(valor: number): string {
  return valor.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
}

export function fmtPeso(gramas: number): string {
  if (Math.abs(gramas) >= 1000) {
    return `${(gramas / 1000).toLocaleString('pt-BR', { maximumFractionDigits: 2 })} kg`;
  }
  return `${gramas.toLocaleString('pt-BR', { maximumFractionDigits: 0 })} g`;
}

/**
 * Sugestão de pacotes (250g/500g) para cobrir um alvo em gramas.
 * Prioriza praticidade: usa o mínimo de pacotes e prefere 500g.
 * (Sugestão, não obrigação — o operador pode ajustar depois.)
 */
export function sugerirPacotes(alvo: number): { p250: number; p500: number } {
  if (alvo <= 0) {
    return { p250: 0, p500: 0 };
  }
  let p500 = Math.floor(alvo / 500);
  const resto = alvo - p500 * 500;
  let p250 = 0;
  if (resto > 250) {
    p500 += 1; // 1 pacote de 500 é mais prático que 2 de 250
  } else if (resto > 0) {
    p250 = 1;
  }
  return { p250, p500 };
}

export function gramasPacotes(p250: number, p500: number): number {
  return (p250 || 0) * 250 + (p500 || 0) * 500;
}

/** Custo (R$) de X gramas cozidas de um ingrediente (custo/kg cru ÷ rendimento). */
export function custoIngrediente(gramasCozidas: number, ing: IngredienteMock | undefined): number {
  if (!ing || ing.coeficiente <= 0) {
    return 0;
  }
  const custoRealKg = ing.custoKg / ing.coeficiente; // por kg cozido
  return (gramasCozidas / 1000) * custoRealKg;
}
