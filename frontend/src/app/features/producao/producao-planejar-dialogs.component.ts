import { Component, Inject, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { fmtPeso } from './producao.mock';
import { ProducaoMockService } from './producao-mock.service';

function paraBr(iso: string): string {
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
      <p class="info">{{ data.qtd }} receita(s) personalizada(s) selecionada(s) entrarão nesta produção.</p>
      <mat-form-field appearance="outline" class="full">
        <mat-label>Dia da produção</mat-label>
        <input matInput type="date" [(ngModel)]="dataIso" />
      </mat-form-field>
      <p class="nota">As receitas selecionadas passarão de <strong>Não pronta</strong> para <strong>Planejada</strong> neste dia.</p>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="ref.close()">Cancelar</button>
      <button mat-flat-button class="btn-cta" [disabled]="!dataIso" (click)="confirmar()"><mat-icon>event_available</mat-icon> Planejar</button>
    </mat-dialog-actions>
  `,
  styles: [`
    .info { margin: 0 0 0.75rem; font-weight: 600; color: var(--an-texto-titulo); }
    .full { width: 100%; }
    .nota { margin: 0.25rem 0 0; font-size: 0.8rem; color: var(--an-texto-secundario); }
    .btn-cta { background: var(--an-cta); color: #fff; }
    mat-dialog-content { min-width: 380px; }
    @media (max-width: 440px) { mat-dialog-content { min-width: auto; } }
  `],
})
export class DataProducaoDialogComponent {
  dataIso = '';
  constructor(
    readonly ref: MatDialogRef<DataProducaoDialogComponent, string>,
    @Inject(MAT_DIALOG_DATA) readonly data: { qtd: number },
  ) {}
  confirmar(): void {
    if (this.dataIso) {
      this.ref.close(paraBr(this.dataIso));
    }
  }
}

// ===================== Editar uma produção planejada =====================
@Component({
  selector: 'app-editar-producao-dialog',
  standalone: true,
  imports: [MatDialogModule, MatButtonModule, MatIconModule],
  template: `
    <h2 mat-dialog-title>Produção planejada — {{ data.dia }}</h2>
    <mat-dialog-content>
      <h3 class="secao">Receitas nesta produção ({{ itens().length }})</h3>
      @for (p of itens(); track p.id) {
        <div class="linha">
          <div>
            <span class="pet">{{ p.pet }}</span> <span class="cod">{{ p.receitaCodigo }}</span>
            <span class="meta">{{ p.cliente }} · {{ p.pacotes }} pacotes de {{ fmtPeso(p.pesoPacoteGramas) }}</span>
          </div>
          <button mat-stroked-button class="btn-rem" (click)="mock.removerDaProducao(p.id)"><mat-icon>close</mat-icon> Remover</button>
        </div>
      } @empty {
        <p class="vazio">Nenhuma receita nesta produção.</p>
      }

      <h3 class="secao">Adicionar receitas disponíveis ({{ mock.disponiveis().length }})</h3>
      @for (p of mock.disponiveis(); track p.id) {
        <div class="linha disp">
          <div>
            <span class="pet">{{ p.pet }}</span> <span class="cod">{{ p.receitaCodigo }}</span>
            <span class="meta">{{ p.cliente }} · entrega {{ p.dataEntrega }} · {{ p.pacotes }} pacotes de {{ fmtPeso(p.pesoPacoteGramas) }}</span>
          </div>
          <button mat-flat-button class="btn-add" (click)="mock.adicionarNaProducao(p.id, data.dia)"><mat-icon>add</mat-icon> Adicionar</button>
        </div>
      } @empty {
        <p class="vazio">Não há receitas disponíveis para adicionar.</p>
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
  readonly mock = inject(ProducaoMockService);
  readonly fmtPeso = fmtPeso;

  constructor(
    readonly ref: MatDialogRef<EditarProducaoDialogComponent>,
    @Inject(MAT_DIALOG_DATA) readonly data: { dia: string },
  ) {}

  itens() {
    return this.mock.personalizadas().filter((p) => p.planejadaDia === this.data.dia);
  }
}
