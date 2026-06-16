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
  /** Quando preenchido, a entrega veio de um Pedido PJ. */
  pedidoId?: number | null;
  preferenciaHorario?: string;
  pets: string[];
  /**
   * Sinalização visual (preparatória para o Módulo de Rotas).
   * `true` quando a entrega ainda não foi incluída em nenhuma rota planejada do dia.
   * Por enquanto não vem do backend; é derivada/simulada no frontend.
   */
  foraDaRota?: boolean;
  /**
   * Resumo operacional do conteúdo da entrega (receitas da casa + personalizadas).
   * Carregado sob demanda a partir do detalhe real da entrega; o estoque vem do
   * módulo de Estoque (produto acabado) e a prontidão do status de preparo real.
   */
  operacional?: ResumoOperacional;
}

/** Item de Receita da Casa dentro do resumo operacional da entrega. */
export interface ItemCasaOperacional {
  receitaId: number;
  receitaNome: string;
  pacotes: number;
  tamanhoGramas: number;
  estoqueDisponivel: number; // saldo real de produto acabado (pacotes)
}

/** Item de Receita Personalizada dentro do resumo operacional da entrega. */
export interface ItemPersonalizadaOperacional {
  petNome: string;
  receitaCodigo: string;
  pacotes: number;
  tamanhoGramas: number;
  pronta: boolean; // prontidão real (preenchida pela Produção)
  parcial?: boolean;
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
  estoque: number; // saldo real de produto acabado (módulo de Estoque)
  falta: number;
}

export interface ResumoPersonalizadasDia {
  total: number;
  prontas: number;
  naoProntas: number;
  itens: ItemPersonalizadaOperacional[];
}

/** Prontidão consolidada de uma entrega (para o cartão minimizado). */
export interface ProntidaoEntrega {
  temConteudo: boolean;
  tudoPronto: boolean;
  casaFalta: number; // receitas da casa sem estoque suficiente
  persNaoProntas: number; // personalizadas ainda não prontas
}

/** Avalia se a entrega está pronta para separar (estoque + prontidão). */
export function prontidaoEntrega(op?: ResumoOperacional): ProntidaoEntrega {
  const casa = op?.casa ?? [];
  const pers = op?.personalizadas ?? [];
  const casaFalta = casa.filter((c) => c.pacotes > c.estoqueDisponivel).length;
  const persNaoProntas = pers.filter((p) => !p.pronta).length;
  return {
    temConteudo: casa.length > 0 || pers.length > 0,
    tudoPronto: casaFalta === 0 && persNaoProntas === 0,
    casaFalta,
    persNaoProntas,
  };
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

/** Situação de estoque de Produto Acabado da Casa (físico/comprometido/disponível/falta). */
export interface SituacaoEstoqueItem {
  receitaId: number;
  receitaNome: string;
  pesoGramas: number;
  tamanhoNome: string;
  necessario: number;
  fisico: number;
  comprometido: number;
  disponivel: number;
  falta: number;
  temFalta: boolean;
  semItemEstoque: boolean;
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
  statusPreparo: string;
  pacotesProntos: number | null;
  pacotes: EntregaItemPacote[];
  ingredientes: EntregaItemIngrediente[];
}

export interface EntregaPet {
  id: number;
  petId: number | null;
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
  pedidoId?: number | null;
  preferenciaHorario?: string;
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
// entrega). O estoque vem do módulo de Estoque (produto acabado) e a
// prontidão das personalizadas vem do status de preparo real.
// =====================================================================

/**
 * Constrói o resumo operacional a partir do detalhe da entrega.
 * `estoqueProdutoAcabado`: mapa `${receitaId}-${pesoGramas}` → saldo real (pacotes).
 */
export function operacionalDeDetalhe(d: EntregaDetalhe, estoqueProdutoAcabado?: Map<string, number>): ResumoOperacional {
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
              receitaId: it.receitaId,
              receitaNome: it.receitaNome,
              tamanhoGramas: pac.pesoGramas,
              pacotes: pac.quantidade,
              estoqueDisponivel: estoqueProdutoAcabado?.get(`${it.receitaId}-${pac.pesoGramas}`) ?? 0,
            });
          }
        }
      } else {
        personalizadas.push({
          petNome: pet.petNome,
          receitaCodigo: it.receitaCodigo,
          pacotes: it.quantidadePacotes ?? 0,
          tamanhoGramas: it.tamanhoPacoteGramas ?? 0,
          pronta: it.statusPreparo === 'Pronta',
          parcial: it.statusPreparo === 'ParcialmentePronta',
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
