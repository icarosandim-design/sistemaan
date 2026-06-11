export interface FaixaConsumo {
  id: number;
  pesoInicial: number;
  pesoFinal: number;
  gramasPorDia: number;
  ativo: boolean;
}

export interface SalvarFaixaConsumoRequest {
  pesoInicial: number;
  pesoFinal: number;
  gramasPorDia: number;
  ativo: boolean;
}

export function fmtKg(v: number): string {
  return `${v.toLocaleString('pt-BR', { maximumFractionDigits: 2 })} kg`;
}

export function fmtGramas(v: number): string {
  return `${v.toLocaleString('pt-BR', { maximumFractionDigits: 0 })} g/dia`;
}
