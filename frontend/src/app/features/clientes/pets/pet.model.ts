export type Sexo = 'Macho' | 'Femea';

export interface Pet {
  id: number;
  nome: string;
  raca: string;
  pesoKg: number;
  dataNascimento: string | null; // aaaa-mm-dd
  idadeAprox: string | null;
  sexo: Sexo | null;
  ativo: boolean;
  observacoesGerais: string;
  observacoesAlimentares: string;
  gramasDiaAjustadas: number | null;
}

export const SEXOS: { valor: Sexo; label: string }[] = [
  { valor: 'Macho', label: 'Macho' },
  { valor: 'Femea', label: 'Fêmea' },
];

export function labelSexo(s: Sexo | null): string {
  return s === 'Macho' ? 'Macho' : s === 'Femea' ? 'Fêmea' : '—';
}

/**
 * MOCK da Tabela de Consumo: sugere gramas/dia pelo peso.
 * Futuramente virá de GET /api/faixas-consumo/aplica?peso=.
 */
export function sugestaoGramasDia(pesoKg: number): number | null {
  const faixas = [
    { ini: 3, fim: 5, g: 200 },
    { ini: 6, fim: 8, g: 290 },
    { ini: 9, fim: 11, g: 370 },
    { ini: 12, fim: 14, g: 440 },
  ];
  const f = faixas.find((x) => pesoKg >= x.ini && pesoKg <= x.fim);
  return f ? f.g : null;
}

/** Pets de exemplo (mock) para validação visual — não são persistidos. */
export const MOCK_PETS: Pet[] = [
  {
    id: 1,
    nome: 'Thor',
    raca: 'Golden Retriever',
    pesoKg: 12,
    dataNascimento: '2021-03-10',
    idadeAprox: null,
    sexo: 'Macho',
    ativo: true,
    observacoesGerais: 'Dócil, ansioso em dias de chuva.',
    observacoesAlimentares: 'Sensível a frango com pele.',
    gramasDiaAjustadas: 400,
  },
  {
    id: 2,
    nome: 'Luna',
    raca: 'Vira-lata',
    pesoKg: 7,
    dataNascimento: null,
    idadeAprox: '3 anos',
    sexo: 'Femea',
    ativo: true,
    observacoesGerais: '',
    observacoesAlimentares: '',
    gramasDiaAjustadas: null,
  },
];

/**
 * MOCK: nomes dos pets de um cliente, só para exibição na listagem nesta fase.
 * Determinístico pelo id do cliente. Será substituído pela relação real
 * Cliente → Pets quando o backend de Pets existir.
 */
export function petsMockDoCliente(clienteId: number): string[] {
  const pools = [
    ['Thor', 'Luna', 'Mel'],
    ['Bidu'],
    ['Nina', 'Rex'],
    ['Amora'],
    [],
  ];
  return pools[clienteId % pools.length];
}
