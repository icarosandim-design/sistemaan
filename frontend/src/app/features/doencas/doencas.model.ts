export interface Doenca {
  id: number;
  nome: string;
  ordem: number;
  ativo: boolean;
}

export interface SalvarDoencaRequest {
  nome: string;
  ordem: number;
  ativo: boolean;
}
