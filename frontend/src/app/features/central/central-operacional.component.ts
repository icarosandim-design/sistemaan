import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog } from '@angular/material/dialog';
import { CentralResumo, Kpi } from './central.model';
import { CentralService } from './central.service';
import { AuthService } from '../../core/auth/auth.service';
import { PERFIL } from '../../core/auth/perfis';
import { VendaAvulsaDialogComponent } from '../vendas/venda-avulsa-dialog.component';

@Component({
  selector: 'app-central-operacional',
  standalone: true,
  imports: [RouterLink, MatCardModule, MatIconModule, MatButtonModule, MatMenuModule, MatTooltipModule, MatProgressSpinnerModule],
  templateUrl: './central-operacional.component.html',
  styleUrl: './central-operacional.component.scss',
})
export class CentralOperacionalComponent implements OnInit {
  private readonly service = inject(CentralService);
  private readonly auth = inject(AuthService);
  private readonly dialog = inject(MatDialog);

  readonly resumo = signal<CentralResumo | null>(null);
  readonly carregando = signal(true);
  readonly erro = signal(false);

  /** Card de produção começa minimizado. */
  readonly producaoExpandida = signal(false);

  /** Só o Administrador enxerga KPIs financeiros (faturamento recorrente). */
  get isAdmin(): boolean {
    return this.auth.temPapel(PERFIL.ADMIN);
  }

  ngOnInit(): void {
    this.carregar();
  }

  /** Abre o fluxo de Venda avulsa PF; ao concluir, recarrega o resumo. */
  abrirVendaAvulsa(): void {
    const ref = this.dialog.open(VendaAvulsaDialogComponent, { autoFocus: false, maxWidth: '94vw' });
    ref.afterClosed().subscribe((r) => {
      if (r) {
        this.carregar();
      }
    });
  }

  carregar(): void {
    this.carregando.set(true);
    this.erro.set(false);
    this.service.obterResumo().subscribe({
      next: (r) => {
        this.resumo.set(r);
        this.carregando.set(false);
      },
      error: () => {
        this.erro.set(true);
        this.carregando.set(false);
      },
    });
  }

  // ===== Acessores resilientes ao resumo ausente =====
  get diaSelecionado(): string {
    return this.resumo()?.diaSelecionado ?? '';
  }

  get kpis() {
    return this.resumo()?.kpis ?? [];
  }

  /** KPIs visíveis ao perfil atual: cartões financeiros são só do Administrador. */
  get kpisVisiveis(): Kpi[] {
    return this.kpis.filter((k) => !k.somenteAdmin || this.isAdmin);
  }

  get producao() {
    return this.resumo()?.producao ?? null;
  }

  get rotas() {
    return this.resumo()?.rotas ?? null;
  }

  get entregas7() {
    return this.resumo()?.entregas7 ?? [];
  }

  get producaoCasa() {
    return this.resumo()?.producaoCasa ?? [];
  }

  get producaoPersonalizada() {
    return this.resumo()?.producaoPersonalizada ?? [];
  }

  get ingredientes() {
    return this.resumo()?.ingredientes ?? [];
  }

  get estoque() {
    return this.resumo()?.estoque ?? [];
  }

  get alertas() {
    return this.resumo()?.alertas ?? [];
  }

  get temProducao(): boolean {
    return this.producaoCasa.length > 0 || this.producaoPersonalizada.length > 0;
  }

  // ===== Derivados de produção =====
  get subtotalCasa(): number {
    return this.producaoCasa.reduce((s, c) => s + c.pacotes, 0);
  }

  get subtotalPersonalizada(): number {
    return this.producaoPersonalizada.reduce((s, p) => s + p.pacotes, 0);
  }

  get totalPacotes(): number {
    return this.subtotalCasa + this.subtotalPersonalizada;
  }

  get totalReceitas(): number {
    return this.producaoCasa.length + this.producaoPersonalizada.length;
  }

  private readonly maxCasaVisivel = 8;
  private readonly maxPersVisivel = 12;

  get casaVisivel() {
    return this.producaoCasa.slice(0, this.maxCasaVisivel);
  }

  get casaExtra(): number {
    return Math.max(0, this.producaoCasa.length - this.maxCasaVisivel);
  }

  get persVisivel() {
    return this.producaoPersonalizada.slice(0, this.maxPersVisivel);
  }

  get persExtra(): number {
    return Math.max(0, this.producaoPersonalizada.length - this.maxPersVisivel);
  }

  get casaDensidade(): string {
    const n = this.producaoCasa.length;
    return n <= 3 ? 'd1' : n <= 6 ? 'd2' : 'd3';
  }

  get persDensidade(): string {
    const n = this.producaoPersonalizada.length;
    return n <= 6 ? 'd1' : n <= 12 ? 'd2' : 'd3';
  }

  get faltantes(): { nome: string; falta: number }[] {
    return this.ingredientes
      .filter((i) => i.estoque < i.crus)
      .map((i) => ({ nome: i.nome, falta: i.crus - i.estoque }));
  }

  get bannerTexto(): string {
    const partes = this.faltantes.map((f) => `${this.fmtNum(f.falta)} kg de ${f.nome.toLowerCase()}`);
    return `Comprar ${this.juntar(partes)}.`;
  }

  fmtKg(v: number): string {
    return `${this.fmtNum(v)}kg`;
  }

  private fmtNum(v: number): string {
    return v.toLocaleString('pt-BR', { maximumFractionDigits: 1 });
  }

  // ===== Datas (a API entrega ISO; a tela formata mantendo o visual) =====
  /** Converte ISO yyyy-MM-dd em Date local (sem deslocamento de fuso). */
  private toDate(iso: string): Date {
    const [y, m, d] = (iso || '').split('-').map(Number);
    return new Date(y, (m || 1) - 1, d || 1);
  }

  /** Formata ISO em dd/MM. */
  fmtData(iso: string): string {
    if (!iso) {
      return '';
    }
    const [, m, d] = iso.split('-');
    return `${d}/${m}`;
  }

  /** Dia da semana abreviado (ex.: "Qua"). */
  diaSemana(iso: string): string {
    const rotulo = this.toDate(iso).toLocaleDateString('pt-BR', { weekday: 'short' }).replace('.', '');
    return rotulo.charAt(0).toUpperCase() + rotulo.slice(1);
  }

  get diaSelecionadoFmt(): string {
    return this.fmtData(this.diaSelecionado);
  }

  rotuloStatusProducao(s: string): string {
    switch (s) {
      case 'Planejada':
        return 'Planejada';
      case 'EmAndamento':
        return 'Em andamento';
      case 'Finalizada':
        return 'Finalizada';
      default:
        return s;
    }
  }

  private juntar(itens: string[]): string {
    if (itens.length <= 1) {
      return itens.join('');
    }
    return `${itens.slice(0, -1).join(', ')} e ${itens[itens.length - 1]}`;
  }
}
