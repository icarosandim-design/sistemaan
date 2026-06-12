export interface TamanhoPacote {
  id: number;
  nome: string;
  pesoGramas: number;
  ativo: boolean;
  observacao: string | null;
}

export interface SalvarTamanhoPacoteRequest {
  nome: string;
  pesoGramas: number;
  observacao: string | null;
  ativo: boolean;
}

export function fmtPeso(gramas: number): string {
  if (gramas >= 1000) {
    return `${(gramas / 1000).toLocaleString('pt-BR', { maximumFractionDigits: 2 })} kg`;
  }
  return `${gramas.toLocaleString('pt-BR', { maximumFractionDigits: 0 })} g`;
}
