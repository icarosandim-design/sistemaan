export type TipoProduto = 'ReceitaDaCasa' | 'Petisco' | 'ProdutoComprado' | 'Brinde' | 'Outro';

export const TIPOS_PRODUTO: { valor: TipoProduto; label: string }[] = [
  { valor: 'ReceitaDaCasa', label: 'Receita da Casa' },
  { valor: 'Petisco', label: 'Petisco' },
  { valor: 'ProdutoComprado', label: 'Produto comprado' },
  { valor: 'Brinde', label: 'Brinde' },
  { valor: 'Outro', label: 'Outro' },
];

export const UNIDADES_PRODUTO = ['Unidade', 'Pacote', 'Kg', 'G', 'Caixa', 'Litro', 'Ml', 'Outro'];

export interface Produto {
  id: number;
  nome: string;
  codigo: string | null;
  tipo: TipoProduto;
  receitaCasaId: number | null;
  receitaCasaNome: string | null;
  tamanhoPacoteId: number | null;
  tamanhoPacoteNome: string | null;
  unidadeMedida: string;
  pesoGramas: number | null;
  precoVendaAvulsaPF: number;
  precoVendaPJ: number;
  controlaEstoque: boolean;
  produzidoInternamente: boolean;
  ativo: boolean;
  observacoes: string | null;
}

export interface SalvarProdutoRequest {
  nome: string;
  codigo: string | null;
  tipo: TipoProduto;
  receitaCasaId: number | null;
  tamanhoPacoteId: number | null;
  unidadeMedida: string | null;
  pesoGramas: number | null;
  precoVendaAvulsaPF: number;
  precoVendaPJ: number;
  controlaEstoque: boolean;
  produzidoInternamente: boolean;
  ativo: boolean;
  observacoes: string | null;
}

export function labelTipoProduto(t: TipoProduto): string {
  return TIPOS_PRODUTO.find((x) => x.valor === t)?.label ?? t;
}
