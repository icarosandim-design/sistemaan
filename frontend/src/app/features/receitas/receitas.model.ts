export interface IngredienteAtivo {
  id: number;
  nome: string;
  categoria: string;
  coeficiente: number; // rendimento cozido/cru (para custo)
  custoKg: number; // custo por kg cru
}

export interface ItemReceita {
  ingredienteId: number;
  gramas: number; // gramas COZIDAS (o que vai na bacia)
}

export interface ReceitaCasa {
  id: number;
  codigo: string;
  nome: string;
  ativo: boolean;
  observacoes: string;
  itens: ItemReceita[];
}

/** Base obrigatória da ficha técnica da Receita da Casa: 1 kg cozido. */
export const BASE_GRAMAS = 1000;

export function fmtMoeda(valor: number): string {
  return valor.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
}

export function fmtGramas(v: number): string {
  return `${v.toLocaleString('pt-BR', { maximumFractionDigits: 0 })} g`;
}

/** Custo (R$) de um item: usa o custo real/kg do ingrediente (já embute a conversão). */
export function custoItem(gramasCozidas: number, ing: IngredienteAtivo | undefined): number {
  if (!ing || ing.coeficiente <= 0) {
    return 0;
  }
  const custoRealKg = ing.custoKg / ing.coeficiente; // por kg cozido
  return (gramasCozidas / 1000) * custoRealKg;
}

/** Custo total da ficha (base 1 kg = custo por kg cozido). */
export function custoReceita(itens: ItemReceita[], mapa: Map<number, IngredienteAtivo>): number {
  return itens.reduce((s, it) => s + custoItem(it.gramas, mapa.get(it.ingredienteId)), 0);
}

export function totalGramas(itens: ItemReceita[]): number {
  return itens.reduce((s, it) => s + (it.gramas || 0), 0);
}
