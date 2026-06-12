export type TipoCliente = 'Assinante' | 'Avulso';
export type FormaPagamento = 'Cartao' | 'Pix' | 'Dinheiro' | 'Outro';
export type StatusFinanceiro = 'EmDia' | 'Pendente' | 'Inadimplente';

export interface Cliente {
  id: number;
  nome: string;
  cpf?: string | null;
  telefone?: string | null;
  email?: string | null;
  origemVenda?: string | null;
  observacoes?: string | null;
  rua?: string | null;
  numero?: string | null;
  complemento?: string | null;
  cep?: string | null;
  bairro?: string | null;
  cidade?: string | null;
  estado?: string | null;
  frequenciaEntregaId?: number | null;
  primeiraEntrega?: string | null;
  ativo: boolean;
  motivoCancelamento?: string | null;
  dataCancelamento?: string | null;
  tipoCliente: TipoCliente;
  formaPagamento?: FormaPagamento | null;
  diaCobranca?: number | null;
  valorRecorrenteMensal: number;
  statusFinanceiro: StatusFinanceiro;
  observacoesFinanceiras?: string | null;
  pets?: string[];
}

export interface SalvarClienteRequest {
  nome: string;
  cpf?: string | null;
  telefone?: string | null;
  email?: string | null;
  origemVenda?: string | null;
  observacoes?: string | null;
  rua?: string | null;
  numero?: string | null;
  complemento?: string | null;
  cep?: string | null;
  bairro?: string | null;
  cidade?: string | null;
  estado?: string | null;
  frequenciaEntregaId?: number | null;
  primeiraEntrega?: string | null;
  tipoCliente: TipoCliente;
  formaPagamento?: FormaPagamento | null;
  diaCobranca?: number | null;
  valorRecorrenteMensal: number;
  statusFinanceiro: StatusFinanceiro;
  observacoesFinanceiras?: string | null;
}

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

export const ORIGENS_VENDA = [
  'Indicação',
  'Instagram',
  'Facebook',
  'Google',
  'WhatsApp',
  'Loja física',
  'Outro',
];

export const UFS = [
  'AC', 'AL', 'AP', 'AM', 'BA', 'CE', 'DF', 'ES', 'GO', 'MA', 'MT', 'MS', 'MG',
  'PA', 'PB', 'PR', 'PE', 'PI', 'RJ', 'RN', 'RS', 'RO', 'RR', 'SC', 'SP', 'SE', 'TO',
];

export function labelTipo(v: TipoCliente): string {
  return TIPOS_CLIENTE.find((t) => t.valor === v)?.label ?? v;
}

export function labelStatusFinanceiro(v: StatusFinanceiro): string {
  return STATUS_FINANCEIRO.find((s) => s.valor === v)?.label ?? v;
}

export function fmtCpf(cpf?: string | null): string {
  if (!cpf || cpf.length !== 11) {
    return cpf || '—';
  }
  return `${cpf.slice(0, 3)}.${cpf.slice(3, 6)}.${cpf.slice(6, 9)}-${cpf.slice(9)}`;
}

export function fmtMoeda(valor: number): string {
  return valor.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
}
