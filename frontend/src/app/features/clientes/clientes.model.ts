export type TipoCliente = 'Assinante' | 'Avulso';
export type FormaPagamento = 'Cartao' | 'Pix' | 'Dinheiro' | 'Outro';
export type StatusFinanceiro = 'EmDia' | 'Pendente' | 'Inadimplente';

export interface Cliente {
  id: number;
  nome: string;
  telefone?: string | null;
  email?: string | null;
  endereco?: string | null;
  bairro?: string | null;
  cidade?: string | null;
  observacoes?: string | null;
  ativo: boolean;
  tipoCliente: TipoCliente;
  formaPagamento?: FormaPagamento | null;
  diaCobranca?: number | null;
  valorRecorrenteMensal: number;
  statusFinanceiro: StatusFinanceiro;
  observacoesFinanceiras?: string | null;
}

export type SalvarClienteRequest = Omit<Cliente, 'id'>;

export const TIPOS_CLIENTE: { valor: TipoCliente; label: string }[] = [
  { valor: 'Assinante', label: 'Assinante' },
  { valor: 'Avulso', label: 'Avulso' },
];

export const FORMAS_PAGAMENTO: { valor: FormaPagamento; label: string }[] = [
  { valor: 'Cartao', label: 'Cartão' },
  { valor: 'Pix', label: 'Pix' },
  { valor: 'Dinheiro', label: 'Dinheiro' },
  { valor: 'Outro', label: 'Outro' },
];

export const STATUS_FINANCEIRO: { valor: StatusFinanceiro; label: string }[] = [
  { valor: 'EmDia', label: 'Em dia' },
  { valor: 'Pendente', label: 'Pendente' },
  { valor: 'Inadimplente', label: 'Inadimplente' },
];

export function labelTipo(v: TipoCliente): string {
  return TIPOS_CLIENTE.find((t) => t.valor === v)?.label ?? v;
}

export function labelStatusFinanceiro(v: StatusFinanceiro): string {
  return STATUS_FINANCEIRO.find((s) => s.valor === v)?.label ?? v;
}

export function fmtMoeda(valor: number): string {
  return valor.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
}
