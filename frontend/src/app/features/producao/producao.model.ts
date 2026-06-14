// =====================================================================
// Módulo de Produção — contratos reais da API (espelham os DTOs do backend
// em SistemaAN.Application.Producao). Ver ProducaoController (api/producao).
// =====================================================================

// ----- Demanda (leitura de Entregas + Estoque) -----
export interface DemandaPersonalizada {
  entregaItemId: number;
  entregaId: number;
  entregaPetId: number;
  petId: number;
  clienteId: number;
  petNome: string;
  clienteNome: string;
  receitaCodigo: string;
  receitaNome: string;
  dataEntrega: string; // yyyy-MM-dd
  pacotes: number;
  pesoPacoteGramas: number;
  statusPreparo: StatusPreparo;
}

export interface DemandaCasa {
  receitaId: number;
  receitaNome: string;
  tamanhoPacoteId: number | null;
  tamanhoNome: string;
  pesoGramas: number;
  necessario: number;
  estoque: number;
  falta: number;
  itemEstoqueId: number | null;
}

export interface Demanda {
  personalizadas: DemandaPersonalizada[];
  casa: DemandaCasa[];
}

// ----- Ordem de produção -----
export type StatusPreparo = 'NaoPronta' | 'ParcialmentePronta' | 'Pronta';
export type StatusOrdem = 'Planejada' | 'EmAndamento' | 'Finalizada';
export type StatusFicha = 'Pendente' | 'EmPreparo' | 'Produzida' | 'Envasada' | 'Conferida' | 'NaoFeita';
export type TipoFicha = 'Casa' | 'Personalizada';

export interface FichaIngrediente {
  ingredienteId: number;
  ingredienteNome: string;
  categoria: string;
  gramasCozidas: number;
  coeficiente: number;
}

export interface FichaProducao {
  id: number;
  tipo: TipoFicha;
  entregaItemId: number | null;
  clienteNome: string | null;
  petNome: string | null;
  receitaCodigo: string;
  receitaNome: string;
  dataEntrega: string | null;
  quantidadePacotes: number;
  pesoPacoteGramas: number;
  quantidadeTotalGramas: number;
  status: StatusFicha;
  quantidadePacotesReal: number | null;
  motivoNaoFeita: string | null;
  observacoes: string | null;
  ingredientes: FichaIngrediente[];
}

export interface ConsumoConsolidado {
  ingredienteId: number;
  ingredienteNome: string;
  categoria: string;
  cozidoGramas: number;
  cruGramas: number;
  itemEstoqueId: number | null;
  semItemVinculado: boolean;
}

export interface OrdemProducao {
  id: number;
  data: string; // yyyy-MM-dd
  status: StatusOrdem;
  observacoes: string | null;
  fichas: FichaProducao[];
  consolidado: ConsumoConsolidado[];
}

export interface OrdemProducaoResumo {
  id: number;
  data: string;
  status: StatusOrdem;
  totalFichas: number;
}

// ----- Requests -----
export interface CriarOrdemRequest {
  data: string; // yyyy-MM-dd
}

export interface FichaCasaRequest {
  receitaId: number;
  tamanhoPacoteId: number;
  quantidadePacotes: number;
}

export interface AdicionarFichasRequest {
  entregaItemIds: number[];
  casa: FichaCasaRequest[];
}

export interface MudarStatusFichaRequest {
  status: StatusFicha;
}

export interface ConcluirFichaRequest {
  pacotesReais: number;
  pesoEnvasadoGramas?: number | null;
  observacoes?: string | null;
}

export interface MarcarNaoFeitaRequest {
  motivo: string;
}

export interface ConsumoRealRequest {
  ingredienteId: number;
  realCruGramas?: number | null;
  realCozidoGramas?: number | null;
  motivo?: string | null;
}

export interface RegistrarConsumoRequest {
  itens: ConsumoRealRequest[];
}

export interface FinalizarProducaoRequest {
  tudoProduzido: boolean;
  observacoes?: string | null;
}

export interface ResumoConsumo {
  ingredienteNome: string;
  planejadoCruGramas: number;
  realCruGramas: number | null;
  planejadoCozidoGramas: number;
  realCozidoGramas: number | null;
  baixaRealizada: boolean;
}

export interface FinalizacaoResultado {
  ordemId: number;
  status: StatusOrdem;
  fichasConcluidas: number;
  fichasNaoFeitas: number;
  produtoAcabadoGerado: number;
  personalizadasProntas: number;
  pendenciasEstoque: string[];
  consumos: ResumoConsumo[];
}

// ===== Formatação =====
export function fmtPeso(gramas: number): string {
  if (Math.abs(gramas) >= 1000) {
    return `${(gramas / 1000).toLocaleString('pt-BR', { maximumFractionDigits: 2 })} kg`;
  }
  return `${gramas.toLocaleString('pt-BR', { maximumFractionDigits: 0 })} g`;
}

export function fmtMoeda(valor: number): string {
  return valor.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
}

/** Quantidade por dia (por pacote) = total ÷ nº de pacotes. */
export function porDia(gramasTotal: number, pacotes: number): number {
  return pacotes > 0 ? gramasTotal / pacotes : gramasTotal;
}

// ===== Rótulos =====
export function rotuloStatusFicha(s: StatusFicha): string {
  switch (s) {
    case 'Pendente': return 'A fazer';
    case 'EmPreparo': return 'Em preparo';
    case 'Produzida': return 'Produzida';
    case 'Envasada': return 'Envasada';
    case 'Conferida': return 'Conferida';
    case 'NaoFeita': return 'Não feita';
  }
}

export function rotuloStatusPreparo(p: StatusPreparo | null): string {
  switch (p) {
    case 'Pronta': return 'Pronta';
    case 'ParcialmentePronta': return 'Parcialmente pronta';
    case 'NaoPronta': return 'Não pronta';
    default: return '—';
  }
}

export function rotuloStatusOrdem(s: StatusOrdem): string {
  switch (s) {
    case 'Planejada': return 'Planejada';
    case 'EmAndamento': return 'Em andamento';
    case 'Finalizada': return 'Finalizada';
  }
}
