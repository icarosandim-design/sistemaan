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
  /**
   * Resumo operacional do conteúdo da entrega (receitas da casa + personalizadas).
   * Para entregas reais é carregado sob demanda a partir do detalhe.
   * Estoque e prontidão são MOCK por enquanto (ver seção no fim do arquivo).
   */
  operacional?: ResumoOperacional;
}

/** Item de Receita da Casa dentro do resumo operacional da entrega. */
export interface ItemCasaOperacional {
  receitaNome: string;
  pacotes: number;
  tamanhoGramas: number;
  estoqueDisponivel: number; // ⚠️ MOCK
}

/** Item de Receita Personalizada dentro do resumo operacional da entrega. */
export interface ItemPersonalizadaOperacional {
  petNome: string;
  receitaCodigo: string;
  pacotes: number;
  tamanhoGramas: number;
  pronta: boolean; // ⚠️ MOCK
}

export interface ResumoOperacional {
  casa: ItemCasaOperacional[];
  personalizadas: ItemPersonalizadaOperacional[];
}

/** Linha do resumo do dia para Receitas da Casa (agregado por receita + tamanho). */
export interface ResumoCasaDia {
  receitaNome: string;
  tamanhoGramas: number;
  necessario: number;
  estoque: number; // ⚠️ MOCK
  falta: number;
}

export interface ResumoPersonalizadasDia {
  total: number;
  prontas: number;
  naoProntas: number;
  itens: ItemPersonalizadaOperacional[];
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
// Resumo operacional (mapeamento a partir do detalhe + agregações do dia)
// ---------------------------------------------------------------------
// O breakdown de receitas/pacotes/tamanho é REAL (vem do detalhe da
// entrega). Estoque e prontidão são MOCK por enquanto (ver funções abaixo),
// mas a estrutura já está pronta para integrar Estoque/Produção depois.
// =====================================================================

/** Constrói o resumo operacional a partir do detalhe da entrega. */
export function operacionalDeDetalhe(d: EntregaDetalhe): ResumoOperacional {
  const casaMap = new Map<string, ItemCasaOperacional>();
  const personalizadas: ItemPersonalizadaOperacional[] = [];

  for (const pet of d.pets) {
    for (const it of pet.itens) {
      if (it.tipo === 'Casa') {
        for (const pac of it.pacotes) {
          const key = `${it.receitaNome}|${pac.pesoGramas}`;
          const ex = casaMap.get(key);
          if (ex) {
            ex.pacotes += pac.quantidade;
          } else {
            casaMap.set(key, {
              receitaNome: it.receitaNome,
              tamanhoGramas: pac.pesoGramas,
              pacotes: pac.quantidade,
              estoqueDisponivel: estoqueMockPacotes(it.receitaNome, pac.pesoGramas),
            });
          }
        }
      } else {
        personalizadas.push({
          petNome: pet.petNome,
          receitaCodigo: it.receitaCodigo,
          pacotes: it.quantidadePacotes ?? 0,
          tamanhoGramas: it.tamanhoPacoteGramas ?? 0,
          pronta: prontaMock(`${d.id}-${it.receitaCodigo}`),
        });
      }
    }
  }

  return { casa: [...casaMap.values()], personalizadas };
}

/** Agrega as Receitas da Casa do dia por receita + tamanho. */
export function resumoCasaDoDia(entregas: EntregaResumo[]): ResumoCasaDia[] {
  const map = new Map<string, ResumoCasaDia>();
  for (const e of entregas) {
    for (const c of e.operacional?.casa ?? []) {
      const key = `${c.receitaNome}|${c.tamanhoGramas}`;
      const ex = map.get(key);
      if (ex) {
        ex.necessario += c.pacotes;
      } else {
        map.set(key, {
          receitaNome: c.receitaNome,
          tamanhoGramas: c.tamanhoGramas,
          necessario: c.pacotes,
          estoque: c.estoqueDisponivel,
          falta: 0,
        });
      }
    }
  }
  const lista = [...map.values()];
  for (const r of lista) {
    r.falta = Math.max(0, r.necessario - r.estoque);
  }
  return lista.sort((a, b) => a.receitaNome.localeCompare(b.receitaNome) || a.tamanhoGramas - b.tamanhoGramas);
}

/** Resumo de prontidão das personalizadas do dia. */
export function resumoPersonalizadasDoDia(entregas: EntregaResumo[]): ResumoPersonalizadasDia {
  const itens = entregas.flatMap((e) => e.operacional?.personalizadas ?? []);
  const prontas = itens.filter((i) => i.pronta).length;
  return { total: itens.length, prontas, naoProntas: itens.length - prontas, itens };
}

// =====================================================================
// ⚠️ ESTOQUE / PRONTIDÃO MOCK — substituir pelos módulos de Estoque/Produção
// ---------------------------------------------------------------------
// Valores determinísticos (estáveis entre renders) só para validar o
// layout operacional. Conectar ao backend real quando existir.
// =====================================================================

const ESTOQUE_MOCK: Record<string, number> = {
  'Frango|500': 20,
  'Carne|500': 5,
  'Porco|250': 8,
  'Frango|250': 12,
  'Carne|250': 3,
  'Peixe|500': 14,
};

function hash(s: string): number {
  let h = 0;
  for (let i = 0; i < s.length; i++) {
    h = (h * 31 + s.charCodeAt(i)) % 1000;
  }
  return h;
}

/** Estoque disponível (em pacotes) MOCK para uma receita+tamanho. */
export function estoqueMockPacotes(receitaNome: string, tamanhoGramas: number): number {
  const k = `${receitaNome}|${tamanhoGramas}`;
  if (k in ESTOQUE_MOCK) {
    return ESTOQUE_MOCK[k];
  }
  return 2 + (hash(k) % 18);
}

/** Prontidão MOCK de uma receita personalizada (≈ 60% prontas). */
export function prontaMock(seed: string): boolean {
  return hash(seed) % 5 < 3;
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
      const entregaId = id++;
      // Mix de tipos: a cada 3, uma entrega com Casa + Personalizada (ponto 4).
      const mista = i % 3 === 2;
      const soCasa = !mista && i % 2 === 0;
      const op = mockOperacional(entregaId, i, soCasa, mista);
      const pets = [
        ...op.casa.map((_, k) => MOCK_PETS[(i + k) % MOCK_PETS.length]),
        ...op.personalizadas.map((p) => p.petNome),
      ];
      const tipos = [op.casa.length ? 'Casa' : '', op.personalizadas.length ? 'Personalizada' : '']
        .filter(Boolean)
        .join(', ');
      lista.push({
        id: entregaId,
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
        tipos,
        totalGramas: 1400 + i * 350,
        totalPacotes: 2 + (i % 4),
        entregadorId: null,
        pets: [...new Set(pets)],
        // Algumas entregas marcadas como "fora da rota" para validar a sinalização.
        foraDaRota: i === 1,
        operacional: op,
      });
    }
  }
  return lista;
}

