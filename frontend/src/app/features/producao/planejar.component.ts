import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { DemandaCasa, FichaCasaRequest, fmtMoeda, fmtPeso, rotuloStatusPreparo } from './producao.model';
import { ProducaoStore } from './producao.store';
import { DataProducaoDialogComponent, EditarProducaoDialogComponent } from './producao-planejar-dialogs.component';

// Estimativas de planejamento (a demanda não traz ingredientes; valores precisos saem na Produção do dia).
const FATOR_CRU_ESTIMADO = 1.8;
const CUSTO_KG_CRU_ESTIMADO = 22;

@Component({
  selector: 'app-producao-planejar',
  standalone: true,
  imports: [
    FormsModule, MatButtonModule, MatIconModule, MatCheckboxModule,
    MatInputModule, MatFormFieldModule, MatProgressSpinnerModule,
  ],
  templateUrl: './planejar.component.html',
  styleUrl: './planejar.component.scss',
})
export class ProducaoPlanejarComponent implements OnInit {
  readonly store = inject(ProducaoStore);
  private readonly dialog = inject(MatDialog);
  private readonly snack = inject(MatSnackBar);
  readonly fmtPeso = fmtPeso;
  readonly fmtMoeda = fmtMoeda;
  readonly rotuloStatusPreparo = rotuloStatusPreparo;

  readonly janelas = ['Hoje', 'Amanhã', 'Próximos 3 dias', 'Próximos 7 dias', 'Próximos 15 dias', 'Próximos 30 dias'];
  janela = 'Próximos 7 dias';
  personalizado = false;
  rangeDe = '';
  rangeAte = '';

  busca = '';
  apenasNaoProntas = false;

  // Seleção local (não persistida até "Planejar produção").
  readonly selPers = signal<Set<number>>(new Set());
  readonly selCasa = signal<Map<string, number>>(new Map()); // chave -> qtd a produzir

  ngOnInit(): void {
    this.recarregar();
  }

  // ----- Janela / período -----
  selecionarJanela(j: string): void {
    this.janela = j;
    this.personalizado = false;
    this.recarregar();
  }

  ativarPersonalizado(): void {
    this.personalizado = true;
    if (this.rangeDe && this.rangeAte) {
      this.recarregar();
    }
  }

  aplicarRange(): void {
    if (this.rangeDe && this.rangeAte) {
      this.recarregar();
    }
  }

  recarregar(): void {
    const { inicio, fim } = this.periodo();
    this.store.carregarDemanda(inicio, fim);
  }

  private periodo(): { inicio: string; fim: string } {
    if (this.personalizado && this.rangeDe && this.rangeAte) {
      return { inicio: this.rangeDe, fim: this.rangeAte };
    }
    const hoje = new Date();
    const ini = new Date(hoje);
    const fim = new Date(hoje);
    switch (this.janela) {
      case 'Hoje': break;
      case 'Amanhã': ini.setDate(hoje.getDate() + 1); fim.setDate(hoje.getDate() + 1); break;
      case 'Próximos 3 dias': fim.setDate(hoje.getDate() + 2); break;
      case 'Próximos 7 dias': fim.setDate(hoje.getDate() + 6); break;
      case 'Próximos 15 dias': fim.setDate(hoje.getDate() + 14); break;
      case 'Próximos 30 dias': fim.setDate(hoje.getDate() + 29); break;
    }
    return { inicio: this.iso(ini), fim: this.iso(fim) };
  }

  private iso(d: Date): string {
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
  }

  get janelaTexto(): string {
    if (this.personalizado) {
      return this.rangeDe && this.rangeAte ? `${this.fmtData(this.rangeDe)} até ${this.fmtData(this.rangeAte)}` : 'período personalizado';
    }
    return this.janela;
  }

  fmtData(iso: string): string {
    if (!iso) return '—';
    const [y, m, d] = iso.split('-');
    return `${d}/${m}/${y}`;
  }

  relativo(iso: string): string {
    const hoje = new Date(); hoje.setHours(0, 0, 0, 0);
    const alvo = new Date(`${iso}T00:00:00`);
    const dias = Math.round((alvo.getTime() - hoje.getTime()) / 86400000);
    if (dias === 0) return 'hoje';
    if (dias === 1) return 'amanhã';
    if (dias > 1) return `em ${dias} dias`;
    return `há ${-dias} dia(s)`;
  }

  // ----- Disponíveis (demanda não planejada) -----
  readonly disponiveis = computed(() =>
    this.store.demanda().personalizadas.filter((p) => !this.store.planejadosIds().has(p.entregaItemId)),
  );

