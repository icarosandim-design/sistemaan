export type TipoConversao = 'perda' | 'ganho' | 'sem_conversao';

export interface Ingrediente {
  id: number;
  nome: string;
  categoria: string;
  tipoConversao: TipoConversao;
  coeficiente: number; // rendimento = cozido ÷ cru
  custoKg: number; // custo informado manualmente nesta fase
  ativo: boolean;
}

export const CATEGORIAS: string[] = [
  'Proteína',
  'Carboidrato',
  'Vegetal',
  'Óleo',
  'Suplemento',
  'Tempero',
];

export const TIPOS_CONVERSAO: { valor: TipoConversao; label: string }[] = [
  { valor: 'perda', label: 'Perda' },
  { valor: 'ganho', label: 'Ganho' },
  { valor: 'sem_conversao', label: 'Sem conversão' },
];

export function labelTipoConversao(tipo: TipoConversao): string {
  return TIPOS_CONVERSAO.find((t) => t.valor === tipo)?.label ?? tipo;
}

/** Formata peso em g ou kg conforme magnitude (PT-BR, vírgula). */
export function fmtPeso(gramas: number): string {
  if (gramas >= 1000) {
    const kg = gramas / 1000;
    return `${kg.toLocaleString('pt-BR', { maximumFractionDigits: 2 })} kg`;
  }
  return `${gramas.toLocaleString('pt-BR', { maximumFractionDigits: 0 })} g`;
}

export function fmtMoeda(valor: number): string {
  return valor.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
}

/** Fator de correção em % (perda ou ganho de peso no preparo). */
export function fatorCorrecaoPct(tipo: TipoConversao, coef: number): number | null {
  if (tipo === 'perda') {
    return (1 - coef) * 100;
  }
  if (tipo === 'ganho') {
    return (coef - 1) * 100;
  }
  return null;
}

export function fmtFatorCorrecao(tipo: TipoConversao, coef: number): string {
  if (tipo === 'sem_conversao') {
    return '—';
  }
  const pct = fatorCorrecaoPct(tipo, coef) ?? 0;
  const sinal = tipo === 'perda' ? '−' : '+';
  return `${sinal}${pct.toLocaleString('pt-BR', { maximumFractionDigits: 0 })}%`;
}

/** Converte o fator de correção (%) no coeficiente de rendimento (cozido ÷ cru). */
export function coefDeFator(tipo: TipoConversao, pct: number): number {
  if (tipo === 'perda') {
    return 1 - pct / 100;
  }
  if (tipo === 'ganho') {
    return 1 + pct / 100;
  }
  return 1;
}

/** Custo real por kg do alimento pronto (cozido) = custo/kg ÷ rendimento. */
export function custoRealKg(custoKg: number, coef: number): number {
  return coef && coef > 0 ? custoKg / coef : custoKg;
}

/** Linhas de pré-visualização da conversão. */
export function previewConversao(tipo: TipoConversao, coef: number): string[] {
  if (tipo === 'sem_conversao' || !coef || coef <= 0) {
    return ['Sem conversão — o peso cru é igual ao peso cozido.'];
  }
  const cozidoDe1kgCru = 1000 * coef;
  const cruPara1kgCozido = 1000 / coef;
  return [
    `1 kg cru → ${fmtPeso(cozidoDe1kgCru)} cozido`,
    `${fmtPeso(cruPara1kgCozido)} cru → 1 kg cozido`,
  ];
}

export const MOCK_INGREDIENTES: Ingrediente[] = [
  { id: 1, nome: 'Batata-doce', categoria: 'Carboidrato', tipoConversao: 'perda', coeficiente: 0.55, custoKg: 6.5, ativo: true },
  { id: 2, nome: 'Arroz integral', categoria: 'Carboidrato', tipoConversao: 'ganho', coeficiente: 3.0, custoKg: 7.2, ativo: true },
  { id: 3, nome: 'Frango (peito)', categoria: 'Proteína', tipoConversao: 'perda', coeficiente: 0.7, custoKg: 18.9, ativo: true },
  { id: 4, nome: 'Carne bovina (patinho)', categoria: 'Proteína', tipoConversao: 'perda', coeficiente: 0.65, custoKg: 32.5, ativo: true },
  { id: 5, nome: 'Fígado bovino', categoria: 'Proteína', tipoConversao: 'perda', coeficiente: 0.72, custoKg: 19.0, ativo: true },
  { id: 6, nome: 'Abóbora', categoria: 'Vegetal', tipoConversao: 'perda', coeficiente: 0.8, custoKg: 4.8, ativo: true },
  { id: 7, nome: 'Cenoura', categoria: 'Vegetal', tipoConversao: 'perda', coeficiente: 0.88, custoKg: 5.5, ativo: true },
  { id: 8, nome: 'Aveia em flocos', categoria: 'Carboidrato', tipoConversao: 'ganho', coeficiente: 2.5, custoKg: 9.0, ativo: true },
  { id: 9, nome: 'Óleo de coco', categoria: 'Óleo', tipoConversao: 'sem_conversao', coeficiente: 1, custoKg: 39.9, ativo: true },
  { id: 10, nome: 'Sal', categoria: 'Tempero', tipoConversao: 'sem_conversao', coeficiente: 1, custoKg: 2.5, ativo: false },
  { id: 11, nome: 'Suplemento vitamínico', categoria: 'Suplemento', tipoConversao: 'sem_conversao', coeficiente: 1, custoKg: 120.0, ativo: true },
];
