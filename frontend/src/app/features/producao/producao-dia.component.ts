import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ConsumoConsolidado, ConsumoRealRequest, FichaProducao, fmtPeso, rotuloStatusFicha, rotuloStatusOrdem } from './producao.model';
import { ProducaoStore } from './producao.store';
import { ProducaoFinalizarDialogComponent, FinalizarResult } from './finalizar-dialog.component';
import { FichaMaxComponent, FichasMaxComponent, FULLSCREEN, IngredientesMaxComponent } from './producao-max-dialogs.component';

const ORDEM_CATS = ['Proteínas', 'Carboidratos', 'Legumes', 'Temperos', 'Óleos', 'Suplementos', 'Outros'];

interface ConsumoLinha {
  cruKg: number | null;
  cozidoKg: number | null;
  sobraKg: number | null;
  perdaKg: number | null;
  motivo: string | null;
}

function gParaKg(gramas: number | null): number | null {
  return gramas != null ? gramas / 1000 : null;
}

@Component({
  selector: 'app-producao-dia',
  standalone: true,
  imports: [
    FormsModule, RouterLink, MatButtonModule, MatIconModule, MatTooltipModule,
    MatFormFieldModule, MatInputModule, MatSelectModule, MatProgressSpinnerModule,
  ],
  templateUrl: './producao-dia.component.html',
  styleUrl: './producao-dia.component.scss',
})
export class ProducaoDiaComponent implements OnInit {
  readonly store = inject(ProducaoStore);
  private readonly dialog = inject(MatDialog);
  private readonly snack = inject(MatSnackBar);

  readonly fmtPeso = fmtPeso;
  readonly rotuloStatusFicha = rotuloStatusFicha;
  readonly rotuloStatusOrdem = rotuloStatusOrdem;

  // Pesagem real por ingrediente (kg/observação) — alimenta a baixa na finalização.
  readonly consumo = signal<Map<number, ConsumoLinha>>(new Map());

  async ngOnInit(): Promise<void> {
    await this.store.garantirOrdemDia();
    this.seed();
  }

  async trocarDia(data: string): Promise<void> {
    await this.store.selecionarDia(data);
    this.seed();
  }

  /** Reidrata os campos reais a partir do que já está salvo na ordem carregada. */
  private seed(): void {
    const mapa = new Map<number, ConsumoLinha>();
    for (const c of this.ordem()?.consolidado ?? []) {
      mapa.set(c.ingredienteId, {
        cruKg: gParaKg(c.realCruGramas),
        cozidoKg: gParaKg(c.realCozidoGramas),
        sobraKg: gParaKg(c.sobraGramas),
        perdaKg: gParaKg(c.perdaGramas),
        motivo: c.observacao,
      });
    }
    this.consumo.set(mapa);
  }

  fmtData(iso: string): string {
    if (!iso) return '—';
    const [y, m, d] = iso.split('-');
    return `${d}/${m}/${y}`;
  }

  readonly ordem = this.store.ordem;
  readonly fichas = computed(() => this.ordem()?.fichas ?? []);
  readonly finalizada = computed(() => this.ordem()?.status === 'Finalizada');

  readonly totalFichas = computed(() => this.fichas().length);
  readonly pendentes = computed(() => this.fichas().filter((f) => f.status !== 'Conferida' && f.status !== 'NaoFeita').length);
  readonly concluidas = computed(() => this.fichas().filter((f) => f.status === 'Conferida').length);

  /** Consolidado agrupado por categoria, na ordem definida. */
  readonly gruposConsolidado = computed<{ categoria: string; itens: ConsumoConsolidado[] }[]>(() => {
    const map = new Map<string, ConsumoConsolidado[]>();
    for (const i of this.ordem()?.consolidado ?? []) {
      const lista = map.get(i.categoria) ?? [];
      lista.push(i);
      map.set(i.categoria, lista);
    }
    const ordenadas = ORDEM_CATS.filter((c) => map.has(c));
    const extras = [...map.keys()].filter((c) => !ORDEM_CATS.includes(c));
    return [...ordenadas, ...extras].map((c) => ({ categoria: c, itens: map.get(c)! }));
  });

  // ----- Pesagem real -----
  private linha(id: number): ConsumoLinha {
    return this.consumo().get(id) ?? { cruKg: null, cozidoKg: null, sobraKg: null, perdaKg: null, motivo: null };
  }

  private patchLinha(id: number, patch: Partial<ConsumoLinha>): void {
    this.consumo.update((m) => {
      const n = new Map(m);
      n.set(id, { ...this.linha(id), ...patch });
      return n;
    });
  }

  private num(v: number | null): number | null {
    return v === null || v === undefined || isNaN(v as number) ? null : v;
  }

