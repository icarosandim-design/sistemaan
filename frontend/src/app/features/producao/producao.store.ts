import { Injectable, computed, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { ProducaoService } from './producao.service';
import {
  ConcluirFichaRequest,
  ConsumoRealRequest,
  Demanda,
  FichaCasaRequest,
  FinalizacaoResultado,
  OrdemProducao,
  OrdemProducaoResumo,
  StatusFicha,
} from './producao.model';

export function hojeIso(): string {
  const d = new Date();
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
}

/** Estado compartilhado do Módulo de Produção (planejamento + execução do dia). */
@Injectable({ providedIn: 'root' })
export class ProducaoStore {
  private readonly api = inject(ProducaoService);

  // ===================== Planejamento =====================
  readonly demanda = signal<Demanda>({ personalizadas: [], casa: [] });
  readonly ordensPlanejadas = signal<OrdemProducao[]>([]); // não-finalizadas, completas
  readonly carregandoPlanejar = signal(false);

  /** entregaItemIds já planejados (em alguma ordem não-finalizada). */
  readonly planejadosIds = computed(() => {
    const set = new Set<number>();
    for (const o of this.ordensPlanejadas()) {
      for (const f of o.fichas) {
        if (f.entregaItemId != null) {
          set.add(f.entregaItemId);
        }
      }
    }
    return set;
  });

  async carregarDemanda(inicio: string, fim: string): Promise<void> {
    this.carregandoPlanejar.set(true);
    try {
      const [demanda] = await Promise.all([firstValueFrom(this.api.obterDemanda(inicio, fim)), this.carregarOrdens()]);
      this.demanda.set(demanda);
    } finally {
      this.carregandoPlanejar.set(false);
    }
  }

  async carregarOrdens(): Promise<void> {
    const resumos = await firstValueFrom(this.api.listarOrdens());
    const naoFinalizadas = resumos.filter((o) => o.status !== 'Finalizada');
    const completas = await Promise.all(naoFinalizadas.map((o) => firstValueFrom(this.api.obter(o.id))));
    this.ordensPlanejadas.set(completas.sort((a, b) => a.data.localeCompare(b.data)));
  }

  async planejar(data: string, entregaItemIds: number[], casa: FichaCasaRequest[]): Promise<OrdemProducao> {
    const ordem = await firstValueFrom(this.api.criarOuObter(data));
    const atualizada = await firstValueFrom(this.api.adicionarFichas(ordem.id, { entregaItemIds, casa }));
    await this.carregarOrdens();
    return atualizada;
  }

  async adicionarNaProducao(ordemId: number, entregaItemId: number): Promise<void> {
    await firstValueFrom(this.api.adicionarFichas(ordemId, { entregaItemIds: [entregaItemId], casa: [] }));
    await this.carregarOrdens();
  }

  async removerFicha(fichaId: number): Promise<void> {
    await firstValueFrom(this.api.removerFicha(fichaId));
    await this.carregarOrdens();
  }

  // ===================== Execução (Produção do dia / Cozinha) =====================
  readonly ordensResumo = signal<OrdemProducaoResumo[]>([]);
  readonly dataSelecionada = signal<string>(hojeIso());
  readonly ordem = signal<OrdemProducao | null>(null);
  readonly carregandoDia = signal(false);

  async carregarOrdensResumo(): Promise<void> {
    this.ordensResumo.set(await firstValueFrom(this.api.listarOrdens()));
  }

  /** Define o dia exibido e carrega a ordem correspondente (ou null se não existir). */
  async selecionarDia(data: string): Promise<void> {
    this.dataSelecionada.set(data);
    await this.carregarPorData(data);
  }

  async carregarPorData(data: string): Promise<void> {
    this.carregandoDia.set(true);
    try {
      this.ordem.set(await firstValueFrom(this.api.obterPorData(data)));
    } catch {
      this.ordem.set(null);
    } finally {
      this.carregandoDia.set(false);
    }
  }

  /** Garante uma ordem carregada para a tela do dia/cozinha (usa o dia selecionado). */
  async garantirOrdemDia(): Promise<void> {
    await this.carregarOrdensResumo();
    const resumos = this.ordensResumo();
    const hoje = hojeIso();
    let alvo = this.dataSelecionada();
    if (!resumos.some((o) => o.data === alvo)) {
      const naoFinal = resumos.filter((o) => o.status !== 'Finalizada').sort((a, b) => a.data.localeCompare(b.data));
      alvo = resumos.some((o) => o.data === hoje) ? hoje : naoFinal[0]?.data ?? hoje;
    }
    await this.selecionarDia(alvo);
  }

  async mudarStatusFicha(fichaId: number, status: StatusFicha): Promise<void> {
    this.ordem.set(await firstValueFrom(this.api.mudarStatusFicha(fichaId, status)));
  }

  async concluirFicha(fichaId: number, req: ConcluirFichaRequest): Promise<void> {
    this.ordem.set(await firstValueFrom(this.api.concluirFicha(fichaId, req)));
  }

  async marcarNaoFeita(fichaId: number, motivo: string): Promise<void> {
    this.ordem.set(await firstValueFrom(this.api.marcarNaoFeita(fichaId, motivo)));
  }

  /** Registra a pesagem real (cru) e finaliza a ordem. Retorna o resultado (pendências etc.). */
  async finalizarDia(consumos: ConsumoRealRequest[], tudoProduzido: boolean, observacoes: string | null): Promise<FinalizacaoResultado> {
    const o = this.ordem();
    if (!o) {
      throw new Error('Nenhuma ordem carregada.');
    }
    if (consumos.length > 0) {
      await firstValueFrom(this.api.registrarConsumo(o.id, { itens: consumos }));
    }
    const resultado = await firstValueFrom(this.api.finalizar(o.id, { tudoProduzido, observacoes }));
    await this.carregarPorData(o.data);
    await this.carregarOrdensResumo();
    return resultado;
  }
}
