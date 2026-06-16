export interface MotivoCancelamento {
  id: number;
  nome: string;
  ordem: number;
  ativo: boolean;
  observacoes: string | null;
}

export interface SalvarMotivoCancelamentoRequest {
  nome: string;
  ordem: number;
  ativo: boolean;
  observacoes: string | null;
}