const MOCK_RECEITAS_CASA: { nome: string; tamanho: number }[] = [
  { nome: 'Frango', tamanho: 500 },
  { nome: 'Carne', tamanho: 500 },
  { nome: 'Porco', tamanho: 250 },
  { nome: 'Peixe', tamanho: 500 },
];
const MOCK_CODIGOS_PERS = ['VET-001', 'VET-002', 'VET-003'];

/** Monta um resumo operacional MOCK coerente para uma entrega hipotética. */
function mockOperacional(entregaId: number, i: number, soCasa: boolean, mista: boolean): ResumoOperacional {
  const casa: ItemCasaOperacional[] = [];
  const personalizadas: ItemPersonalizadaOperacional[] = [];

  if (soCasa || mista) {
    const qtdReceitas = (i % 2) + 1;
    for (let r = 0; r < qtdReceitas; r++) {
      const rec = MOCK_RECEITAS_CASA[(i + r) % MOCK_RECEITAS_CASA.length];
      const pacotes = 2 + ((i + r) % 5);
      casa.push({
        receitaNome: rec.nome,
        tamanhoGramas: rec.tamanho,
        pacotes,
        estoqueDisponivel: estoqueMockPacotes(rec.nome, rec.tamanho),
      });
    }
  }

  if (!soCasa || mista) {
    const codigo = MOCK_CODIGOS_PERS[i % MOCK_CODIGOS_PERS.length];
    const pet = MOCK_PETS[i % MOCK_PETS.length];
    personalizadas.push({
      petNome: pet,
      receitaCodigo: codigo,
      pacotes: 8 + (i % 10),
      tamanhoGramas: 750 + (i % 3) * 100,
      pronta: prontaMock(`${entregaId}-${codigo}`),
    });
  }

  return { casa, personalizadas };
}
