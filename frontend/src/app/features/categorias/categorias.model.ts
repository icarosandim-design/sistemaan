export interface CategoriaIngrediente {
  id: number;
  nome: string;
  descricao: string | null;
  ordem: number;
  ativo: boolean;
}

export interface SalvarCategoriaIngredienteRequest {
  nome: string;
  descricao: string | null;
  ordem: number;
  ativo: boolean;
}
