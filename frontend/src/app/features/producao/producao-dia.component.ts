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

  // Pesagem real (cru, em kg) e motivo por ingrediente — alimenta a baixa na finalização.
  readonly consumo = signal<Map<number, { cruKg: number | null; motivo: string | null }>>(new Map());

  ngOnInit(): void {
    this.store.garantirOrdemDia();
  }

  trocarDia(data: string): void {
    this.store.selecionarDia(data);
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
  cruDe(i: ConsumoConsolidado): number | null {
    return this.consumo().get(i.ingredienteId)?.cruKg ?? null;
  }

  setCru(i: ConsumoConsolidado, v: number | null): void {
    this.consumo.update((m) => {
      const n = new Map(m);
      const atual = n.get(i.ingredienteId) ?? { cruKg: null, motivo: null };
      n.set(i.ingredienteId, { ...atual, cruKg: v === null || isNaN(v as number) ? null : v });
      return n;
    });
  }

  motivoDe(i: ConsumoConsolidado): string | null {
    return this.consumo().get(i.ingredienteId)?.motivo ?? null;
  }

  setMotivo(i: ConsumoConsolidado, v: string): void {
    this.consumo.update((m) => {
      const n = new Map(m);
      const atual = n.get(i.ingredienteId) ?? { cruKg: null, motivo: null };
      n.set(i.ingredienteId, { ...atual, motivo: v || null });
      return n;
    });
  }

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
    const ref = this.dialog.open(ProducaoFinalizarDialogComponent, { width: '560px', maxWidth: '96vw', autoFocus: false });
    ref.afterClosed().subscribe(async (res: FinalizarResult | undefined) => {
      if (!res) return;
      const itens: ConsumoRealRequest[] = (o.consolidado ?? [])
        .map((c) => {
          const loc = this.consumo().get(c.ingredienteId);
          return {
            ingredienteId: c.ingredienteId,
            realCruGramas: loc?.cruKg != null ? Math.round(loc.cruKg * 1000) : null,
            realCozidoGramas: null,
            motivo: loc?.motivo ?? null,
          };
        })
        .filter((x) => x.realCruGramas != null || x.motivo);
      try {
        const r = await this.store.finalizarDia(itens, res.tudoProduzido, res.observacoes);
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
