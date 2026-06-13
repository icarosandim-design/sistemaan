import { Injectable, computed, signal } from '@angular/core';
import {
  DemandaCasaMock,
  DemandaPersonalizadaMock,
  FichaMock,
  IngredienteConsolidadoMock,
  seedCasa,
  seedConsolidado,
  seedFichas,
  seedPersonalizadas,
  StatusFichaMock,
} from './producao.mock';

/**
 * ⚠️ PROTÓTIPO/MOCK: estado em memória do Módulo de Produção (somente frontend).
 * Mantém o estado entre as telas para a navegação do protótipo. Sem persistência real.
 */
@Injectable({ providedIn: 'root' })
export class ProducaoMockService {
  readonly personalizadas = signal<DemandaPersonalizadaMock[]>(seedPersonalizadas());
  readonly casa = signal<DemandaCasaMock[]>(seedCasa());
  readonly consolidado = signal<IngredienteConsolidadoMock[]>(seedConsolidado());
  readonly fichas = signal<FichaMock[]>(seedFichas());

  // Dia da produção em visualização (protótipo: hoje). Informativo — as telas não trocam o dia.
  readonly dataProducao = new Date().toLocaleDateString('pt-BR');

  // ----- Planejar -----
  /** Personalizadas ainda disponíveis (não planejadas em nenhum dia). */
  readonly disponiveis = computed(() => this.personalizadas().filter((p) => p.planejadaDia === null));

  /** Produções já planejadas, agrupadas por dia. */
  readonly producoesPlanejadas = computed(() => {
    const map = new Map<string, DemandaPersonalizadaMock[]>();
    for (const p of this.personalizadas()) {
      if (p.planejadaDia) {
        const lista = map.get(p.planejadaDia) ?? [];
        lista.push(p);
        map.set(p.planejadaDia, lista);
      }
    }
    return [...map.entries()]
      .map(([dia, itens]) => ({ dia, itens }))
      .sort((a, b) => a.dia.localeCompare(b.dia));
  });

  /** Planeja as personalizadas selecionadas (e ainda disponíveis) para um dia. */
  planejarSelecionadasPara(dia: string): number {
    let qtd = 0;
    this.personalizadas.update((xs) =>
      xs.map((x) => {
        if (x.planejadaDia === null && x.selecionada) {
          qtd++;
          return { ...x, planejadaDia: dia, selecionada: false };
        }
        return x;
      }),
    );
    return qtd;
  }

  /** Remove um pet da produção planejada → volta para disponível (não pronta). */
  removerDaProducao(id: number): void {
    this.personalizadas.update((xs) => xs.map((x) => (x.id === id ? { ...x, planejadaDia: null, selecionada: false } : x)));
  }

  /** Adiciona um pet disponível a uma produção planejada de um dia → planejado. */
  adicionarNaProducao(id: number, dia: string): void {
    this.personalizadas.update((xs) => xs.map((x) => (x.id === id ? { ...x, planejadaDia: dia, selecionada: false } : x)));
  }

  alternarPersonalizada(id: number): void {
    this.personalizadas.update((xs) => xs.map((x) => (x.id === id ? { ...x, selecionada: !x.selecionada } : x)));
  }

  alternarCasa(id: number): void {
    this.casa.update((xs) => xs.map((x) => (x.id === id ? { ...x, incluir: !x.incluir } : x)));
  }

  definirQtdCasa(id: number, qtd: number): void {
    this.casa.update((xs) => xs.map((x) => (x.id === id ? { ...x, qtdProduzir: qtd } : x)));
  }

  readonly totalSelecionadas = computed(
    () => this.personalizadas().filter((p) => p.selecionada).length + this.casa().filter((c) => c.incluir).length,
  );

  // ----- Fichas / cozinha -----
  readonly fichasPendentes = computed(() => this.fichas().filter((f) => f.status !== 'Concluida' && f.status !== 'NaoFeita'));
  readonly fichasConcluidas = computed(() => this.fichas().filter((f) => f.status === 'Concluida'));
  readonly fichasNaoFeitas = computed(() => this.fichas().filter((f) => f.status === 'NaoFeita'));

  mudarStatusFicha(id: number, status: StatusFichaMock): void {
    this.fichas.update((xs) => xs.map((f) => (f.id === id ? { ...f, status } : f)));
  }

  // Avança para o próximo status na fila da cozinha.
  avancarFicha(id: number): void {
    const ordem: StatusFichaMock[] = ['Pendente', 'EmProducao', 'Envasando', 'Concluida'];
    this.fichas.update((xs) =>
      xs.map((f) => {
        if (f.id !== id) {
          return f;
        }
        const i = ordem.indexOf(f.status);
        const prox = i >= 0 && i < ordem.length - 1 ? ordem[i + 1] : f.status;
        return { ...f, status: prox };
      }),
    );
  }

  faltaConsolidado(i: IngredienteConsolidadoMock): number {
    return Math.max(0, i.cruGramas - i.estoqueGramas);
  }

  readonly alertasEstoque = computed(() => this.consolidado().filter((i) => this.faltaConsolidado(i) > 0).length);
}
