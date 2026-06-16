// ===== Tipos e enums (espelham o backend) =====
export type TipoItemEstoque = 'Insumo' | 'ProdutoAcabadoCasa';

export const UNIDADES_MEDIDA = ['Kg', 'G', 'Unidade', 'Pacote', 'Caixa', 'Litro', 'Ml', 'Outro'] as const;

export const CATEGORIAS_FORNECEDOR = ['Ingredientes', 'Embalagens', 'Etiquetas', 'MaterialLimpeza', 'Servicos', 'Outros'] as const;

export const TIPOS_SAIDA = ['SaidaProducao', 'Descarte', 'Perda', 'Vencimento', 'TransferenciaSaida', 'ConsumoInterno'] as const;

const ROTULOS: Record<string, string> = {
  // Categorias de estoque
  Proteinas: 'Proteínas', Carboidratos: 'Carboidratos', Legumes: 'Legumes', Visceras: 'Vísceras',
  Suplementos: 'Suplementos', Embalagens: 'Embalagens', Etiquetas: 'Etiquetas',
  MateriaisLimpeza: 'Materiais de limpeza', MateriaisAuxiliares: 'Materiais auxiliares',
  ProdutoAcabado: 'Produto acabado', Outros: 'Outros',
  // Unidades
  Kg: 'kg', G: 'g', Unidade: 'unidade', Pacote: 'pacote', Caixa: 'caixa', Litro: 'litro', Ml: 'ml', Outro: 'outro',
  // Categorias de fornecedor
  Ingredientes: 'Ingredientes', MaterialLimpeza: 'Material de limpeza', Servicos: 'Serviços',
  // Tipo de item
  Insumo: 'Insumo', ProdutoAcabadoCasa: 'Produto acabado (Casa)',
  // Tipos de movimentação
  EntradaCompra: 'Entrada (compra)', EntradaProducao: 'Entrada (produção)', AjustePositivo: 'Ajuste +',
  TransferenciaEntrada: 'Transferência (entrada)', SaidaProducao: 'Saída (produção)', Descarte: 'Descarte',
  Perda: 'Perda', Vencimento: 'Vencimento', AjusteNegativo: 'Ajuste −', TransferenciaSaida: 'Transferência (saída)',
  ConsumoInterno: 'Consumo interno', BaixaEntrega: 'Baixa (entrega)',
  // Status de lote
  Ativo: 'Ativo', Esgotado: 'Esgotado', Vencido: 'Vencido', Bloqueado: 'Bloqueado',
};

export function rotulo(valor: string | null | undefined): string {
  if (!valor) {
    return '—';
  }
  return ROTULOS[valor] ?? valor;
}

// ===== Fornecedor =====
export interface Fornecedor {
  id: number;
  nome: string;
  nomeFantasia: string | null;
  documento: string | null;
  telefone: string | null;
  whatsApp: string | null;
  email: string | null;
  pessoaContato: string | null;
  endereco: string | null;
  cidade: string | null;
  estado: string | null;
  categoria: string | null;
  observacoes: string | null;
  prazoPagamentoDias: number | null;
  formaPagamentoPreferida: string | null;
  chavePix: string | null;
  dadosBancarios: string | null;
  ativo: boolean;
}

export interface SalvarFornecedorRequest {
  nome: string;
  nomeFantasia: string | null;
  documento: string | null;
  telefone: string | null;
  whatsApp: string | null;
  email: string | null;
  pessoaContato: string | null;
  endereco: string | null;
  cidade: string | null;
  estado: string | null;
  categoria: string | null;
  observacoes: string | null;
  prazoPagamentoDias: number | null;
  formaPagamentoPreferida: string | null;
  chavePix: string | null;
  dadosBancarios: string | null;
  ativo: boolean;
}

// ===== Item de estoque =====
export interface ItemEstoque {
  id: number;
  tipo: TipoItemEstoque;
  nome: string;
  categoria: string;
  unidadeMedida: string;
  ingredienteId: number | null;
  ingredienteNome: string | null;
  receitaId: number | null;
  receitaNome: string | null;
  tamanhoPacoteId: number | null;
  tamanhoPacoteNome: string | null;
  quantidadeAtual: number;
  quantidadeMinima: number;
  custoMedio: number;
  fornecedorPrincipalId: number | null;
  fornecedorPrincipalNome: string | null;
  localArmazenamento: string | null;
  controlaValidade: boolean;
  ativo: boolean;
  observacoes: string | null;
  abaixoDoMinimo: boolean;
}

/** "Estoque" dinâmico de Receita Personalizada (pacotes prontos, ainda não entregues). */
export interface PersonalizadaPronta {
  entregaId: number;
  receitaCodigo: string;
  receitaNome: string;
  pesoGramas: number;
  petNome: string;
  clienteNome: string;
  dataPrevista: string;
  pacotesProntos: number;
  statusEntrega: string;
}

export interface CriarItemInsumoRequest {
  nome: string;
  categoria: string;
  unidadeMedida: string;
  ingredienteId: number | null;
  quantidadeMinima: number;
  fornecedorPrincipalId: number | null;
  localArmazenamento: string | null;
  controlaValidade: boolean;
  observacoes: string | null;
  ativo: boolean;
}

export interface CriarItemProdutoAcabadoRequest {
  nome: string;
  receitaId: number;
  tamanhoPacoteId: number;
  quantidadeMinima: number;
  localArmazenamento: string | null;
  controlaValidade: boolean;
  observacoes: string | null;
  ativo: boolean;
}

