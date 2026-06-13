// =====================================================================
// ⚠️ PROTÓTIPO / MOCK — Módulo de Produção (somente frontend) ⚠️
// ---------------------------------------------------------------------
// NÃO usa backend, endpoints, banco nem APIs reais. Dados fictícios apenas
// para validar o fluxo visual com o operador e a cozinha.
// Remover/substituir quando o backend real do módulo for implementado.
// =====================================================================

export type ProntidaoMock = 'NaoPronta' | 'ParcialmentePronta' | 'Pronta';
export type StatusFichaMock = 'Pendente' | 'EmProducao' | 'Envasando' | 'Concluida' | 'NaoFeita';

export interface IngredienteFichaMock {
  nome: string;
  gramas: number;
}

export interface FichaMock {
  id: number;
  tipo: 'Casa' | 'Personalizada';
  cliente: string | null;
  pet: string | null;
  receitaCodigo: string;
  receitaNome: string;
  dataEntrega: string;
  totalBaciaGramas: number;
  pacotes: number;
  pesoPacoteGramas: number;
  prontidao: ProntidaoMock | null;
  ingredientes: IngredienteFichaMock[];
  observacoes: string | null;
  status: StatusFichaMock;
}

export interface DemandaPersonalizadaMock {
  id: number;
  pet: string;
  cliente: string;
  receitaCodigo: string;
  dataEntrega: string;
  diasParaEntrega: number;
  pacotes: number;
  pesoPacoteGramas: number;
  prontidao: ProntidaoMock;
  selecionada: boolean;
  /** Dia da produção em que foi planejada (null = ainda disponível / não planejada). */
  planejadaDia: string | null;
}

export interface DemandaCasaMock {
  id: number;
  receitaNome: string;
  tamanho: string;
  necessario: number;
  estoque: number;
  incluir: boolean;
  qtdProduzir: number;
}

export interface IngredienteConsolidadoMock {
  nome: string;
  categoria: string;
  cozidoGramas: number;
  cruGramas: number;
  estoqueGramas: number;
}

// ===== Formatação =====
export function fmtPeso(gramas: number): string {
  if (Math.abs(gramas) >= 1000) {
    return `${(gramas / 1000).toLocaleString('pt-BR', { maximumFractionDigits: 2 })} kg`;
  }
  return `${gramas.toLocaleString('pt-BR', { maximumFractionDigits: 0 })} g`;
}

export function rotuloProntidao(p: ProntidaoMock | null): string {
  switch (p) {
    case 'Pronta': return 'Pronta';
    case 'ParcialmentePronta': return 'Parcialmente pronta';
    case 'NaoPronta': return 'Não pronta';
    default: return '—';
  }
}

export function rotuloStatusFicha(s: StatusFichaMock): string {
  switch (s) {
    case 'Pendente': return 'A fazer';
    case 'EmProducao': return 'Em produção';
    case 'Envasando': return 'Envasando';
    case 'Concluida': return 'Concluída';
    case 'NaoFeita': return 'Não feita';
  }
}

