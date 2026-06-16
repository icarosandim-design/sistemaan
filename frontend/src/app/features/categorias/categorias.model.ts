export type EscopoCategoria = 'Alimento' | 'Material' | 'Ambos';

export const ESCOPOS_CATEGORIA: { valor: EscopoCategoria; label: string }[] = [
  { valor: 'Alimento', label: 'Alimento' },
  { valor: 'Material', label: 'Material' },
  { valor: 'Ambos', label: 'Ambos' },
];

export function rotuloEscopo(v: string): string {
  return ESCOPOS_CATEGORIA.find((e) => e.valor === v)?.label ?? v;
}

export interface CategoriaIngrediente {
  id: number;
  nome: string;
  descricao: string | null;
  ordem: number;
  escopo: string;
  ativo: boolean;
}

export interface SalvarCategoriaIngredienteRequest {
  nome: string;
  descricao: string | null;
  ordem: number;
  escopo: string;
  ativo: boolean;
}
