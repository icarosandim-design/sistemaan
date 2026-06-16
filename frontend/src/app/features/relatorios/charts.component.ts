import { Component, Input } from '@angular/core';

/** Ponto genérico para os gráficos do Dashboard (sem dependência externa). */
export interface ChartPoint {
  label: string;
  valor: number;
  valor2?: number;
}

const PALETA = ['#3f4f2d', '#b08d57', '#7a8450', '#a8b17e', '#8c6b3f', '#5c6b3a', '#c8881f', '#2a5599'];

function fmtNum(v: number): string {
  return v.toLocaleString('pt-BR', { maximumFractionDigits: v >= 100 ? 0 : 1 });
}

// ---------------------------------------------------------------------------
// Barras verticais (1 ou 2 séries) — com rótulos de valor
// ---------------------------------------------------------------------------
@Component({
  selector: 'an-bar-chart',
  standalone: true,
  template: `
    @if (!dados.length) {
      <p class="vazio-graf">Sem dados no período.</p>
    } @else {
      <svg viewBox="0 0 640 340" class="graf" preserveAspectRatio="xMidYMid meet">
        <line x1="34" [attr.y1]="base" x2="634" [attr.y2]="base" stroke="#e3d9c6" stroke-width="1.5" />
        @for (b of barras(); track b.label + $index) {
          <rect [attr.x]="b.x1" [attr.y]="b.y1" [attr.width]="b.w" [attr.height]="b.h1" [attr.fill]="cor" rx="3">
            <title>{{ b.label }}: {{ b.v1f }}</title>
          </rect>
          <text [attr.x]="series2 ? b.x1 + b.w / 2 : b.cx" [attr.y]="b.y1 - 8" text-anchor="middle" class="val">{{ b.v1f }}</text>
          @if (series2) {
            <rect [attr.x]="b.x2" [attr.y]="b.y2" [attr.width]="b.w" [attr.height]="b.h2" [attr.fill]="cor2" rx="3">
              <title>{{ b.label }}: {{ b.v2f }}</title>
            </rect>
            <text [attr.x]="b.x2 + b.w / 2" [attr.y]="b.y2 - 8" text-anchor="middle" class="val val2">{{ b.v2f }}</text>
          }
          <text [attr.x]="b.cx" [attr.y]="base + 26" text-anchor="middle" class="lbl">{{ b.label }}</text>
        }
      </svg>
    }
  `,
  styles: [`
    .graf { width: 100%; height: auto; min-height: 220px; }
    .lbl { font-size: 18px; fill: var(--an-texto-secundario); font-weight: 600; }
    .val { font-size: 17px; fill: var(--an-texto-titulo); font-weight: 700; }
    .val2 { fill: var(--an-cta-hover); }
    .vazio-graf { color: var(--an-texto-secundario); font-size: 0.9rem; text-align: center; padding: 2rem 0; }
  `],
})
export class BarChartComponent {
  @Input() dados: ChartPoint[] = [];
  @Input() series2 = false;
  @Input() cor = PALETA[0];
  @Input() cor2 = PALETA[1];

  readonly base = 300; // linha de base (top 30 p/ rótulos + innerH 270)

  barras() {
    const padX = 34, top = 30, w = 640;
    const innerW = w - padX * 2;
    const innerH = this.base - top;
    const n = this.dados.length;
    const groupW = innerW / n;
    const max = Math.max(1, ...this.dados.map((d) => Math.max(d.valor, d.valor2 ?? 0)));
    const bw = this.series2 ? Math.min(groupW * 0.34, 70) : Math.min(groupW * 0.55, 90);
    return this.dados.map((d, i) => {
      const cx = padX + groupW * i + groupW / 2;
      const h1 = Math.max(1, (d.valor / max) * innerH);
      const h2 = Math.max(1, ((d.valor2 ?? 0) / max) * innerH);
      const x1 = this.series2 ? cx - bw - 3 : cx - bw / 2;
      const x2 = cx + 3;
      return {
        label: d.label, cx, w: bw,
        x1, y1: this.base - h1, h1, v1f: fmtNum(d.valor),
        x2, y2: this.base - h2, h2, v2f: fmtNum(d.valor2 ?? 0),
      };
    });
  }
}

// ---------------------------------------------------------------------------
// Barras horizontais (ranking)
// ---------------------------------------------------------------------------
@Component({
  selector: 'an-hbar-chart',
  standalone: true,
  template: `
    @if (!dados.length) {
      <p class="vazio-graf">Sem dados no período.</p>
    } @else {
      <div class="hbar">
        @for (b of barras(); track b.label + $index) {
          <div class="row">
            <span class="rl" [title]="b.label">{{ b.label }}</span>
            <div class="track"><div class="fill" [style.width.%]="b.pct" [style.background]="cor"></div></div>
            <span class="rv">{{ fmt(b.valor) }}</span>
          </div>
        }
      </div>
    }
  `,
  styles: [`
    .vazio-graf { color: var(--an-texto-secundario); font-size: 0.9rem; text-align: center; padding: 2rem 0; }
    .hbar { display: flex; flex-direction: column; gap: 0.6rem; }
    .row { display: grid; grid-template-columns: minmax(0,10rem) 1fr auto; align-items: center; gap: 0.6rem; font-size: 0.92rem; }
    .rl { overflow: hidden; text-overflow: ellipsis; white-space: nowrap; color: var(--an-texto-titulo); font-weight: 500; }
    .track { background: var(--an-fundo-secundario); border-radius: 6px; height: 20px; overflow: hidden; }
    .fill { height: 100%; border-radius: 6px; min-width: 3px; }
    .rv { font-variant-numeric: tabular-nums; color: var(--an-texto-titulo); font-weight: 700; min-width: 2.5rem; text-align: right; }
  `],
})
export class HBarChartComponent {
  @Input() dados: ChartPoint[] = [];
  @Input() cor = PALETA[0];
  @Input() prefixo = '';
  @Input() sufixo = '';

