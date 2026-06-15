export type Sexo = 'Macho' | 'Femea';

/** Espelha o PetDto retornado pela API. */
export interface Pet {
  id: number;
  clienteId: number;
  nome: string;
  raca: string | null;
  pesoKg: number;
  dataNascimento: string | null; // aaaa-mm-dd
  idadeAprox: string | null;
  sexo: Sexo | null;
  ativo: boolean;
  observacoesGerais: string | null;
  observacoesAlimentares: string | null;
  gramasDiaAjustadas: number | null;
  gramasDiaSugeridas: number | null; // calculado pela Tabela de Consumo (backend)
}

/** Linha da visão geral de Pets (PetResumoDto no backend). */
export interface PetResumo {
  id: number;
  clienteId: number;
  nome: string;
  tutorNome: string;
  raca: string | null;
  pesoKg: number;
  sexo: Sexo | null;
  ativo: boolean;
  tipoAlimentacao: string | null;
  receitaAtual: string | null;
  proximaEntrega: string | null;
}

/** Payload de criação/edição (SalvarPetRequest no backend). */
export interface SalvarPetRequest {
  nome: string;
  raca: string | null;
  pesoKg: number;
  dataNascimento: string | null;
  idadeAprox: string | null;
  sexo: Sexo | null;
  observacoesGerais: string | null;
  observacoesAlimentares: string | null;
  gramasDiaAjustadas: number | null;
}

export const SEXOS: { valor: Sexo; label: string }[] = [
  { valor: 'Macho', label: 'Macho' },
  { valor: 'Femea', label: 'Fêmea' },
];

export function labelSexo(s: Sexo | null): string {
  return s === 'Macho' ? 'Macho' : s === 'Femea' ? 'Fêmea' : '—';
}