  get personalizadasFiltradas() {
    const t = this.busca.trim().toLowerCase();
    return this.disponiveis().filter((p) => {
      if (this.apenasNaoProntas && p.statusPreparo !== 'NaoPronta') return false;
      if (t && !p.petNome.toLowerCase().includes(t) && !p.clienteNome.toLowerCase().includes(t)) return false;
      return true;
    });
  }

  // ----- Seleção -----
  persSelecionada(id: number): boolean {
    return this.selPers().has(id);
  }

  togglePers(id: number): void {
    this.selPers.update((s) => {
      const n = new Set(s);
      n.has(id) ? n.delete(id) : n.add(id);
      return n;
    });
  }

  casaKey(c: DemandaCasa): string {
    return `${c.receitaId}-${c.tamanhoPacoteId}`;
  }

  casaIncluida(c: DemandaCasa): boolean {
    return this.selCasa().has(this.casaKey(c));
  }

  casaQtd(c: DemandaCasa): number {
    return this.selCasa().get(this.casaKey(c)) ?? 0;
  }

  podeIncluirCasa(c: DemandaCasa): boolean {
    return c.tamanhoPacoteId != null;
  }

  alternarCasa(c: DemandaCasa): void {
    if (!this.podeIncluirCasa(c)) return;
    this.selCasa.update((m) => {
      const n = new Map(m);
      const k = this.casaKey(c);
      if (n.has(k)) n.delete(k);
      else n.set(k, c.falta > 0 ? c.falta : c.necessario);
      return n;
    });
  }

  definirQtdCasa(c: DemandaCasa, qtd: number): void {
    this.selCasa.update((m) => {
      const n = new Map(m);
      n.set(this.casaKey(c), Math.max(0, Math.trunc(qtd) || 0));
      return n;
    });
  }

  get personalizadasSelecionadas(): number {
    return this.selPers().size;
  }

  get casaSelecionadas(): number {
    return this.selCasa().size;
  }

  // ----- Resumo -----
  get totalCozidoSelecionado(): number {
    const pers = this.disponiveis()
      .filter((p) => this.persSelecionada(p.entregaItemId))
      .reduce((s, p) => s + p.pacotes * p.pesoPacoteGramas, 0);
    const casa = this.store.demanda().casa
      .filter((c) => this.casaIncluida(c))
      .reduce((s, c) => s + this.casaQtd(c) * c.pesoGramas, 0);
    return pers + casa;
  }

  get totalCruSelecionado(): number {
    return Math.round(this.totalCozidoSelecionado * FATOR_CRU_ESTIMADO);
  }

  get custoEstimado(): number {
    return (this.totalCruSelecionado / 1000) * CUSTO_KG_CRU_ESTIMADO;
  }

  // ----- Ações -----
  planejar(): void {
    const ids = [...this.selPers()];
    const casa: FichaCasaRequest[] = this.store.demanda().casa
      .filter((c) => this.casaIncluida(c) && c.tamanhoPacoteId != null && this.casaQtd(c) > 0)
      .map((c) => ({ receitaId: c.receitaId, tamanhoPacoteId: c.tamanhoPacoteId as number, quantidadePacotes: this.casaQtd(c) }));

    if (ids.length === 0 && casa.length === 0) {
      this.snack.open('Selecione ao menos uma receita (personalizada ou da casa).', 'OK', { duration: 3000 });
      return;
    }

    const ref = this.dialog.open(DataProducaoDialogComponent, {
      data: { qtd: ids.length, qtdCasa: casa.length, custo: this.custoEstimado },
      autoFocus: false,
    });
    ref.afterClosed().subscribe(async (dataIso: string | undefined) => {
      if (!dataIso) return;
      try {
        await this.store.planejar(dataIso, ids, casa);
        this.selPers.set(new Set());
        this.selCasa.set(new Map());
        this.snack.open(`Produção planejada para ${this.fmtData(dataIso)}.`, 'OK', { duration: 3000 });
      } catch (e) {
        this.snack.open(this.erro(e), 'OK', { duration: 4000 });
      }
    });
  }

  editarProducao(ordemId: number): void {
    this.dialog.open(EditarProducaoDialogComponent, { data: { ordemId }, autoFocus: false, maxWidth: '96vw' });
  }

  private erro(e: unknown): string {
    const err = (e as { error?: { errors?: Record<string, string[]>; detail?: string } })?.error;
    const first = err?.errors ? Object.values(err.errors)[0]?.[0] : undefined;
    return first ?? err?.detail ?? 'Não foi possível concluir a operação.';
  }
}
