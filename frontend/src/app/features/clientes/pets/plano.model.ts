// ============================================================================
// Plano Alimentar do Pet — MOCK (somente frontend, sem backend/persistência).
// Os TAMANHOS DE PACOTE vêm do cadastro real (API). Frequências, Receitas da
// Casa e Ingredientes ainda são dados de exemplo (virão dos módulos reais).
// ============================================================================

import { TamanhoPacote } from '../../tamanhos-pacote/tamanhos-pacote.model';

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

/** Quantidades de pacotes por tamanho: { tamanhoId: quantidade }. */
export type Pacotes = Record<number, number>;

/** Distribuição de uma Receita da Casa dentro do ciclo. A gramagem por receita
 * é calculada (divisão igual do total); o operador ajusta os pacotes. */
export interface ItemCasa {
  receitaId: number | null;
  pacotes: Pacotes;
}

/** Ingrediente (cozido) de uma Receita Personalizada. */
export interface ItemPersonalizado {
  ingredienteId: number | null;
  gramasCozidas: number;
}

/** Receita Personalizada — pertence a um único pet. O "pacote" tem o tamanho
 * da própria receita (soma dos ingredientes cozidos); o operador informa quantos
 * pacotes daquela receita entram no ciclo. */
export interface ReceitaPersonalizada {
  uid: number;
  codigo: string;
  observacoesPreparo: string;
  itens: ItemPersonalizado[];
  quantidadePacotes: number;
}

// ---------------------------------------------------------------------------
// Dados mockados (frequências / receitas da casa / ingredientes)
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

/** Soma de gramas dos pacotes informados, usando os pesos do cadastro. */
export function gramasPacotes(pacotes: Pacotes, tamanhos: TamanhoPacote[]): number {
  return tamanhos.reduce((s, t) => s + (pacotes[t.id] || 0) * t.pesoGramas, 0);
}

/**
 * Sugestão de pacotes para cobrir um alvo (gramas), usando os TAMANHOS ATIVOS
 * cadastrados. Prioriza: (1) menor número de pacotes; (2) menor sobra.
 * Nunca entrega menos que o alvo. Retorna { tamanhoId: quantidade }.
 * É sugestão — o operador pode ajustar manualmente depois.
 */
export function sugerirPacotes(alvo: number, tamanhos: TamanhoPacote[]): Pacotes {
  const result: Pacotes = {};
  for (const t of tamanhos) {
    result[t.id] = 0;
  }

  const validos = tamanhos.filter((t) => t.pesoGramas > 0);
  if (alvo <= 0 || validos.length === 0) {
    return result;
  }

  const maxPeso = Math.max(...validos.map((t) => t.pesoGramas));
  const limite = alvo + maxPeso;

  // Proteção contra alvos muito grandes: cai num greedy (maior pacote primeiro).
  if (limite > 200000) {
    let restante = alvo;
    for (const t of [...validos].sort((a, b) => b.pesoGramas - a.pesoGramas)) {
      const q = Math.floor(restante / t.pesoGramas);
      result[t.id] = q;
      restante -= q * t.pesoGramas;
    }
    if (restante > 0) {
      const menor = [...validos].sort((a, b) => a.pesoGramas - b.pesoGramas)[0];
      result[menor.id] += 1;
    }
    return result;
  }

  // "Troco mínimo" que cobre o alvo: menos pacotes e, em empate, menor sobra.
  const INF = Number.POSITIVE_INFINITY;
  const minCount = new Array<number>(limite + 1).fill(INF);
  const escolha = new Array<number>(limite + 1).fill(-1); // id do último pacote usado
  minCount[0] = 0;
  for (let a = 1; a <= limite; a++) {
    for (const t of validos) {
      const anterior = a - t.pesoGramas;
      if (anterior >= 0 && minCount[anterior] + 1 < minCount[a]) {
        minCount[a] = minCount[anterior] + 1;
        escolha[a] = t.id;
      }
    }
  }

  // Entre os totais >= alvo, pega o de menor nº de pacotes (e, dentro disso, menor total).
  let melhorA = -1;
  let melhorCount = INF;
  for (let a = alvo; a <= limite; a++) {
    if (minCount[a] < melhorCount) {
      melhorCount = minCount[a];
      melhorA = a;
    }
  }
  if (melhorA < 0) {
    return result;
  }

  const pesoPorId = new Map(validos.map((t) => [t.id, t.pesoGramas]));
  let a = melhorA;
  while (a > 0 && escolha[a] >= 0) {
    const id = escolha[a];
    result[id] = (result[id] || 0) + 1;
    a -= pesoPorId.get(id) ?? a;
  }
  return result;
}

/** Custo (R$) de X gramas cozidas de um ingrediente (custo/kg cru ÷ rendimento). */
export function custoIngrediente(gramasCozidas: number, ing: IngredienteMock | undefined): number {
  if (!ing || ing.coeficiente <= 0) {
    return 0;
  }
  const custoRealKg = ing.custoKg / ing.coeficiente; // por kg cozido
  return (gramasCozidas / 1000) * custoRealKg;
}
