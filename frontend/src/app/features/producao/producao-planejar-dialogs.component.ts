import { Component, Inject, computed, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';
import { fmtMoeda, fmtPeso } from './producao.model';
import { ProducaoStore } from './producao.store';

function fmtData(iso: string): string {
  if (!iso) return '—';
  const [y, m, d] = iso.split('-');
  return `${d}/${m}/${y}`;
}

// ===================== Escolher o dia da produção =====================
@Component({
  selector: 'app-data-producao-dialog',
  standalone: true,
  imports: [FormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatIconModule],
  template: `
    <h2 mat-dialog-title>Planejar produção</h2>
    <mat-dialog-content>
      <p class="info">
        {{ data.qtd }} personalizada(s)@if (data.qtdCasa) { · {{ data.qtdCasa }} da casa } entrarão nesta produção.
      </p>
      <div class="custo"><mat-icon>payments</mat-icon> Custo estimado: <strong>~{{ fmtMoeda(data.custo) }}</strong> <span class="obs">(visualização)</span></div>
      <mat-form-field appearance="outline" class="full">
        <mat-label>Dia da produção</mat-label>
        <input matInput type="date" [(ngModel)]="dataIso" />
      </mat-form-field>
      <p class="nota">Se já existir uma produção neste dia, as receitas serão adicionadas a ela.</p>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="ref.close()">Cancelar</button>
      <button mat-flat-button class="btn-cta" [disabled]="!dataIso" (click)="confirmar()"><mat-icon>event_available</mat-icon> Planejar</button>
    </mat-dialog-actions>
  `,
  styles: [`
    .info { margin: 0 0 0.5rem; font-weight: 600; color: var(--an-texto-titulo); }
    .custo { display: flex; align-items: center; gap: 0.4rem; margin: 0 0 0.75rem; font-size: 0.9rem; color: var(--an-texto-secundario);
      strong { color: var(--an-primaria); } .obs { font-size: 0.75rem; font-style: italic; } .mat-icon { color: var(--an-cta); font-size: 1.15rem; width: 1.15rem; height: 1.15rem; } }
    .full { width: 100%; }
    .nota { margin: 0.25rem 0 0; font-size: 0.8rem; color: var(--an-texto-secundario); }
    .btn-cta { background: var(--an-cta); color: #fff; }
    mat-dialog-content { min-width: 380px; }
    @media (max-width: 440px) { mat-dialog-content { min-width: auto; } }
  `],
})
export class DataProducaoDialogComponent {
  dataIso = '';
  readonly fmtMoeda = fmtMoeda;
  constructor(
    readonly ref: MatDialogRef<DataProducaoDialogComponent, string>,
    @Inject(MAT_DIALOG_DATA) readonly data: { qtd: number; qtdCasa: number; custo: number },
  ) {}
  confirmar(): void {
    if (this.dataIso) {
      this.ref.close(this.dataIso);
    }
  }
}

// ===================== Editar uma produção planejada =====================
@Component({
  selector: 'app-editar-producao-dialog',
  standalone: true,
  imports: [MatDialogModule, MatButtonModule, MatIconModule],
  template: `
    <h2 mat-dialog-title>Produção planejada — {{ ordem() ? fmtData(ordem()!.data) : '—' }}</h2>
    <mat-dialog-content>
      <h3 class="secao">Receitas nesta produção ({{ fichas().length }})</h3>
      @for (f of fichas(); track f.id) {
        <div class="linha">
          <div>
            <span class="pet">{{ f.tipo === 'Casa' ? f.receitaNome : f.petNome }}</span> <span class="cod">{{ f.receitaCodigo }}</span>
            <span class="meta">{{ f.tipo === 'Casa' ? 'Receita da Casa' : f.clienteNome }} · {{ f.quantidadePacotes }} pacotes de {{ fmtPeso(f.pesoPacoteGramas) }}</span>
          </div>
          <button mat-stroked-button class="btn-rem" (click)="remover(f.id)"><mat-icon>close</mat-icon> Remover</button>
        </div>
      } @empty {
        <p class="vazio">Nenhuma receita nesta produção.</p>
      }

      <h3 class="secao">Adicionar personalizadas disponíveis ({{ disponiveis().length }})</h3>
      @for (p of disponiveis(); track p.entregaItemId) {
        <div class="linha disp">
          <div>
            <span class="pet">{{ p.petNome }}</span> <span class="cod">{{ p.receitaCodigo }}</span>
            <span class="meta">{{ p.clienteNome }} · entrega {{ fmtData(p.dataEntrega) }} · {{ p.pacotes }} pacotes de {{ fmtPeso(p.pesoPacoteGramas) }}</span>
          </div>
          <button mat-flat-button class="btn-add" (click)="adicionar(p.entregaItemId)"><mat-icon>add</mat-icon> Adicionar</button>
        </div>
      } @empty {
        <p class="vazio">Não há personalizadas disponíveis para adicionar.</p>
      }
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-flat-button class="btn-cta" (click)="ref.close()">Concluir</button>
    </mat-dialog-actions>
  `,
  styles: [`
    .secao { margin: 0.75rem 0 0.4rem; font-size: 0.78rem; font-weight: 800; text-transform: uppercase; color: var(--an-texto-secundario); }
    .linha { display: flex; align-items: center; justify-content: space-between; gap: 0.75rem; padding: 0.45rem 0; border-bottom: 1px dashed var(--an-fundo-secundario); }
    .linha.disp { opacity: 0.95; }
    .pet { font-weight: 700; color: var(--an-texto-titulo); }
    .cod { font-size: 0.75rem; color: var(--an-texto-secundario); }
    .meta { display: block; font-size: 0.8rem; color: var(--an-texto-secundario); }
    .btn-rem { color: #b3261e; }
    .btn-add { background: var(--an-cta); color: #fff; }
    .vazio { color: var(--an-texto-secundario); font-size: 0.85rem; padding: 0.4rem 0; }
    .btn-cta { background: var(--an-cta); color: #fff; }
    mat-dialog-content { min-width: 520px; }
    @media (max-width: 560px) { mat-dialog-content { min-width: auto; } }
  `],
})
export class EditarProducaoDialogComponent {
  readonly store = inject(ProducaoStore);
  private readonly snack = inject(MatSnackBar);
  readonly fmtPeso = fmtPeso;
  readonly fmtData = fmtData;

  readonly ordem = computed(() => this.store.ordensPlanejadas().find((o) => o.id === this.data.ordemId) ?? null);
  readonly fichas = computed(() => this.ordem()?.fichas ?? []);
  readonly disponiveis = computed(() =>
    this.store.demanda().personalizadas.filter((p) => !this.store.planejadosIds().has(p.entregaItemId)),
  );

  constructor(
    readonly ref: MatDialogRef<EditarProducaoDialogComponent>,
    @Inject(MAT_DIALOG_DATA) readonly data: { ordemId: number },
  ) {}

  async adicionar(entregaItemId: number): Promise<void> {
    try {
      await this.store.adicionarNaProducao(this.data.ordemId, entregaItemId);
    } catch {
      this.snack.open('Não foi possível adicionar.', 'OK', { duration: 3000 });
    }
  }

  async remover(fichaId: number): Promise<void> {
    try {
      await this.store.removerFicha(fichaId);
    } catch {
      this.snack.open('Não foi possível remover.', 'OK', { duration: 3000 });
    }
  }
}
