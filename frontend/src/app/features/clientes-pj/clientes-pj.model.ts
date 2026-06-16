export interface ClientePjResumo {
  id: number;
  nomeFantasia: string;
  razaoSocial: string;
  cnpj: string;
  cidade: string | null;
  telefone: string | null;
  tipoPJ: string;
  ativo: boolean;
}

export interface ClientePj {
  id: number;
  razaoSocial: string;
  nomeFantasia: string;
  cnpj: string;
  inscricaoEstadual: string | null;
  telefone: string | null;
  whatsapp: string | null;
  email: string | null;
  pessoaContato: string | null;
  cargoContato: string | null;
  rua: string | null;
  numero: string | null;
  complemento: string | null;
  bairro: string | null;
  cidade: string | null;
  estado: string | null;
  cep: string | null;
  entregaRua: string | null;
  entregaNumero: string | null;
  entregaComplemento: string | null;
  entregaBairro: string | null;
  entregaCidade: string | null;
  entregaEstado: string | null;
  entregaCep: string | null;
  tipoPJ: string;
  condicaoComercial: string | null;
  prazoPagamento: string | null;
  diaEntregaPreferencial: number | null;
  frequenciaCompra: string | null;
  observacoes: string | null;
  observacoesComerciais: string | null;
  preferenciaHorario: string;
  ativo: boolean;
  atualizadoEm: string | null;
}

export type SalvarClientePjRequest = Omit<ClientePj, 'id' | 'ativo' | 'atualizadoEm'>;

export interface TipoPjOpcao {
  valor: string;
  rotulo: string;
}

// ----- Pedidos -----
export type StatusPedido = 'Rascunho' | 'Confirmado' | 'Cancelado';

export interface PedidoItem {
  id: number;
  receitaId: number;
  receitaCodigo: string;
  receitaNome: string;
  tamanhoPacoteId: number;
  tamanhoNome: string;
  pesoGramas: number;
  quantidade: number;
  observacao: string | null;
  produtoId: number | null;
  precoUnitario: number | null;
  valorTotalItem: number | null;
}

export interface Pedido {
  id: number;
  clienteId: number;
  clienteNome: string;
  dataPedido: string;
  dataEntrega: string;
  status: StatusPedido;
  observacoes: string | null;
  entregaId: number | null;
  entregaStatus: string | null;
  totalPacotes: number;
  valorTotal: number | null;
  itens: PedidoItem[];
}

export interface PedidoResumo {
  id: number;
  clienteId: number;
  dataPedido: string;
  dataEntrega: string;
  status: StatusPedido;
  totalItens: number;
  totalPacotes: number;
  entregaId: number | null;
  entregaStatus: string | null;
}

export interface SalvarPedidoItemRequest {
  receitaId: number;
  tamanhoPacoteId: number;
  quantidade: number;
  observacao?: string | null;
  produtoId?: number | null;
  precoUnitario?: number | null;
}

export interface SalvarPedidoRequest {
  clienteId: number;
  dataPedido: string;
  dataEntrega: string;
  observacoes?: string | null;
  itens: SalvarPedidoItemRequest[];
}
