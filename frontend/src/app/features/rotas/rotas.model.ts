export type StatusRota = 'Rascunho' | 'Planejada' | 'Despachada' | 'Concluida' | 'Cancelada';
export type PeriodoRota = 'Manha' | 'Tarde' | 'HorarioComercial' | 'Extra';

export const PERIODOS_ROTA: { valor: PeriodoRota; rotulo: string }[] = [
  { valor: 'Manha', rotulo: 'Manhã' },
  { valor: 'Tarde', rotulo: 'Tarde' },
  { valor: 'HorarioComercial', rotulo: 'Horário comercial' },
  { valor: 'Extra', rotulo: 'Extra' },
];

export function rotuloPeriodo(p: string): string {
  return PERIODOS_ROTA.find((x) => x.valor === p)?.rotulo ?? p;
}

export function rotuloStatusRota(s: StatusRota): string {
  switch (s) {
    case 'Rascunho': return 'Rascunho';
    case 'Planejada': return 'Planejada';
    case 'Despachada': return 'Despachada';
    case 'Concluida': return 'Concluída';
    case 'Cancelada': return 'Cancelada';
  }
}

export interface RotaResumo {
  id: number;
  data: string;
  nome: string;
  periodo: string;
  entregador: string | null;
  status: StatusRota;
  totalEntregas: number;
}

export interface RotaParada {
  entregaId: number;
  ordem: number;
  clienteNome: string;
  ehPj: boolean;
  pedidoId: number | null;
  endereco: string;
  bairro: string | null;
  cidade: string | null;
  telefone: string | null;
  preferenciaHorario: string;
  statusEntrega: string;
  itensResumo: string;
  enderecoIncompleto: boolean;
  prontidaoTexto: string | null;
  estoqueAlerta: string; // '' | 'ok' | 'falta' | 'naocadastrado'
}

export interface Rota {
  id: number;
  data: string;
  nome: string;
  periodo: string;
  entregador: string | null;
  status: StatusRota;
  observacoes: string | null;
  paradas: RotaParada[];
}

export interface EntregaDisponivel {
  id: number;
  clienteNome: string;
  ehPj: boolean;
  pedidoId: number | null;
  endereco: string;
  bairro: string | null;
  cidade: string | null;
  telefone: string | null;
  preferenciaHorario: string;
  statusEntrega: string;
  itensResumo: string;
  enderecoIncompleto: boolean;
  prontidaoTexto: string | null;
  emRotaAtivaId: number | null;
}

export interface CriarRotaRequest {
  data: string;
  nome: string;
  periodo: string;
  entregador?: string | null;
  observacoes?: string | null;
}

export interface AtualizarRotaRequest {
  nome: string;
  periodo: string;
  entregador?: string | null;
  observacoes?: string | null;
}

/** Alerta quando a preferência do cliente não combina com o período da saída. */
export function preferenciaIncompativel(periodoRota: string, preferencia: string): boolean {
  if (periodoRota === 'Manha' && preferencia === 'Tarde') return true;
  if (periodoRota === 'Tarde' && preferencia === 'Manha') return true;
  return false;
}
