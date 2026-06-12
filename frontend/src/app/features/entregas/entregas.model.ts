export type EntregaStatus =
  | 'Programada'
  | 'ConfirmadaCliente'
  | 'SaiuParaEntrega'
  | 'Entregue'
  | 'NaoEntregue'
  | 'Reagendada'
  | 'Cancelada';

export interface EntregaResumo {
  id: number;
  clienteId: number;
  clienteNome: string;
  telefone: string | null;
  dataPrevista: string; // aaaa-mm-dd
  status: EntregaStatus;
  bairro: string | null;
  cidade: string | null;
  tipos: string;
  totalGramas: number;
  totalPacotes: number;
  entregadorId: number | null;
  pets: string[];
}

export interface EntregaItemPacote {
  tamanhoLabel: string;
  pesoGramas: number;
  quantidade: number;
}

export interface EntregaItemIngrediente {
  ingredienteId: number;
  nome: string;
  categoria: string;
  gramasCozidas: number;
}

export interface EntregaItem {
  id: number;
  receitaId: number;
  receitaCodigo: string;
  receitaNome: string;
  tipo: 'Casa' | 'Personalizada';
  quantidadeCicloGramas: number | null;
  tamanhoPacoteGramas: number | null;
  quantidadePacotes: number | null;
  pacotes: EntregaItemPacote[];
  ingredientes: EntregaItemIngrediente[];
}

export interface EntregaPet {
  id: number;
  petId: number;
  petNome: string;
  tipoAlimentacao: 'Casa' | 'Personalizada';
  gramasDia: number | null;
  quantidadeTotalGramas: number;
  itens: EntregaItem[];
}

export interface EntregaHistorico {
  quando: string;
  usuario: string;
  evento: string;
  statusDe: string | null;
  statusPara: string | null;
}

export interface EntregaDetalhe {
  id: number;
  clienteId: number;
  dataPrevista: string;
  status: EntregaStatus;
  clienteNome: string;
  telefone: string | null;
  rua: string | null;
  numero: string | null;
  complemento: string | null;
  cep: string | null;
  bairro: string | null;
  cidade: string | null;
  estado: string | null;
  frequenciaNome: string;
  diasCiclo: number;
  observacoesInternas: string | null;
  observacoesEntregador: string | null;
  entregadorId: number | null;
  motivoNaoEntrega: string | null;
  motivoReagendamento: string | null;
  reagendadaDeId: number | null;
  reagendadaParaId: number | null;
  motivoCancelamento: string | null;
  pets: EntregaPet[];
  historico: EntregaHistorico[];
}

export const STATUS_ENTREGA: { valor: EntregaStatus; label: string }[] = [
  { valor: 'Programada', label: 'Programada' },
  { valor: 'ConfirmadaCliente', label: 'Confirmada' },
  { valor: 'SaiuParaEntrega', label: 'Saiu p/ entrega' },
  { valor: 'Entregue', label: 'Entregue' },
  { valor: 'NaoEntregue', label: 'Não entregue' },
  { valor: 'Reagendada', label: 'Reagendada' },
  { valor: 'Cancelada', label: 'Cancelada' },
];

export function labelStatus(s: string): string {
  return STATUS_ENTREGA.find((x) => x.valor === s)?.label ?? s;
}

/** Classe CSS por status (cores). */
export function classeStatus(s: string): string {
  switch (s) {
    case 'Entregue':
    case 'ConfirmadaCliente':
      return 'st-ok';
    case 'SaiuParaEntrega':
      return 'st-rota';
    case 'NaoEntregue':
      return 'st-erro';
    case 'Reagendada':
      return 'st-aviso';
    case 'Cancelada':
      return 'st-cancel';
    default:
      return 'st-prog';
  }
}

export function fmtPeso(gramas: number): string {
  if (Math.abs(gramas) >= 1000) {
    return `${(gramas / 1000).toLocaleString('pt-BR', { maximumFractionDigits: 2 })} kg`;
  }
  return `${gramas.toLocaleString('pt-BR', { maximumFractionDigits: 0 })} g`;
}

export const MOTIVOS_NAO_ENTREGA = [
  'Cliente ausente',
  'Endereço incorreto',
  'Cliente pediu para reagendar',
  'Problema com entregador',
  'Problema no veículo',
  'Pagamento pendente',
  'Produto não saiu para entrega',
  'Outro',
];

export const MOTIVOS_REAGENDAMENTO = [
  'Cliente solicitou',
  'Cliente ausente',
  'Problema no endereço',
  'Problema operacional',
  'Falta de produto',
  'Pagamento pendente',
  'Outro',
];

export interface SalvarStatusRequest {
  status: EntregaStatus;
}
export interface MotivoRequest {
  motivo: string;
}
export interface ReagendarRequest {
  novaData: string;
  motivo: string;
}
export interface AlterarAgendaRequest {
  novaData: string;
  frequenciaEntregaId: number | null;
  motivo: string;
}