// ===== Seeds (dados fictícios) =====
export function seedPersonalizadas(): DemandaPersonalizadaMock[] {
  return [
    { id: 1, pet: 'Scooby', cliente: 'Ana Beatriz Souza', receitaCodigo: 'VET-001', dataEntrega: 'amanhã', diasParaEntrega: 1, pacotes: 5, pesoPacoteGramas: 200, prontidao: 'NaoPronta', selecionada: true, planejadaDia: null },
    { id: 2, pet: 'Thor', cliente: 'Carlos Eduardo Lima', receitaCodigo: 'VET-002', dataEntrega: 'amanhã', diasParaEntrega: 1, pacotes: 10, pesoPacoteGramas: 150, prontidao: 'NaoPronta', selecionada: true, planejadaDia: null },
    { id: 3, pet: 'Mel', cliente: 'Mariana Castro', receitaCodigo: 'VET-004', dataEntrega: 'amanhã', diasParaEntrega: 1, pacotes: 4, pesoPacoteGramas: 250, prontidao: 'NaoPronta', selecionada: true, planejadaDia: null },
    { id: 4, pet: 'Bidu', cliente: 'Rafael Antunes', receitaCodigo: 'VET-005', dataEntrega: 'amanhã', diasParaEntrega: 1, pacotes: 6, pesoPacoteGramas: 200, prontidao: 'NaoPronta', selecionada: true, planejadaDia: null },
    { id: 5, pet: 'Luna', cliente: 'Juliana Prado', receitaCodigo: 'VET-006', dataEntrega: 'amanhã', diasParaEntrega: 1, pacotes: 8, pesoPacoteGramas: 180, prontidao: 'NaoPronta', selecionada: false, planejadaDia: null },
    { id: 6, pet: 'Nina', cliente: 'Felipe Moraes', receitaCodigo: 'VET-003', dataEntrega: 'em 2 dias', diasParaEntrega: 2, pacotes: 7, pesoPacoteGramas: 300, prontidao: 'ParcialmentePronta', selecionada: true, planejadaDia: null },
    // já planejadas (demonstram a área de "Produções planejadas")
    { id: 7, pet: 'Zeus', cliente: 'Patrícia Nogueira', receitaCodigo: 'VET-007', dataEntrega: 'em 3 dias', diasParaEntrega: 3, pacotes: 5, pesoPacoteGramas: 250, prontidao: 'NaoPronta', selecionada: false, planejadaDia: '16/06/2026' },
    { id: 8, pet: 'Amora', cliente: 'Bruno Carvalho', receitaCodigo: 'VET-008', dataEntrega: 'em 5 dias', diasParaEntrega: 5, pacotes: 6, pesoPacoteGramas: 200, prontidao: 'NaoPronta', selecionada: false, planejadaDia: '16/06/2026' },
  ];
}

export function seedCasa(): DemandaCasaMock[] {
  return [
    { id: 101, receitaNome: 'Frango', tamanho: '500g', necessario: 30, estoque: 100, incluir: false, qtdProduzir: 0 },
    { id: 102, receitaNome: 'Carne bovina', tamanho: '500g', necessario: 80, estoque: 20, incluir: true, qtdProduzir: 60 },
    { id: 103, receitaNome: 'Suína', tamanho: '500g', necessario: 30, estoque: 100, incluir: false, qtdProduzir: 0 },
    { id: 104, receitaNome: 'Carne bovina', tamanho: '250g', necessario: 40, estoque: 12, incluir: true, qtdProduzir: 28 },
  ];
}

export function seedConsolidado(): IngredienteConsolidadoMock[] {
  // inclui ingredientes repetidos entre fichas (já somados) e faltas (Bovina, Abóbora).
  return [
    { nome: 'Frango', categoria: 'Proteínas', cozidoGramas: 1000, cruGramas: 1800, estoqueGramas: 20000 },
    { nome: 'Carne bovina', categoria: 'Proteínas', cozidoGramas: 3000, cruGramas: 6000, estoqueGramas: 4000 },
    { nome: 'Suína', categoria: 'Proteínas', cozidoGramas: 500, cruGramas: 5000, estoqueGramas: 8000 },
    { nome: 'Fígado bovino', categoria: 'Proteínas', cozidoGramas: 300, cruGramas: 500, estoqueGramas: 2000 },
    { nome: 'Arroz integral', categoria: 'Carboidratos', cozidoGramas: 600, cruGramas: 1500, estoqueGramas: 5000 },
    { nome: 'Batata doce', categoria: 'Carboidratos', cozidoGramas: 700, cruGramas: 1260, estoqueGramas: 800 },
    { nome: 'Abóbora', categoria: 'Legumes', cozidoGramas: 800, cruGramas: 3000, estoqueGramas: 1000 },
    { nome: 'Cenoura', categoria: 'Legumes', cozidoGramas: 400, cruGramas: 2000, estoqueGramas: 3000 },
    { nome: 'Brócolis', categoria: 'Legumes', cozidoGramas: 250, cruGramas: 600, estoqueGramas: 1500 },
    { nome: 'Cúrcuma', categoria: 'Temperos', cozidoGramas: 20, cruGramas: 20, estoqueGramas: 500 },
    { nome: 'Salsinha', categoria: 'Temperos', cozidoGramas: 30, cruGramas: 40, estoqueGramas: 200 },
    { nome: 'Óleo de girassol', categoria: 'Óleos', cozidoGramas: 150, cruGramas: 150, estoqueGramas: 4000 },
    { nome: 'Óleo de peixe', categoria: 'Óleos', cozidoGramas: 80, cruGramas: 80, estoqueGramas: 60 },
    { nome: 'Ômega 3', categoria: 'Suplementos', cozidoGramas: 40, cruGramas: 40, estoqueGramas: 1000 },
    { nome: 'Cálcio', categoria: 'Suplementos', cozidoGramas: 25, cruGramas: 25, estoqueGramas: 800 },
  ];
}

