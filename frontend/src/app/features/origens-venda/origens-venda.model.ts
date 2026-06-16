export interface OrigemVenda {
  id: number;
  nome: string;
  ordem: number;
  ativo: boolean;
}

export interface SalvarOrigemVendaRequest {
  nome: string;
  ordem: number;
  ativo: boolean;
}
