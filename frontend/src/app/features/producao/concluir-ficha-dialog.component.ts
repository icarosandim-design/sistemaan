import { Component, Inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatRadioModule } from '@angular/material/radio';
import { MatIconModule } from '@angular/material/icon';
import { fmtPeso, FichaMock, StatusFichaMock } from './producao.mock';

export interface ConcluirFichaResult {
  status: StatusFichaMock;
  pacotesFeitos: number | null;
  motivo: string | null;
}

/** ⚠️ PROTÓTIPO/MOCK: concluir ficha (feito X de Y) ou marcar não feita com motivo. */
@Component({
  selector: 'app-concluir-ficha-dialog',
  standalone: true,
  imports: [FormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatRadioModule, MatIconModule],
  template: `
    <h2 mat-dialog-title>{{ f.pet || f.receitaNome }} <span class="cod">{{ f.receitaCodigo }}</span></h2>
    <mat-dialog-content>
      <p class="plan">Planejado: <strong>{{ f.pacotes }}</strong> pacotes de {{ fmtPeso(f.pesoPacoteGramas) }}</p>

      <mat-radio-group [(ngModel)]="status" class="radios">
        <mat-radio-button value="Concluida">Concluída</mat-radio-button>
        <mat-radio-button value="NaoFeita">Não feita</mat-radio-button>
      </mat-radio-group>

      @if (status === 'Concluida') {
        <mat-form-field appearance="outline" class="full">
          <mat-label>Pacotes realmente feitos</mat-label>
          <input matInput type="number" [(ngModel)]="pacotesFeitos" [max]="f.pacotes" />
          <span matTextSuffix>de {{ f.pacotes }}</span>
        </mat-form-field>
        @if (pacotesFeitos !== null && pacotesFeitos < f.pacotes) {
          <p class="aviso"><mat-icon>info</mat-icon> Parcial: {{ pacotesFeitos }} de {{ f.pacotes }}.</p>
        }
        @if (f.tipo === 'Casa') {
          <p class="estoque"><mat-icon>inventory_2</mat-icon> Entrará no estoque: <strong>{{ pacotesFeitos ?? f.pacotes }}</strong> pacote(s) de {{ f.receitaNome }}.</p>
        } @else {
          <p class="estoque"><mat-icon>check_circle</mat-icon> A entrega ficará <strong>{{ (pacotesFeitos ?? f.pacotes) < f.pacotes ? 'parcialmente pronta' : 'pronta' }}</strong>.</p>
        }
      } @else if (status === 'NaoFeita') {
        <mat-form-field appearance="outline" class="full">
          <mat-label>Motivo (não feita)</mat-label>
          <textarea matInput rows="2" [(ngModel)]="motivo" placeholder="Ex.: faltou insumo, não deu tempo…"></textarea>
        </mat-form-field>
        @if (f.tipo === 'Personalizada') {
          <p class="aviso"><mat-icon>undo</mat-icon> A receita volta para <strong>Não pronta</strong>.</p>
        }
      }
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="ref.close()">Cancelar</button>
      <button mat-flat-button class="btn-cta" [disabled]="!status" (click)="confirmar()"><mat-icon>check</mat-icon> Confirmar</button>
    </mat-dialog-actions>
  `,
  styles: [`
    .cod { font-size: 0.85rem; color: var(--an-texto-secundario); }
    .plan { margin: 0 0 0.6rem; color: var(--an-texto-secundario); }
    .radios { display: flex; gap: 1.2rem; margin-bottom: 0.75rem; }
    .full { width: 100%; }
    .aviso, .estoque { display: flex; align-items: center; gap: 0.4rem; font-size: 0.85rem; margin: 0.25rem 0 0; color: var(--an-texto-secundario); }
    .estoque .mat-icon, .aviso .mat-icon { font-size: 1.05rem; width: 1.05rem; height: 1.05rem; color: var(--an-cta); }
    .btn-cta { background: var(--an-cta); color: #fff; }
    mat-dialog-content { min-width: 400px; }
    @media (max-width: 440px) { mat-dialog-content { min-width: auto; } }
  `],
})
export class ConcluirFichaDialogComponent {
  readonly fmtPeso = fmtPeso;
  readonly f: FichaMock;
  status: StatusFichaMock;
  pacotesFeitos: number | null;
  motivo = '';

  constructor(
    readonly ref: MatDialogRef<ConcluirFichaDialogComponent, ConcluirFichaResult>,
    @Inject(MAT_DIALOG_DATA) data: { ficha: FichaMock; modoInicial: StatusFichaMock },
  ) {
    this.f = data.ficha;
    this.status = data.modoInicial;
    this.pacotesFeitos = data.ficha.pacotes;
  }

  confirmar(): void {
    if (this.status === 'Concluida') {
      this.ref.close({ status: 'Concluida', pacotesFeitos: this.pacotesFeitos, motivo: null });
    } else {
      this.ref.close({ status: 'NaoFeita', pacotesFeitos: null, motivo: this.motivo.trim() || null });
    }
  }
}