export function seedFichas(): FichaMock[] {
  return [
    {
      id: 1, tipo: 'Personalizada', cliente: 'Ana Beatriz Souza', pet: 'Scooby', receitaCodigo: 'VET-001', receitaNome: 'Personalizada Scooby',
      dataEntrega: 'amanhã', totalBaciaGramas: 1000, pacotes: 5, pesoPacoteGramas: 200, prontidao: 'NaoPronta',
      ingredientes: [{ nome: 'Frango', gramas: 400 }, { nome: 'Arroz integral', gramas: 200 }, { nome: 'Abóbora', gramas: 200 }, { nome: 'Cenoura', gramas: 200 }],
      observacoes: 'Sem frango com osso.', status: 'Pendente',
    },
    {
      id: 2, tipo: 'Personalizada', cliente: 'Carlos Eduardo Lima', pet: 'Thor', receitaCodigo: 'VET-002', receitaNome: 'Personalizada Thor',
      dataEntrega: 'amanhã', totalBaciaGramas: 1500, pacotes: 10, pesoPacoteGramas: 150, prontidao: 'NaoPronta',
      ingredientes: [{ nome: 'Carne bovina', gramas: 800 }, { nome: 'Arroz integral', gramas: 400 }, { nome: 'Cenoura', gramas: 300 }],
      observacoes: null, status: 'EmProducao',
    },
    {
      id: 3, tipo: 'Personalizada', cliente: 'Felipe Moraes', pet: 'Nina', receitaCodigo: 'VET-003', receitaNome: 'Personalizada Nina',
      dataEntrega: 'em 2 dias', totalBaciaGramas: 2100, pacotes: 7, pesoPacoteGramas: 300, prontidao: 'ParcialmentePronta',
      ingredientes: [{ nome: 'Carne bovina', gramas: 1200 }, { nome: 'Abóbora', gramas: 600 }, { nome: 'Cenoura', gramas: 300 }],
      observacoes: 'Alergia a frango.', status: 'Envasando',
    },
    {
      id: 4, tipo: 'Personalizada', cliente: 'Mariana Castro', pet: 'Mel', receitaCodigo: 'VET-004', receitaNome: 'Personalizada Mel',
      dataEntrega: 'amanhã', totalBaciaGramas: 1000, pacotes: 4, pesoPacoteGramas: 250, prontidao: 'NaoPronta',
      ingredientes: [{ nome: 'Suína', gramas: 500 }, { nome: 'Abóbora', gramas: 300 }, { nome: 'Arroz integral', gramas: 200 }],
      observacoes: null, status: 'Pendente',
    },
    {
      id: 5, tipo: 'Personalizada', cliente: 'Rafael Antunes', pet: 'Bidu', receitaCodigo: 'VET-005', receitaNome: 'Personalizada Bidu',
      dataEntrega: 'amanhã', totalBaciaGramas: 1200, pacotes: 6, pesoPacoteGramas: 200, prontidao: 'Pronta',
      ingredientes: [{ nome: 'Frango', gramas: 600 }, { nome: 'Cenoura', gramas: 300 }, { nome: 'Arroz integral', gramas: 300 }],
      observacoes: null, status: 'Concluida',
    },
    {
      id: 6, tipo: 'Casa', cliente: null, pet: null, receitaCodigo: 'CASA-CARNE', receitaNome: 'Carne bovina 500g',
      dataEntrega: 'estoque', totalBaciaGramas: 30000, pacotes: 60, pesoPacoteGramas: 500, prontidao: null,
      ingredientes: [{ nome: 'Carne bovina', gramas: 18000 }, { nome: 'Arroz integral', gramas: 7000 }, { nome: 'Abóbora', gramas: 5000 }],
      observacoes: 'Reforço de estoque (falta para a semana).', status: 'Pendente',
    },
  ];
}