  cruDe(i: ConsumoConsolidado) { return this.linha(i.ingredienteId).cruKg; }
  setCru(i: ConsumoConsolidado, v: number | null) { this.patchLinha(i.ingredienteId, { cruKg: this.num(v) }); }
  cozidoDe(i: ConsumoConsolidado) { return this.linha(i.ingredienteId).cozidoKg; }
  setCozido(i: ConsumoConsolidado, v: number | null) { this.patchLinha(i.ingredienteId, { cozidoKg: this.num(v) }); }
  sobraDe(i: ConsumoConsolidado) { return this.linha(i.ingredienteId).sobraKg; }
  setSobra(i: ConsumoConsolidado, v: number | null) { this.patchLinha(i.ingredienteId, { sobraKg: this.num(v) }); }
  perdaDe(i: ConsumoConsolidado) { return this.linha(i.ingredienteId).perdaKg; }
  setPerda(i: ConsumoConsolidado, v: number | null) { this.patchLinha(i.ingredienteId, { perdaKg: this.num(v) }); }
  motivoDe(i: ConsumoConsolidado) { return this.linha(i.ingredienteId).motivo; }
  setMotivo(i: ConsumoConsolidado, v: string) { this.patchLinha(i.ingredienteId, { motivo: v || null }); }

  /** Ingredientes do consolidado ainda sem o cru real informado (bloqueiam finalização). */
  readonly cruRealFaltando = computed(() =>
    (this.ordem()?.consolidado ?? []).filter((c) => this.linha(c.ingredienteId).cruKg == null).length,
  );

  /** Fichas ainda não finalizadas (nem Conferida nem Não feita). */
  readonly fichasEmAberto = computed(() =>
    this.fichas().filter((f) => f.status !== 'Conferida' && f.status !== 'NaoFeita').length,
  );

  // ----- Maximizar -----
  maximizarIngredientes(): void {
    this.dialog.open(IngredientesMaxComponent, FULLSCREEN);
  }

  maximizarFichas(): void {
    this.dialog.open(FichasMaxComponent, FULLSCREEN);
  }

  maximizarFicha(ficha: FichaProducao): void {
    this.dialog.open(FichaMaxComponent, { ...FULLSCREEN, data: { ficha } });
  }

  imprimirMapa(): void {
    const o = this.ordem();
    if (o) window.open(`/producao/impressao/mapa?ordem=${o.id}`, '_blank');
  }

  imprimirFichas(): void {
    const o = this.ordem();
    if (o) window.open(`/producao/impressao/fichas?ordem=${o.id}`, '_blank');
  }

  finalizar(): void {
    const o = this.ordem();
    if (!o) return;

    // Guarda 1: todas as fichas precisam estar Conferida ou Não feita.
    if (this.fichasEmAberto() > 0) {
      this.snack.open('Existem fichas ainda pendentes. Marque todas como Conferida ou Não feita antes de finalizar a produção.', 'OK', { duration: 5000 });
      return;
    }
    // Guarda 2: cru real obrigatório para todos os ingredientes.
    if (this.cruRealFaltando() > 0) {
      this.snack.open('Informe o peso cru real usado para todos os ingredientes antes de finalizar a produção.', 'OK', { duration: 5000 });
      return;
    }

    const ref = this.dialog.open(ProducaoFinalizarDialogComponent, { width: '560px', maxWidth: '96vw', autoFocus: false });
    ref.afterClosed().subscribe(async (res: FinalizarResult | undefined) => {
      if (!res) return;
      const kg = (v: number | null) => (v != null ? Math.round(v * 1000) : null);
      const itens: ConsumoRealRequest[] = (o.consolidado ?? []).map((c) => {
        const loc = this.linha(c.ingredienteId);
        return {
          ingredienteId: c.ingredienteId,
          realCruGramas: kg(loc.cruKg),
          realCozidoGramas: kg(loc.cozidoKg),
          sobraGramas: kg(loc.sobraKg),
          perdaGramas: kg(loc.perdaKg),
          motivo: loc.motivo ?? null,
        };
      });
      try {
        const r = await this.store.finalizarDia(itens, res.tudoProduzido, res.observacoes);
        this.seed();
        const pend = r.pendenciasEstoque.length
          ? ` Pendências de estoque: ${r.pendenciasEstoque.join(', ')}.`
          : '';
        this.snack.open(
          `Produção finalizada. ${r.fichasConcluidas} concluída(s), ${r.produtoAcabadoGerado} pacote(s) ao estoque, ${r.personalizadasProntas} entrega(s) prontas.${pend}`,
          'OK',
          { duration: 6000 },
        );
      } catch (e) {
        this.snack.open(this.erro(e), 'OK', { duration: 4000 });
      }
    });
  }

  private erro(e: unknown): string {
    const err = (e as { error?: { errors?: Record<string, string[]>; detail?: string } })?.error;
    const first = err?.errors ? Object.values(err.errors)[0]?.[0] : undefined;
    return first ?? err?.detail ?? 'Não foi possível finalizar.';
  }
}
