export interface VendaAvulsaItemRequest {
  receitaId: number;
  tamanhoPacoteId: number;
  quantidade: number;
  observacao?: string | null;
}

export interface SalvarVendaAvulsaRequest {
  clienteId: number;
  petId: number;
  dataEntrega: string;
  observacoes?: string | null;
  valor?: number | null;
  formaPagamento?: string | null;
  itens: VendaAvulsaItemRequest[];
}

export interface VendaAvulsaResultado {
  entregaId: number;
  dataEntrega: string;
  status: string;
  totalPacotes: number;
}
