import { Component, Input } from '@angular/core';

/** Ponto genérico para os gráficos do Dashboard (sem dependência externa). */
export interface ChartPoint {
  label: string;
  valor: number;
  valor2?: number;
}

const PALETA = ['#3f4f2d', '#b08d57', '#7a8450', '#a8b17e', '#8c6b3f', '#5c6b3a', '#c8881f', '#2a5599'];

// ---------------------------------------------------------------------------
// Barras verticais (1 ou 2 séries)
// ---------------------------------------------------------------------------
@Component({
  selector: 'an-bar-chart',
  standalone: true,
  template: `
    @if (!dados.length) {
      <p class="vazio-graf">Sem dados no período.</p>
    } @else {
      <svg viewBox="0 0 600 250" class="graf" preserveAspectRatio="xMidYMid meet">
        @for (b of barras(); track b.label + $index) {
          <rect [attr.x]="b.x1" [attr.y]="b.y1" [attr.width]="b.w" [attr.height]="b.h1" [attr.fill]="cor" rx="2">
            <title>{{ b.label }}: {{ b.v1 }}</title>
          </rect>
          @if (series2) {
            <rect [attr.x]="b.x2" [attr.y]="b.y2" [attr.width]="b.w" [attr.height]="b.h2" [attr.fill]="cor2" rx="2">
              <title>{{ b.label }}: {{ b.v2 }}</title>
            </rect>
          }
          <text [attr.x]="b.cx" y="244" text-anchor="middle" class="lbl">{{ b.label }}</text>
        }
      </svg>
    }
  `,
  styles: [`
    .graf { width: 100%; height: auto; }
    .lbl { font-size: 9px; fill: var(--an-texto-secundario); }
    .vazio-graf { color: var(--an-texto-secundario); font-size: 0.85rem; text-align: center; padding: 1.5rem 0; }
  `],
})
export class BarChartComponent {
  @Input() dados: ChartPoint[] = [];
  @Input() series2 = false;
  @Input() cor = PALETA[0];
  @Input() cor2 = PALETA[1];

  barras() {
    const padX = 24, top = 12, bottom = 30, w = 600, h = 250;
    const innerW = w - padX * 2;
    const innerH = h - top - bottom;
    const n = this.dados.length;
    const groupW = innerW / n;
    const max = Math.max(1, ...this.dados.map((d) => Math.max(d.valor, d.valor2 ?? 0)));
    const bw = this.series2 ? groupW * 0.32 : Math.min(groupW * 0.6, 48);
    return this.dados.map((d, i) => {
      const cx = padX + groupW * i + groupW / 2;
      const h1 = (d.valor / max) * innerH;
      const h2 = ((d.valor2 ?? 0) / max) * innerH;
      const x1 = this.series2 ? cx - bw - 2 : cx - bw / 2;
      const x2 = cx + 2;
      return {
        label: d.label, cx, w: bw,
        x1, y1: top + innerH - h1, h1, v1: d.valor,
        x2, y2: top + innerH - h2, h2, v2: d.valor2 ?? 0,
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
    .vazio-graf { color: var(--an-texto-secundario); font-size: 0.85rem; text-align: center; padding: 1.5rem 0; }
    .hbar { display: flex; flex-direction: column; gap: 0.45rem; }
    .row { display: grid; grid-template-columns: minmax(0,9rem) 1fr auto; align-items: center; gap: 0.5rem; font-size: 0.8rem; }
    .rl { overflow: hidden; text-overflow: ellipsis; white-space: nowrap; color: var(--an-texto-titulo); }
    .track { background: var(--an-fundo-secundario); border-radius: 6px; height: 14px; overflow: hidden; }
    .fill { height: 100%; border-radius: 6px; min-width: 2px; }
    .rv { font-variant-numeric: tabular-nums; color: var(--an-texto-secundario); }
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
    return `${this.prefixo}${v.toLocaleString('pt-BR', { maximumFractionDigits: 2 })}${this.sufixo}`;
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
            <circle cx="60" cy="60" r="45" fill="none" [attr.stroke]="s.cor" stroke-width="18"
              [attr.stroke-dasharray]="s.dash" [attr.stroke-dashoffset]="s.offset"
              transform="rotate(-90 60 60)">
              <title>{{ s.label }}: {{ s.pct }}%</title>
            </circle>
          }
        </svg>
        <ul class="legenda">
          @for (s of segmentos(); track s.label) {
            <li><i [style.background]="s.cor"></i>{{ s.label }} <b>{{ s.pct }}%</b></li>
          }
        </ul>
      </div>
    }
  `,
  styles: [`
    .vazio-graf { color: var(--an-texto-secundario); font-size: 0.85rem; text-align: center; padding: 1.5rem 0; }
    .donut-wrap { display: flex; align-items: center; gap: 1rem; flex-wrap: wrap; }
    .donut { width: 140px; height: 140px; }
    .legenda { list-style: none; margin: 0; padding: 0; display: flex; flex-direction: column; gap: 0.35rem; font-size: 0.82rem; }
    .legenda li { display: flex; align-items: center; gap: 0.4rem; }
    .legenda i { width: 12px; height: 12px; border-radius: 3px; display: inline-block; }
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
// Linha
// ---------------------------------------------------------------------------
@Component({
  selector: 'an-line-chart',
  standalone: true,
  template: `
    @if (dados.length < 2) {
      <p class="vazio-graf">Dados insuficientes para a linha.</p>
    } @else {
      <svg viewBox="0 0 600 250" class="graf" preserveAspectRatio="xMidYMid meet">
        <polyline [attr.points]="pontos()" fill="none" [attr.stroke]="cor" stroke-width="2.5" />
        @for (p of vertices(); track p.label + $index) {
          <circle [attr.cx]="p.x" [attr.cy]="p.y" r="3" [attr.fill]="cor"><title>{{ p.label }}: {{ p.valor }}</title></circle>
          <text [attr.x]="p.x" y="244" text-anchor="middle" class="lbl">{{ p.label }}</text>
        }
      </svg>
    }
  `,
  styles: [`
    .graf { width: 100%; height: auto; }
    .lbl { font-size: 9px; fill: var(--an-texto-secundario); }
    .vazio-graf { color: var(--an-texto-secundario); font-size: 0.85rem; text-align: center; padding: 1.5rem 0; }
  `],
})
export class LineChartComponent {
  @Input() dados: ChartPoint[] = [];
  @Input() cor = PALETA[1];

  vertices() {
    const padX = 30, top = 14, bottom = 30, w = 600, h = 250;
    const innerW = w - padX * 2;
    const innerH = h - top - bottom;
    const n = this.dados.length;
    const max = Math.max(1, ...this.dados.map((d) => d.valor));
    const min = Math.min(...this.dados.map((d) => d.valor), 0);
    const span = max - min || 1;
    return this.dados.map((d, i) => ({
      label: d.label,
      valor: d.valor,
      x: padX + (innerW * i) / (n - 1),
      y: top + innerH - ((d.valor - min) / span) * innerH,
    }));
  }

  pontos(): string {
    return this.vertices().map((p) => `${p.x},${p.y}`).join(' ');
  }
}