  barras() {
    const max = Math.max(1, ...this.dados.map((d) => d.valor));
    return this.dados.map((d) => ({ ...d, pct: (d.valor / max) * 100 }));
  }

  fmt(v: number): string {
    return `${this.prefixo}${fmtNum(v)}${this.sufixo}`;
  }
}

// ---------------------------------------------------------------------------
// Donut
// ---------------------------------------------------------------------------
@Component({
  selector: 'an-donut-chart',
  standalone: true,
  template: `
    @if (!total()) {
      <p class="vazio-graf">Sem dados no período.</p>
    } @else {
      <div class="donut-wrap">
        <svg viewBox="0 0 120 120" class="donut">
          @for (s of segmentos(); track s.label) {
            <circle cx="60" cy="60" r="45" fill="none" [attr.stroke]="s.cor" stroke-width="20"
              [attr.stroke-dasharray]="s.dash" [attr.stroke-dashoffset]="s.offset"
              transform="rotate(-90 60 60)">
              <title>{{ s.label }}: {{ s.pct }}%</title>
            </circle>
          }
        </svg>
        <ul class="legenda">
          @for (s of segmentos(); track s.label) {
            <li><i [style.background]="s.cor"></i><span class="lg-nome">{{ s.label }}</span> <b>{{ s.pct }}%</b></li>
          }
        </ul>
      </div>
    }
  `,
  styles: [`
    .vazio-graf { color: var(--an-texto-secundario); font-size: 0.9rem; text-align: center; padding: 2rem 0; }
    .donut-wrap { display: flex; align-items: center; gap: 1.25rem; flex-wrap: wrap; }
    .donut { width: 180px; height: 180px; }
    .legenda { list-style: none; margin: 0; padding: 0; display: flex; flex-direction: column; gap: 0.5rem; font-size: 0.95rem; }
    .legenda li { display: flex; align-items: center; gap: 0.5rem; }
    .legenda i { width: 14px; height: 14px; border-radius: 3px; display: inline-block; }
    .lg-nome { color: var(--an-texto-titulo); }
  `],
})
export class DonutChartComponent {
  @Input() dados: ChartPoint[] = [];

  total(): number {
    return this.dados.reduce((a, d) => a + d.valor, 0);
  }

  segmentos() {
    const c = 2 * Math.PI * 45;
    const tot = this.total() || 1;
    let acc = 0;
    return this.dados.map((d, i) => {
      const frac = d.valor / tot;
      const len = frac * c;
      const seg = { label: d.label, cor: PALETA[i % PALETA.length], dash: `${len} ${c - len}`, offset: -acc, pct: Math.round(frac * 100) };
      acc += len;
      return seg;
    });
  }
}

// ---------------------------------------------------------------------------
// Linha — com rótulos de valor e pontos maiores
// ---------------------------------------------------------------------------
@Component({
  selector: 'an-line-chart',
  standalone: true,
  template: `
    @if (dados.length < 2) {
      <p class="vazio-graf">Dados insuficientes para a linha.</p>
    } @else {
      <svg viewBox="0 0 640 340" class="graf" preserveAspectRatio="xMidYMid meet">
        <polyline [attr.points]="pontos()" fill="none" [attr.stroke]="cor" stroke-width="3.5" stroke-linejoin="round" />
        @for (p of vertices(); track p.label + $index) {
          <circle [attr.cx]="p.x" [attr.cy]="p.y" r="5" [attr.fill]="cor"><title>{{ p.label }}: {{ p.vf }}</title></circle>
          <text [attr.x]="p.x" [attr.y]="p.y - 12" text-anchor="middle" class="val">{{ p.vf }}</text>
          <text [attr.x]="p.x" y="326" text-anchor="middle" class="lbl">{{ p.label }}</text>
        }
      </svg>
    }
  `,
  styles: [`
    .graf { width: 100%; height: auto; min-height: 220px; }
    .lbl { font-size: 18px; fill: var(--an-texto-secundario); font-weight: 600; }
    .val { font-size: 16px; fill: var(--an-texto-titulo); font-weight: 700; }
    .vazio-graf { color: var(--an-texto-secundario); font-size: 0.9rem; text-align: center; padding: 2rem 0; }
  `],
})
export class LineChartComponent {
  @Input() dados: ChartPoint[] = [];
  @Input() cor = PALETA[1];

  vertices() {
    const padX = 44, top = 36, bottom = 50, w = 640, h = 340;
    const innerW = w - padX * 2;
    const innerH = h - top - bottom;
    const n = this.dados.length;
    const max = Math.max(1, ...this.dados.map((d) => d.valor));
    const min = Math.min(...this.dados.map((d) => d.valor), 0);
    const span = max - min || 1;
    return this.dados.map((d, i) => ({
      label: d.label,
      vf: fmtNum(d.valor),
      x: padX + (innerW * i) / (n - 1),
      y: top + innerH - ((d.valor - min) / span) * innerH,
    }));
  }

  pontos(): string {
    return this.vertices().map((p) => `${p.x},${p.y}`).join(' ');
  }
}
