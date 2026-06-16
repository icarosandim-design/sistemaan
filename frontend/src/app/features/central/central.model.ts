/**
 * Contrato de dados da Central Operacional.
 *
 * A Central é uma camada FINA de leitura/agregação: estes tipos representam o
 * que ela exibe. Os dados vêm de `GET /api/central/resumo` (agregação real no
 * backend); a tela apenas resume e direciona para os módulos donos.
 */

export type StatusEstoque = 'ok' | 'baixo';
export type Severidade = 'erro' | 'aviso' | 'info';

export interface Kpi {
  label: string;
  valor: string;
  icone: string;
  /** Cartão financeiro: só o Administrador enxerga. */
  somenteAdmin: boolean;
}

export interface EntregaDia {
  /** Data ISO (yyyy-MM-dd). A tela formata dia da semana e dd/MM. */
  data: string;
  entregas: number;
  pf: number;
  pj: number;
  alertas: number;
  hoje: boolean;
}

/** Resumo da ordem de produção do dia (nulo quando não há ordem). */
export interface ProducaoResumo {
  ordemId: number;
  status: string;
  finalizada: boolean;
  totalFichas: number;
  casa: number;
  personalizadas: number;
  pendencias: number;
}

export interface ProducaoCasaItem {
  produto: string;
  pacotes: number;
}

export interface ProducaoPersonalizadaItem {
  pet: string;
  codigo: string;
  tutor: string;
  pacotes: number;
}

export interface IngredienteProducao {
  nome: string;
  cozidos: number;
  crus: number;
  estoque: number;
}

export interface EstoqueItem {
  produto: string;
  saldo: number;
  minimo: number;
  status: StatusEstoque;
}

/** Resumo das saídas/rotas do dia (nulo quando não há rotas nem entregas pendentes). */
export interface RotasResumo {
  total: number;
  planejadas: number;
  despachadas: number;
  concluidas: number;
  semEntregador: number;
  entregasSemRota: number;
}

export interface AlertaOperacional {
  tipo: Severidade;
  icone: string;
  texto: string;
  /** Atalho para o módulo dono (ou null quando não houver). */
  rota: string | null;
}

/** Resumo agregado exibido na Central Operacional. */
export interface CentralResumo {
  /** Data ISO (yyyy-MM-dd) de referência do resumo. */
  diaSelecionado: string;
  kpis: Kpi[];
  entregas7: EntregaDia[];
  producao: ProducaoResumo | null;
  producaoCasa: ProducaoCasaItem[];
  producaoPersonalizada: ProducaoPersonalizadaItem[];
  ingredientes: IngredienteProducao[];
  estoque: EstoqueItem[];
  rotas: RotasResumo | null;
  alertas: AlertaOperacional[];
}
