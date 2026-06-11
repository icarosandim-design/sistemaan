/**
 * Contrato de dados da Central Operacional.
 *
 * A Central é uma camada FINA de leitura/agregação: estes tipos representam o
 * que ela exibe. Hoje os dados vêm de mock (ver CentralService); futuramente o
 * mesmo serviço passará a consumir `GET /api/central/resumo`, sem alterar a tela.
 */

export type StatusEstoque = 'ok' | 'baixo';
export type Severidade = 'erro' | 'aviso' | 'info';

export interface Kpi {
  label: string;
  valor: string;
  icone: string;
}

export interface EntregaDia {
  diaSemana: string;
  data: string;
  entregas: number;
  hoje: boolean;
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

export interface AlertaOperacional {
  tipo: Severidade;
  icone: string;
  texto: string;
}

/** Resumo agregado exibido na Central Operacional. */
export interface CentralResumo {
  diaSelecionado: string;
  kpis: Kpi[];
  entregas7: EntregaDia[];
  producaoCasa: ProducaoCasaItem[];
  producaoPersonalizada: ProducaoPersonalizadaItem[];
  ingredientes: IngredienteProducao[];
  estoque: EstoqueItem[];
  alertas: AlertaOperacional[];
}
