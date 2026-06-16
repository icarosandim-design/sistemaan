import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { RelatoriosService } from './relatorios.service';
import { Dashboard } from './relatorios.model';
import { IngredientesService } from '../ingredientes/ingredientes.service';
import { Ingrediente } from '../ingredientes/ingredientes.model';
import { BarChartComponent, HBarChartComponent, DonutChartComponent, LineChartComponent, ChartPoint } from './charts.component';

@Component({
  selector: 'app-relatorios-dashboard',
  standalone: true,
  imports: [
    FormsModule, MatCardModule, MatIconModule, MatButtonModule, MatTooltipModule, MatProgressSpinnerModule,
    BarChartComponent, HBarChartComponent, DonutChartComponent, LineChartComponent,
  ],
  template: `
    <section class="pagina">
      <header class="page-head">
        <div class="page-title">
          <h1>Dashboard gerencial</h1>
          <p class="subtitulo">Visão consolidada do período — vendas, cancelamentos, produção e custos.</p>
        </div>
        <div class="filtros">
          <label>De <input type="date" [(ngModel)]="inicio" /></label>
          <label>até <input type="date" [(ngModel)]="fim" /></label>
          <button mat-flat-button class="btn-cta" (click)="carregar()"><mat-icon>search</mat-icon> Aplicar</button>
        </div>
      </header>

      @if (carregando()) {
        <div class="estado"><mat-spinner diameter="36"></mat-spinner><p>Carregando indicadores...</p></div>
      } @else if (erro()) {
        <div class="estado estado-erro">
          <mat-icon>cloud_off</mat-icon>
          <p>Não foi possível carregar o dashboard.</p>
          <button mat-stroked-button (click)="carregar()"><mat-icon>refresh</mat-icon> Tentar novamente</button>
        </div>
      } @else if (dados()) {
        @let d = dados()!;

        <div class="cards">
          @for (c of d.cards; track c.chave) {
            <mat-card class="card" appearance="outlined" [class.pendente]="c.pendente">
              <div class="c-valor">{{ c.valor }}
                @if (c.estimado) { <mat-icon class="badge" matTooltip="Valor estimado (custo médio)">info</mat-icon> }
                @if (c.pendente) { <mat-icon class="badge" matTooltip="Depende de dados ainda não disponíveis">schedule</mat-icon> }
              </div>
              <div class="c-label">{{ c.label }}</div>
              @if (c.detalhe) { <div class="c-det">{{ c.detalhe }}</div> }
            </mat-card>
          }
        </div>

        <div class="grafs">
          <section class="bloco">
            <h2 class="bt"><mat-icon>show_chart</mat-icon> Vendas por mês (kg)</h2>
            <an-bar-chart [dados]="vendasMes()" cor="#3f4f2d"></an-bar-chart>
          </section>

          <section class="bloco">
            <h2 class="bt"><mat-icon>donut_large</mat-icon> Vendas por tipo (kg)</h2>
            <an-donut-chart [dados]="vendasTipo()"></an-donut-chart>
          </section>

          <section class="bloco">
            <h2 class="bt"><mat-icon>cancel</mat-icon> Cancelamentos por motivo</h2>
            <an-hbar-chart [dados]="cancelMotivo()" cor="#b3261e"></an-hbar-chart>
          </section>

          <section class="bloco">
            <h2 class="bt"><mat-icon>bar_chart</mat-icon> Produção planejada × real (kg)</h2>
            <div class="leg"><i class="i1"></i> Planejado <i class="i2"></i> Real</div>
            <an-bar-chart [dados]="prodPlanReal()" [series2]="true"></an-bar-chart>
          </section>

          <section class="bloco">
            <h2 class="bt"><mat-icon>warning</mat-icon> Custo das perdas por ingrediente (R$)</h2>
            <an-hbar-chart [dados]="custoPerdas()" cor="#c8881f" prefixo="R$ "></an-hbar-chart>
          </section>

          <section class="bloco">
            <h2 class="bt"><mat-icon>trending_up</mat-icon> Evolução do custo médio (compra)</h2>
            <label class="sel">Insumo
              <select [(ngModel)]="ingredienteId" (ngModelChange)="carregar()">
                @for (i of ingredientes(); track i.id) { <option [value]="i.id">{{ i.nome }}</option> }
              </select>
            </label>
            <an-line-chart [dados]="evolucao()" cor="#b08d57"></an-line-chart>
          </section>
        </div>
      }
    </section>
  `,
  styles: [`
    .pagina { display: flex; flex-direction: column; gap: 1rem; max-width: 1200px; margin: 0 auto; }
    .page-head { display: flex; align-items: flex-start; justify-content: space-between; gap: 1rem; flex-wrap: wrap; }
    .page-title h1 { margin: 0; font-size: 1.5rem; font-weight: 700; }
    .subtitulo { margin: 0.35rem 0 0; font-size: 0.85rem; color: var(--an-texto-secundario); }
    .filtros { display: flex; align-items: center; gap: 0.5rem; flex-wrap: wrap; }
    .filtros label { display: flex; align-items: center; gap: 0.3rem; font-size: 0.82rem; color: var(--an-texto-secundario); }
    .filtros input[type=date] { padding: 0.3rem 0.4rem; border: 1px solid var(--an-fundo-secundario); border-radius: 6px; }
    .btn-cta { background: var(--an-cta); color: #fff; }
    .estado { display: flex; flex-direction: column; align-items: center; gap: 0.6rem; padding: 3rem 1rem; color: var(--an-texto-secundario); }
    .estado-erro .mat-icon { font-size: 2.4rem; width: 2.4rem; height: 2.4rem; }
    .cards { display: grid; grid-template-columns: repeat(auto-fill, minmax(180px, 1fr)); gap: 0.75rem; }
    .card { padding: 0.8rem 0.9rem; }
    .card.pendente { opacity: 0.75; }
    .c-valor { font-size: 1.4rem; font-weight: 700; color: var(--an-texto-titulo); display: flex; align-items: center; gap: 0.3rem; }
    .c-label { font-size: 0.82rem; color: var(--an-texto-secundario); margin-top: 0.15rem; }
    .c-det { font-size: 0.72rem; color: var(--an-detalhe-suave); margin-top: 0.1rem; }
    .badge { font-size: 1rem; width: 1rem; height: 1rem; color: var(--an-detalhe-suave); }
    .grafs { display: grid; grid-template-columns: repeat(auto-fit, minmax(330px, 1fr)); gap: 1rem; }
    .bloco { background: var(--an-superficie); border: 1px solid var(--an-fundo-secundario); border-radius: var(--an-raio); padding: 0.9rem 1rem; }
    .bt { display: flex; align-items: center; gap: 0.4rem; margin: 0 0 0.7rem; font-size: 1rem; font-weight: 700; color: var(--an-texto-titulo); }
    .bt .mat-icon { color: var(--an-cta); }
    .leg { display: flex; align-items: center; gap: 0.4rem; font-size: 0.78rem; color: var(--an-texto-secundario); margin-bottom: 0.3rem; }
    .leg i { width: 12px; height: 12px; border-radius: 3px; display: inline-block; }
    .leg .i1 { background: #3f4f2d; } .leg .i2 { background: #b08d57; margin-left: 0.5rem; }
    .sel { display: flex; align-items: center; gap: 0.4rem; font-size: 0.82rem; color: var(--an-texto-secundario); margin-bottom: 0.5rem; }
    .sel select { padding: 0.3rem 0.4rem; border: 1px solid var(--an-fundo-secundario); border-radius: 6px; }
  `],
})
export class RelatoriosDashboardComponent implements OnInit {
  private readonly service = inject(RelatoriosService);
  private readonly ingService = inject(IngredientesService);

