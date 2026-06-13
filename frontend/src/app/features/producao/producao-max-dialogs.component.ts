import { Component, Inject, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialog, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { fmtPeso, FichaMock, IngredienteConsolidadoMock } from './producao.mock';
import { ProducaoMockService } from './producao-mock.service';

export const FULLSCREEN = { width: '100vw', maxWidth: '100vw', height: '100vh', maxHeight: '100vh', panelClass: 'prod-max-dialog', autoFocus: false };

const ORDEM_CATS = ['Proteínas', 'Carboidratos', 'Legumes', 'Temperos', 'Óleos', 'Suplementos', 'Outros'];

// ===================== Ingredientes (tela cheia) =====================
@Component({
  selector: 'app-ingredientes-max',
  standalone: true,
  imports: [MatDialogModule, MatButtonModule, MatIconModule],
  template: `
    <div class="max-head">
      <h2><mat-icon>inventory_2</mat-icon> Lista de ingredientes — preparo</h2>
      <button mat-icon-button (click)="ref.close()"><mat-icon>close</mat-icon></button>
    </div>
    <div class="max-body">
      <div class="thead"><span>Ingrediente</span><span class="t-r">Cozido</span><span class="t-r">Cru estimado</span></div>
      @for (g of grupos(); track g.categoria) {
        <div class="grupo">{{ g.categoria }}</div>
        @for (i of g.itens; track i.nome) {
          <div class="trow"><span class="ing">{{ i.nome }}</span><span class="t-r">{{ fmtPeso(i.cozidoGramas) }}</span><span class="t-r">{{ fmtPeso(i.cruGramas) }}</span></div>
        }
      }
    </div>
  `,
  styles: [`
    :host { display: flex; flex-direction: column; height: 100%; }
    .max-head { display: flex; align-items: center; justify-content: space-between; padding: 0.8rem 1.2rem; border-bottom: 1px solid var(--an-fundo-secundario); }
    .max-head h2 { display: flex; align-items: center; gap: 0.5rem; margin: 0; font-size: 1.4rem; .mat-icon { color: var(--an-cta); } }
    .max-body { flex: 1; overflow-y: auto; padding: 0.5rem 1.2rem 1.2rem; }
    .thead, .trow { display: grid; grid-template-columns: 2fr 1fr 1fr; gap: 1rem; align-items: center; }
    .thead { position: sticky; top: 0; background: var(--an-superficie); font-size: 0.85rem; font-weight: 700; text-transform: uppercase; color: var(--an-texto-secundario); padding: 0.5rem 0.3rem; border-bottom: 2px solid var(--an-fundo-secundario); }
    .trow { font-size: 1.3rem; padding: 0.5rem 0.3rem; border-bottom: 1px dashed var(--an-fundo-secundario); }
    .grupo { font-size: 1rem; font-weight: 800; text-transform: uppercase; color: var(--an-cta); background: var(--an-fundo); padding: 0.4rem 0.4rem; margin-top: 0.5rem; position: sticky; top: 38px; }
    .ing { font-weight: 700; color: var(--an-texto-titulo); }
    .t-r { text-align: right; font-variant-numeric: tabular-nums; }
  `],
})
export class IngredientesMaxComponent {
  private readonly mock = inject(ProducaoMockService);
  readonly fmtPeso = fmtPeso;
  constructor(readonly ref: MatDialogRef<IngredientesMaxComponent>) {}
  grupos(): { categoria: string; itens: IngredienteConsolidadoMock[] }[] {
    const map = new Map<string, IngredienteConsolidadoMock[]>();
    for (const i of this.mock.consolidado()) {
      const l = map.get(i.categoria) ?? [];
      l.push(i);
      map.set(i.categoria, l);
    }
    const ord = ORDEM_CATS.filter((c) => map.has(c));
    const extras = [...map.keys()].filter((c) => !ORDEM_CATS.includes(c));
    return [...ord, ...extras].map((c) => ({ categoria: c, itens: map.get(c)! }));
  }
}

// ===================== Uma ficha (tela cheia) =====================
@Component({
  selector: 'app-ficha-max',
  standalone: true,
  imports: [MatDialogModule, MatButtonModule, MatIconModule],
  template: `
    <div class="max-head">
      <h2>{{ f.pet || f.receitaNome }} <span class="cod">{{ f.receitaCodigo }}</span></h2>
      <button mat-icon-button (click)="ref.close()"><mat-icon>close</mat-icon></button>
    </div>
    <div class="max-body">
      @if (f.cliente) { <div class="sub">{{ f.cliente }} · entrega {{ f.dataEntrega }}</div> }
      <div class="bacia">Total da bacia: <strong>{{ fmtPeso(f.totalBaciaGramas) }}</strong> · {{ f.pacotes }} pacotes de {{ fmtPeso(f.pesoPacoteGramas) }}</div>
      <h3>Ingredientes</h3>
      <ul class="ing">
        @for (ing of f.ingredientes; track ing.nome) { <li><span>{{ ing.nome }}</span><strong>{{ fmtPeso(ing.gramas) }}</strong></li> }
      </ul>
      @if (f.observacoes) { <div class="obs"><mat-icon>info</mat-icon> {{ f.observacoes }}</div> }
    </div>
  `,
  styles: [`
    :host { display: flex; flex-direction: column; height: 100%; }
    .max-head { display: flex; align-items: center; justify-content: space-between; padding: 0.8rem 1.2rem; border-bottom: 1px solid var(--an-fundo-secundario); }
    .max-head h2 { margin: 0; font-size: 2rem; color: var(--an-texto-titulo); }
    .cod { font-size: 1rem; color: var(--an-texto-secundario); margin-left: 0.5rem; }
    .max-body { flex: 1; overflow-y: auto; padding: 1rem 1.4rem; }
    .sub { font-size: 1.1rem; color: var(--an-texto-secundario); }
    .bacia { font-size: 1.3rem; margin: 0.6rem 0 1rem; color: var(--an-texto-titulo); }
    h3 { font-size: 1rem; text-transform: uppercase; color: var(--an-texto-secundario); margin: 0 0 0.5rem; }
    .ing { list-style: none; margin: 0; padding: 0; }
    .ing li { display: flex; justify-content: space-between; font-size: 1.6rem; padding: 0.6rem 0; border-bottom: 1px dashed var(--an-fundo-secundario); }
    .ing strong { color: var(--an-primaria); }
    .obs { display: flex; align-items: center; gap: 0.4rem; margin-top: 1rem; font-size: 1.1rem; color: #8c6b3f; }
  `],
})
export class FichaMaxComponent {
  readonly fmtPeso = fmtPeso;
  readonly f: FichaMock;
  constructor(readonly ref: MatDialogRef<FichaMaxComponent>, @Inject(MAT_DIALOG_DATA) data: { ficha: FichaMock }) {
    this.f = data.ficha;
  }
}

// ===================== Todas as fichas (tela cheia) =====================
@Component({
  selector: 'app-fichas-max',
  standalone: true,
  imports: [MatDialogModule, MatButtonModule, MatIconModule],
  template: `
    <div class="max-head">
      <h2><mat-icon>description</mat-icon> Fichas técnicas ({{ mock.fichas().length }})</h2>
      <button mat-icon-button (click)="ref.close()"><mat-icon>close</mat-icon></button>
    </div>
    <div class="max-body">
      <div class="grid">
        @for (f of mock.fichas(); track f.id) {
          <article class="ficha" [class.casa]="f.tipo === 'Casa'" (click)="abrir(f)">
            <div class="fh"><span class="t">{{ f.pet || f.receitaNome }}</span><span class="cod">{{ f.receitaCodigo }}</span><mat-icon class="exp">open_in_full</mat-icon></div>
            <div class="meta">Bacia {{ fmtPeso(f.totalBaciaGramas) }} · {{ f.pacotes }} × {{ fmtPeso(f.pesoPacoteGramas) }}</div>
            <ul class="ing">@for (ing of f.ingredientes; track ing.nome) { <li>{{ ing.nome }} <strong>{{ fmtPeso(ing.gramas) }}</strong></li> }</ul>
          </article>
        }
      </div>
    </div>
  `,
  styles: [`
    :host { display: flex; flex-direction: column; height: 100%; }
    .max-head { display: flex; align-items: center; justify-content: space-between; padding: 0.8rem 1.2rem; border-bottom: 1px solid var(--an-fundo-secundario); }
    .max-head h2 { display: flex; align-items: center; gap: 0.5rem; margin: 0; font-size: 1.4rem; .mat-icon { color: var(--an-cta); } }
    .max-body { flex: 1; overflow-y: auto; padding: 1rem 1.2rem; }
    .grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(320px, 1fr)); gap: 0.8rem; }
    .ficha { border: 1px solid var(--an-fundo-secundario); border-radius: 12px; padding: 0.9rem 1rem; cursor: pointer; transition: box-shadow .12s; }
    .ficha:hover { box-shadow: 0 2px 10px rgba(0,0,0,0.08); }
    .ficha.casa { box-shadow: inset 4px 0 0 #8c6b3f; }
    .fh { display: flex; align-items: center; gap: 0.5rem; }
    .fh .t { font-weight: 700; font-size: 1.2rem; color: var(--an-texto-titulo); }
    .fh .cod { font-size: 0.8rem; color: var(--an-texto-secundario); }
    .fh .exp { margin-left: auto; color: var(--an-texto-secundario); }
    .meta { font-size: 0.95rem; color: var(--an-texto-secundario); margin: 0.3rem 0; }
    .ing { list-style: none; margin: 0.3rem 0 0; padding: 0; font-size: 1rem; }
    .ing li { display: flex; justify-content: space-between; padding: 0.15rem 0; color: var(--an-texto-secundario); strong { color: var(--an-texto-titulo); } }
  `],
})
export class FichasMaxComponent {
  readonly mock = inject(ProducaoMockService);
  private readonly dialog = inject(MatDialog);
  readonly fmtPeso = fmtPeso;
  constructor(readonly ref: MatDialogRef<FichasMaxComponent>) {}
  abrir(ficha: FichaMock): void {
    this.dialog.open(FichaMaxComponent, { ...FULLSCREEN, data: { ficha } });
  }
}
