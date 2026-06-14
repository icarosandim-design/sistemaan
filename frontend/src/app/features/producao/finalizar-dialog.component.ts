import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatRadioModule } from '@angular/material/radio';
import { MatIconModule } from '@angular/material/icon';

export interface FinalizarResult {
  tudoProduzido: boolean;
  observacoes: string | null;
}

/** Finalização do dia: confere produção e dispara baixa de insumos / entrada de produto acabado / prontidão. */
@Component({
  selector: 'app-producao-finalizar-dialog',
  standalone: true,
  imports: [FormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatRadioModule, MatIconModule],
  template: `
    <h2 mat-dialog-title>Finalizar produção do dia</h2>
    <mat-dialog-content>
      <p class="aviso"><mat-icon>warning</mat-icon> Esta ação é definitiva: dá baixa nos insumos (pelo cru informado), lança o produto acabado da Casa no estoque e marca a prontidão das entregas personalizadas.</p>

      <div class="campo">
        <label>Tudo que estava previsto foi produzido?</label>
        <mat-radio-group [(ngModel)]="tudoProduzido">
          <mat-radio-button [value]="true">Sim</mat-radio-button>
          <mat-radio-button [value]="false">Não</mat-radio-button>
        </mat-radio-group>
      </div>

      <mat-form-field appearance="outline" class="full">
        <mat-label>Observações da finalização</mat-label>
        <textarea matInput rows="3" [(ngModel)]="observacoes" placeholder="Ex.: sobrou comida, faltou insumo X, divergências…"></textarea>
      </mat-form-field>

      <p class="nota">Apenas fichas <strong>conferidas</strong> entram no estoque (Casa) ou marcam a entrega como pronta (personalizadas). Use a tela da Cozinha para concluir as fichas antes de finalizar.</p>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="ref.close()">Cancelar</button>
      <button mat-flat-button class="btn-cta" [disabled]="tudoProduzido === null" (click)="confirmar()"><mat-icon>check</mat-icon> Finalizar</button>
    </mat-dialog-actions>
  `,
  styles: [`
    .aviso { display: flex; align-items: flex-start; gap: 0.4rem; color: #8c6b3f; font-size: 0.85rem; margin: 0 0 0.75rem; }
    .aviso .mat-icon { font-size: 1.1rem; width: 1.1rem; height: 1.1rem; flex: none; margin-top: 0.1rem; }
    .campo { margin: 0.5rem 0; display: flex; flex-direction: column; gap: 0.3rem; }
    .campo label { font-size: 0.85rem; font-weight: 600; color: var(--an-texto-titulo); }
    mat-radio-group { display: flex; gap: 1rem; }
    .full { width: 100%; }
    .nota { margin: 0.5rem 0 0; font-size: 0.78rem; color: var(--an-texto-secundario); font-style: italic; }
    .btn-cta { background: var(--an-cta); color: #fff; }
    mat-dialog-content { min-width: 480px; }
    @media (max-width: 520px) { mat-dialog-content { min-width: auto; } }
  `],
})
export class ProducaoFinalizarDialogComponent {
  tudoProduzido: boolean | null = null;
  observacoes = '';

  constructor(readonly ref: MatDialogRef<ProducaoFinalizarDialogComponent, FinalizarResult>) {}

  confirmar(): void {
    if (this.tudoProduzido === null) return;
    this.ref.close({ tudoProduzido: this.tudoProduzido, observacoes: this.observacoes.trim() || null });
  }
}
