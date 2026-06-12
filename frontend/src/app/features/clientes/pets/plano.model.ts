// ============================================================================
// Plano Alimentar do Pet — integrado à API real.
// Frequências, Receitas da Casa, Ingredientes e Tamanhos de Pacote vêm dos
// módulos reais; o plano e as receitas personalizadas são persistidos.
// ============================================================================

import { TamanhoPacote } from '../../tamanhos-pacote/tamanhos-pacote.model';

export type TipoAlimentacao = 'Casa' | 'Personalizada';

/** Quantidades de pacotes por tamanho: { tamanhoId: quantidade } (uso interno na tela, Casa). */
export type Pacotes = Record<number, number>;

// ---- Plano (DTO da API) ----
export interface PlanoItemPacoteApi {
  tamanhoPacoteId: number;
  quantidade: number;
}

export interface PlanoItemApi {
  id: number;
  receitaId: number;
  quantidadeCicloGramas: number | null;
  quantidadePacotes: number | null;
  pacotes: PlanoItemPacoteApi[];
}

export interface PlanoAlimentar {
  id: number;
  petId: number;
  gramasDiaSugeridas: number | null;
  gramasDiaAjustadas: number | null;
  tipo: TipoAlimentacao;
  ativo: boolean;
  itens: PlanoItemApi[];
}

export interface SalvarPlanoItemRequest {
  receitaId: number;
  quantidadeCicloGramas?: number | null;
  quantidadePacotes?: number | null;
  pacotes?: PlanoItemPacoteApi[];
}

export interface SalvarPlanoRequest {
  gramasDiaSugeridas?: number | null;
  gramasDiaAjustadas?: number | null;
  tipo: TipoAlimentacao;
  itens: SalvarPlanoItemRequest[];
}

// ---- Receita Personalizada (DTO da API) ----
export interface ItemReceitaPersonalizadaApi {
  ingredienteId: number;
  ingrediente: string;
  categoria: string;
  gramas: number;
}

export interface ReceitaPersonalizada {
  id: number;
  petId: number;
  codigo: string;
  nome: string;
  ativo: boolean;
  observacoes: string;
  itens: ItemReceitaPersonalizadaApi[];
  pesoTotalGramas: number;
  custoPacote: number;
  custoPorKgCozido: number;
}

export interface SalvarItemReceitaPersonalizadaRequest {
  ingredienteId: number;
  gramas: number;
}

export interface SalvarReceitaPersonalizadaRequest {
  codigo: string;
  nome: string;
  ativo: boolean;
  observacoes: string;
  itens: SalvarItemReceitaPersonalizadaRequest[];
}

// ---- Estado interno da tela ----
export interface ItemCasa {
  receitaId: number | null;
  pacotes: Pacotes;
}

export interface ItemPersonalizado {
  ingredienteId: number | null;
  gramasCozidas: number;
}

/** Receita personalizada em edição na tela (uid local; id = backend quando salva). */
export interface ReceitaPersEdit {
  uid: number;
  id: number | null;
  codigo: string;
  observacoesPreparo: string;
  itens: ItemPersonalizado[];
  quantidadePacotes: number;
}

// ---- Custo (mesma regra do módulo Ingredientes) ----
export interface CustoIngredienteBase {
  coeficiente: number; // rendimento cozido ÷ cru
  custoKg: number; // custo por kg cru
}

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

export function gramasPacotes(pacotes: Pacotes, tamanhos: TamanhoPacote[]): number {
  return tamanhos.reduce((s, t) => s + (pacotes[t.id] || 0) * t.pesoGramas, 0);
}

/**
 * Sugestão de pacotes para cobrir um alvo (gramas) com os tamanhos ATIVOS.
 * Prioriza: (1) menor nº de pacotes; (2) menor sobra. Nunca menos que o alvo.
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

  const INF = Number.POSITIVE_INFINITY;
  const minCount = new Array<number>(limite + 1).fill(INF);
  const escolha = new Array<number>(limite + 1).fill(-1);
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

/** Custo (R$) de X gramas cozidas (custo/kg cru ÷ rendimento). */
export function custoIngrediente(gramasCozidas: number, ing: CustoIngredienteBase | undefined): number {
  if (!ing || ing.coeficiente <= 0) {
    return 0;
  }
  return (gramasCozidas / 1000) * (ing.custoKg / ing.coeficiente);
}
