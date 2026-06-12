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
  rua: string | null;
  numero: string | null;
  complemento: string | null;
  dataPrevista: string; // aaaa-mm-dd
  status: EntregaStatus;
  bairro: string | null;
  cidade: string | null;
  tipos: string;
  totalGramas: number;
  totalPacotes: number;
  entregadorId: number | null;
  pets: string[];
  /**
   * Sinalização visual (preparatória para o Módulo de Rotas).
   * `true` quando a entrega ainda não foi incluída em nenhuma rota planejada do dia.
   * Por enquanto não vem do backend; é derivada/simulada no frontend.
   */
  foraDaRota?: boolean;
}

/**
 * Endereço resumido para a lista do dia, SEM estado.
 * Ex.: "Morro Dois Irmãos, 139 — casa"
 */
export function enderecoResumo(e: { rua: string | null; numero: string | null; complemento: string | null }): string {
  const partes: string[] = [];
  if (e.rua) {
    partes.push(e.numero ? `${e.rua}, ${e.numero}` : e.rua);
  } else if (e.numero) {
    partes.push(e.numero);
  }
  let texto = partes.join('');
  if (e.complemento) {
    texto = texto ? `${texto} — ${e.complemento}` : e.complemento;
  }
  return texto || '—';
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

// =====================================================================
// ⚠️ DADOS MOCK TEMPORÁRIOS — APENAS PARA VALIDAÇÃO VISUAL DO LAYOUT ⚠️
// ---------------------------------------------------------------------
// Estes dados NÃO vêm do backend. São usados somente quando ainda não há
// entregas reais suficientes para validar a tela (calendário, lista do
// dia, ações do dia, sinalização "fora da rota").
// REMOVER quando o fluxo real de geração de entregas estiver populado.
// =====================================================================

const MOCK_BAIRROS: { bairro: string; cidade: string; rua: string }[] = [
  { bairro: 'Lagoa da Conceição', cidade: 'Florianópolis', rua: 'Rua das Rendeiras' },
  { bairro: 'Morro Dois Irmãos', cidade: 'Florianópolis', rua: 'Servidão do Mirante' },
  { bairro: 'Centro', cidade: 'Florianópolis', rua: 'Rua Felipe Schmidt' },
  { bairro: 'Trindade', cidade: 'Florianópolis', rua: 'Rua Lauro Linhares' },
  { bairro: 'Campeche', cidade: 'Florianópolis', rua: 'Av. Pequeno Príncipe' },
  { bairro: 'Santa Mônica', cidade: 'Florianópolis', rua: 'Rua João Pio Duarte' },
  { bairro: 'Itacorubi', cidade: 'Florianópolis', rua: 'Rua João Câmara' },
];

const MOCK_CLIENTES = [
  'Ana Beatriz Souza', 'Carlos Eduardo Lima', 'Mariana Castro', 'Rafael Antunes',
  'Juliana Prado', 'Felipe Moraes', 'Patrícia Nogueira', 'Bruno Carvalho',
  'Larissa Fontes', 'Thiago Ramos', 'Camila Dias', 'Eduardo Bittencourt',
];

const MOCK_PETS = ['Thor', 'Luna', 'Bidu', 'Mel', 'Nina', 'Bob', 'Cacau', 'Amora', 'Zeus', 'Frida'];
const MOCK_STATUS: EntregaStatus[] = ['Programada', 'ConfirmadaCliente', 'NaoEntregue', 'Reagendada', 'SaiuParaEntrega'];

function isoSomaDias(base: Date, dias: number): string {
  const d = new Date(base.getFullYear(), base.getMonth(), base.getDate() + dias);
  const p = (n: number) => String(n).padStart(2, '0');
  return `${d.getFullYear()}-${p(d.getMonth() + 1)}-${p(d.getDate())}`;
}

/**
 * Gera entregas hipotéticas em 3 dias distintos (5, 6 e 9 entregas)
 * a partir de hoje, para validar o layout. TEMPORÁRIO.
 */
export function gerarEntregasMock(): EntregaResumo[] {
  const hoje = new Date();
  const plano: { offset: number; qtd: number }[] = [
    { offset: 0, qtd: 5 },
    { offset: 2, qtd: 6 },
    { offset: 5, qtd: 9 },
  ];
  const lista: EntregaResumo[] = [];
  let id = 90001; // faixa alta p/ não colidir com ids reais
  for (const { offset, qtd } of plano) {
    const data = isoSomaDias(hoje, offset);
    for (let i = 0; i < qtd; i++) {
      const loc = MOCK_BAIRROS[(i + offset) % MOCK_BAIRROS.length];
      const status = MOCK_STATUS[i % MOCK_STATUS.length];
      const npets = (i % 2) + 1;
      const pets = Array.from({ length: npets }, (_, k) => MOCK_PETS[(i + k) % MOCK_PETS.length]);
      lista.push({
        id: id++,
        clienteId: 0,
        clienteNome: MOCK_CLIENTES[(i + offset) % MOCK_CLIENTES.length],
        telefone: '(48) 99999-0000',
        rua: loc.rua,
        numero: String(100 + i * 7),
        complemento: i % 3 === 0 ? 'casa' : i % 3 === 1 ? `apto ${i + 1}0${i + 1}` : 'fundos',
        dataPrevista: data,
        status,
        bairro: loc.bairro,
        cidade: loc.cidade,
        tipos: i % 2 === 0 ? 'Casa' : 'Personalizada',
        totalGramas: 1400 + i * 350,
        totalPacotes: 2 + (i % 4),
        entregadorId: null,
        pets,
        // Algumas entregas marcadas como "fora da rota" para validar a sinalização.
        foraDaRota: i === 1,
      });
    }
  }
  return lista;
}