  inicio = '';
  fim = '';
  ingredienteId: number | null = null;

  readonly dados = signal<Dashboard | null>(null);
  readonly ingredientes = signal<Ingrediente[]>([]);
  readonly carregando = signal(false);
  readonly erro = signal(false);

  readonly vendasMes = computed<ChartPoint[]>(() =>
    (this.dados()?.vendasPorMes ?? []).map((m) => ({ label: mesLabel(m.mes), valor: m.kg, valor2: m.quantidade })));
  readonly vendasTipo = computed<ChartPoint[]>(() =>
    (this.dados()?.vendasPorTipo ?? []).map((t) => ({ label: t.tipo, valor: t.kg })));
  readonly cancelMotivo = computed<ChartPoint[]>(() =>
    (this.dados()?.cancelamentosPorMotivo ?? []).map((c) => ({ label: c.motivo, valor: c.quantidade })));
  readonly prodPlanReal = computed<ChartPoint[]>(() =>
    (this.dados()?.producaoPlanejadoReal ?? []).map((p) => ({ label: mesLabel(p.periodo), valor: p.kgPlanejado, valor2: p.kgReal })));
  readonly custoPerdas = computed<ChartPoint[]>(() =>
    (this.dados()?.custoPerdasTop ?? []).map((c) => ({ label: c.ingrediente, valor: c.perdaValor })));
  readonly evolucao = computed<ChartPoint[]>(() =>
    (this.dados()?.evolucaoCustoInsumo ?? []).map((e) => ({ label: mesLabel(e.mes), valor: e.custoMedioCompra })));

  ngOnInit(): void {
    const hoje = new Date();
    const ini = new Date(hoje);
    ini.setMonth(hoje.getMonth() - 5);
    ini.setDate(1);
    this.fim = iso(hoje);
    this.inicio = iso(ini);
    this.ingService.listar().subscribe({ next: (is) => this.ingredientes.set(is) });
    this.carregar();
  }

  carregar(): void {
    if (!this.inicio || !this.fim) return;
    this.carregando.set(true);
    this.erro.set(false);
    this.service.dashboard(this.inicio, this.fim, this.ingredienteId ?? undefined).subscribe({
      next: (d) => {
        this.dados.set(d);
        if (this.ingredienteId == null && d.ingredienteEvolucaoId != null) {
          this.ingredienteId = d.ingredienteEvolucaoId;
        }
        this.carregando.set(false);
      },
      error: () => { this.erro.set(true); this.carregando.set(false); },
    });
  }
}

function iso(d: Date): string {
  return d.toISOString().slice(0, 10);
}

function mesLabel(mes: string): string {
  // "2026-06" -> "06/26"
  const [y, m] = mes.split('-');
  return m && y ? `${m}/${y.slice(2)}` : mes;
}
