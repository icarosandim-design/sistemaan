// Interfaces espelhando os DTOs do backend (JSON camelCase).

export interface RelatorioPeriodo {
  inicio: string;
  fim: string;
}

export interface ChaveValor {
  chave: string;
  kg: number;
  quantidade: number;
}

// ---- Dashboard ----
export interface DashboardCard {
  chave: string;
  label: string;
  valor: string;
  detalhe: string | null;
  estimado: boolean;
  pendente: boolean;
}
export interface VendasPorMes {
  mes: string;
  kg: number;
  quantidade: number;
  receitaRecorrenteAtiva: number;
}
export interface VendasPorTipo {
  tipo: string;
  kg: number;
  quantidade: number;
}
export interface CancelamentoPorMotivo {
  motivo: string;
  quantidade: number;
  receitaMensalPerdida: number;
  kgMensalPerdido: number;
}
export interface ProducaoPlanReal {
  periodo: string;
  kgPlanejado: number;
  kgReal: number;
  diferenca: number;
}
export interface CustoPerdaIngrediente {
  ingrediente: string;
  perdaKg: number;
  perdaValor: number;
  sobraKg: number;
  sobraValor: number;
  estimado: boolean;
}
export interface EvolucaoCustoPonto {
  mes: string;
  custoMedioCompra: number;
  ultimoCusto: number;
  variacaoPercentual: number;
}
export interface Dashboard {
  periodo: RelatorioPeriodo;
  cards: DashboardCard[];
  vendasPorMes: VendasPorMes[];
  vendasPorTipo: VendasPorTipo[];
  cancelamentosPorMotivo: CancelamentoPorMotivo[];
  producaoPlanejadoReal: ProducaoPlanReal[];
  custoPerdasTop: CustoPerdaIngrediente[];
  ingredienteEvolucaoId: number | null;
  ingredienteEvolucaoNome: string | null;
  evolucaoCustoInsumo: EvolucaoCustoPonto[];
  receitaPorVendaIndisponivel: boolean;
}

// ---- Vendas ----
export interface VendaLinha {
  data: string;
  tipo: string;
  cliente: string;
  pet: string | null;
  valor: number | null;
  custo: number;
  origem: string | null;
}
export interface RelatorioVendasResumo {
  quantidadeVendas: number;
  totalKg: number;
  receitaRecorrenteAtivaMensal: number;
  custoTotalEstimado: number;
  porTipo: VendasPorTipo[];
  porReceita: ChaveValor[];
  porCidade: ChaveValor[];
  receitaPorVendaIndisponivel: boolean;
}
export interface RelatorioVendas {
  periodo: RelatorioPeriodo;
  resumo: RelatorioVendasResumo;
  linhas: VendaLinha[];
  total: number;
  pagina: number;
  tamanhoPagina: number;
}

// ---- Cancelamentos ----
export interface CancelamentoLinha {
  data: string | null;
  cliente: string;
  pets: string | null;
  motivo: string;
  observacao: string | null;
  valorMensalPerdido: number;
  kgMensalPerdido: number;
  diasComoCliente: number | null;
  usuario: string | null;
}
export interface RelatorioCancelamentosResumo {
  totalCancelamentos: number;
  receitaMensalPerdida: number;
  kgMensalPerdido: number;
  ticketMedioMensal: number;
  tempoMedioDiasAteCancelamento: number | null;
  porMotivo: CancelamentoPorMotivo[];
  porCidade: ChaveValor[];
}
export interface RelatorioCancelamentos {
  periodo: RelatorioPeriodo;
  resumo: RelatorioCancelamentosResumo;
  linhas: CancelamentoLinha[];
  total: number;
  pagina: number;
  tamanhoPagina: number;
}

// ---- Produção planejado x real ----
export interface ProducaoLinha {
  data: string;
  ingrediente: string;
  planejadoCruGramas: number;
  realCruGramas: number;
  planejadoCozidoGramas: number;
  realCozidoGramas: number;
  diferencaCruGramas: number;
  custoPlanejado: number;
  custoRealEstimado: number;
  status: string;
}
export interface RelatorioProducaoResumo {
  producoesPlanejadas: number;
  producoesFinalizadas: number;
  kgPlanejado: number;
  kgReal: number;
  cruPrevistoGramas: number;
  cruRealGramas: number;
  diferencaCruGramas: number;
  cozidoPrevistoGramas: number;
  cozidoRealGramas: number;
  diferencaCozidoGramas: number;
  custoPlanejado: number;
  custoRealEstimado: number;
  diferencaCusto: number;
  custoEstimado: boolean;
}
export interface RelatorioProducao {
  periodo: RelatorioPeriodo;
  resumo: RelatorioProducaoResumo;
  linhas: ProducaoLinha[];
  total: number;
  pagina: number;
  tamanhoPagina: number;
}

// ---- Perdas ----
export interface PerdaLinha {
  data: string;
  ingrediente: string;
  perdaKg: number;
  perdaValor: number;
  sobraKg: number;
  sobraValor: number;
  custoMedioUsado: number;
  fatorCadastrado: number | null;
  fatorReal: number | null;
  estimado: boolean;
}
export interface RelatorioPerdasResumo {
  perdaTotalKg: number;
  perdaTotalValor: number;
  sobraTotalKg: number;
  sobraTotalValor: number;
  porIngrediente: CustoPerdaIngrediente[];
  estimado: boolean;
}
export interface RelatorioPerdas {
  periodo: RelatorioPeriodo;
  resumo: RelatorioPerdasResumo;
  linhas: PerdaLinha[];
  total: number;
  pagina: number;
  tamanhoPagina: number;
}

// ---- Custos de insumos ----
export interface CustoInsumoLinha {
  data: string;
  ingrediente: string;
  fornecedor: string | null;
  quantidade: number;
  unidade: string;
  valorUnitario: number;
  valorTotal: number;
  origem: string;
}
export interface CustoInsumoResumoItem {
  ingredienteId: number;
  ingrediente: string;
  custoMedioAtual: number;
  ultimoCusto: number;
  menorCustoPeriodo: number;
  maiorCustoPeriodo: number;
  quantidadeComprada: number;
  valorTotalComprado: number;
  variacaoValor: number;
  variacaoPercentual: number;
}
export interface RelatorioCustosInsumos {
  periodo: RelatorioPeriodo;
  resumo: CustoInsumoResumoItem[];
  linhas: CustoInsumoLinha[];
  total: number;
  pagina: number;
  tamanhoPagina: number;
  historicoLimitado: boolean;
}

// ---- Estoque ----
export interface EstoqueLinha {
  item: string;
  tipo: string;
  saldoFisico: number;
  unidade: string;
  minimo: number;
  custoMedio: number;
  valorEstimado: number;
  status: string;
}
export interface RelatorioEstoqueResumo {
  totalItens: number;
  itensAbaixoMinimo: number;
  itensSemSaldo: number;
  valorEstimadoTotal: number;
  produtoAcabadoComSaldo: number;
  produtoAcabadoSemSaldo: number;
  comprometidoIndisponivel: boolean;
}
export interface RelatorioEstoque {
  resumo: RelatorioEstoqueResumo;
  linhas: EstoqueLinha[];
}

// ---- Filtros ----
export interface RelatorioFiltro {
  inicio?: string;
  fim?: string;
  tipo?: string;
  clienteId?: number;
  receitaId?: number;
  ingredienteId?: number;
  motivoId?: number;
  fornecedorId?: number;
  status?: string;
  cidade?: string;
  bairro?: string;
  pagina?: number;
  tamanhoPagina?: number;
}
