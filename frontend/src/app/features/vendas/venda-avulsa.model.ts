export interface VendaAvulsaItemRequest {
  produtoId: number;
  quantidade: number;
  precoUnitario?: number | null;
  observacao?: string | null;
}

export interface SalvarVendaAvulsaRequest {
  clienteId: number;
  petId: number;
  dataEntrega: string;
  observacoes?: string | null;
  formaPagamento?: string | null;
  itens: VendaAvulsaItemRequest[];
}

export interface VendaAvulsaResultado {
  vendaAvulsaId: number;
  entregaId: number;
  dataEntrega: string;
  status: string;
  valorTotal: number;
  totalPacotes: number;
}
