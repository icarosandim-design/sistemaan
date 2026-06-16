export interface Raca {
  id: number;
  nome: string;
  ordem: number;
  ativo: boolean;
}

export interface SalvarRacaRequest {
  nome: string;
  ordem: number;
  ativo: boolean;
}