export interface AtualizarItemEstoqueRequest {
  nome: string;
  categoria: string;
  unidadeMedida: string;
  quantidadeMinima: number;
  fornecedorPrincipalId: number | null;
  localArmazenamento: string | null;
  controlaValidade: boolean;
  observacoes: string | null;
  ativo: boolean;
}

// ===== Lote / Movimentação =====
export interface LoteEstoque {
  id: number;
  itemEstoqueId: number;
  codigo: string;
  dataEntrada: string;
  validade: string | null;
  quantidadeInicial: number;
  quantidadeAtual: number;
  custoUnitario: number;
  fornecedorId: number | null;
  fornecedorNome: string | null;
  origem: string;
  status: string;
}

export interface MovimentacaoEstoque {
  id: number;
  itemEstoqueId: number;
  itemNome: string;
  loteEstoqueId: number | null;
  loteCodigo: string | null;
  tipo: string;
  sentido: string;
  quantidade: number;
  saldoAnteriorItem: number;
  saldoPosteriorItem: number;
  custoUnitario: number;
  valorTotal: number;
  usuario: string;
  dataHora: string;
  motivoCodigo: string | null;
  motivo: string | null;
  observacao: string | null;
}

// ===== Operações =====
export interface RegistrarEntradaRequest {
  itemEstoqueId: number;
  quantidade: number;
  valorUnitario: number | null;
  valorTotal: number | null;
  fornecedorId: number | null;
  dataCompra: string;
  dataEntrada: string;
  validade: string | null;
  loteCodigo: string | null;
  localArmazenamento: string | null;
  observacoes: string | null;
  frete: number | null;
}

// ===== Compra com vários itens (uma nota) =====
export interface CompraItemRequest {
  itemEstoqueId: number;
  quantidade: number;
  valorUnitario?: number | null;
  valorTotal?: number | null;
  validade?: string | null;
  loteCodigo?: string | null;
  localArmazenamento?: string | null;
}

export interface RegistrarCompraRequest {
  fornecedorId: number | null;
  dataCompra: string;
  dataEntrada: string;
  notaFiscal?: string | null;
  frete?: number | null;
  observacoes?: string | null;
  itens: CompraItemRequest[];
}

export interface CompraResultado {
  itensRegistrados: number;
  valorProdutos: number;
  frete: number;
  totalPago: number;
  itens: ItemEstoque[];
}

export interface RegistrarSaidaRequest {
  itemEstoqueId: number;
  quantidade: number;
  tipo: string;
  motivoCodigo: string | null;
  motivo: string | null;
  observacao: string | null;
}

export interface RegistrarAjusteRequest {
  itemEstoqueId: number;
  loteEstoqueId: number | null;
  novaQuantidade: number;
  motivo: string;
  observacao: string | null;
}

export interface OpcaoSimples {
  id: number;
  nome: string;
}

// ===== Movimentações (livro-razão geral) =====
export interface MovimentacaoGeral {
  id: number;
  dataHora: string;
  itemEstoqueId: number;
  itemNome: string;
  categoria: string;
  tipo: string;
  sentido: string;
  quantidade: number;
  unidade: string;
  loteCodigo: string | null;
  custoUnitario: number;
  valorTotal: number;
  saldoAnteriorItem: number;
  saldoPosteriorItem: number;
  usuario: string;
  motivo: string | null;
  observacao: string | null;
  fornecedorNome: string | null;
  origem: string;
}

export interface MovimentacaoPagina {
  total: number;
  itens: MovimentacaoGeral[];
}

export interface FiltroMovimentacoes {
  dataInicio?: string | null;
  dataFim?: string | null;
  itemEstoqueId?: number | null;
  categoria?: string | null;
  tipo?: string | null;
  fornecedorId?: number | null;
  loteEstoqueId?: number | null;
  usuario?: string | null;
  motivo?: string | null;
  origem?: string | null;
  pagina?: number;
  tamanhoPagina?: number;
}

// ===== Compras / Entradas =====
export interface EntradaCompra {
  id: number;
  dataCompra: string;
  dataEntrada: string;
  fornecedorId: number | null;
  fornecedorNome: string | null;
  itemEstoqueId: number;
  itemNome: string;
  categoria: string;
  quantidade: number;
  unidade: string;
  valorUnitario: number;
  custoUnitarioEstoque: number;
  valorProdutos: number;
  frete: number;
  valorTotalPago: number;
  loteCodigo: string;
  validade: string | null;
  usuario: string;
  observacoes: string | null;
}

export interface FiltroEntradas {
  dataInicio?: string | null;
  dataFim?: string | null;
  fornecedorId?: number | null;
  itemEstoqueId?: number | null;
  categoria?: string | null;
  loteEstoqueId?: number | null;
  usuario?: string | null;
  valorMin?: number | null;
  valorMax?: number | null;
  comFrete?: boolean | null;
}

/** Tipos de movimentação para filtros. */
export const TIPOS_MOVIMENTACAO = [
  'EntradaCompra', 'EntradaProducao', 'AjustePositivo', 'TransferenciaEntrada',
  'SaidaProducao', 'Descarte', 'Perda', 'Vencimento', 'AjusteNegativo',
  'TransferenciaSaida', 'ConsumoInterno', 'BaixaEntrega',
] as const;

export const ORIGENS_MOVIMENTACAO: { valor: string; label: string }[] = [
  { valor: 'Compra', label: 'Compra' },
  { valor: 'Ajuste', label: 'Ajuste' },
  { valor: 'Producao', label: 'Produção' },
  { valor: 'Entrega', label: 'Entrega' },
];

// ===== Helpers de formatação =====
export function fmtQtd(n: number): string {
  return n.toLocaleString('pt-BR', { maximumFractionDigits: 3 });
}

export function fmtMoeda(n: number): string {
  return n.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
}
