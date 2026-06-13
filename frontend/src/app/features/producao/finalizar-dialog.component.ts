import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatRadioModule } from '@angular/material/radio';
import { MatIconModule } from '@angular/material/icon';

/** ⚠️ PROTÓTIPO/MOCK: tela de finalização do dia. Não salva nada. */
@Component({
  selector: 'app-producao-finalizar-dialog',
  standalone: true,
  imports: [FormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatRadioModule, MatIconModule],
  template: `
    <h2 mat-dialog-title>Finalizar produção do dia</h2>
    <mat-dialog-content>
      <p class="aviso"><mat-icon>science</mat-icon> Protótipo — nenhuma baixa de estoque ou alteração real será feita.</p>

      <div class="campo">
        <label>Tudo que estava previsto foi produzido?</label>
        <mat-radio-group [(ngModel)]="tudoProduzido">
          <mat-radio-button [value]="true">Sim</mat-radio-button>
          <mat-radio-button [value]="false">Não</mat-radio-button>
        </mat-radio-group>
      </div>

      @if (tudoProduzido === false) {
        <mat-form-field appearance="outline" class="full">
          <mat-label>Quais receitas não foram feitas e por quê?</mat-label>
          <textarea matInput rows="2" [(ngModel)]="motivoNaoFeitas"></textarea>
        </mat-form-field>
      }

      <h3 class="secao">Pesagem real (por ingrediente — consolidado)</h3>
      <div class="grade">
        <mat-form-field appearance="outline"><mat-label>Peso cru usado (kg)</mat-label><input matInput type="number" [(ngModel)]="cru" /></mat-form-field>
        <mat-form-field appearance="outline"><mat-label>Peso cozido obtido (kg)</mat-label><input matInput type="number" [(ngModel)]="cozido" /></mat-form-field>
        <mat-form-field appearance="outline"><mat-label>Peso final envasado (kg)</mat-label><input matInput type="number" [(ngModel)]="envasado" /></mat-form-field>
        <mat-form-field appearance="outline"><mat-label>Pacotes produzidos</mat-label><input matInput type="number" [(ngModel)]="pacotes" /></mat-form-field>
      </div>

      <div class="campo">
        <label>Sobrou comida / houve perda?</label>
        <mat-radio-group [(ngModel)]="houvePerda">
          <mat-radio-button [value]="false">Não</mat-radio-button>
          <mat-radio-button [value]="true">Sim</mat-radio-button>
        </mat-radio-group>
      </div>

      <mat-form-field appearance="outline" class="full">
        <mat-label>Observações</mat-label>
        <textarea matInput rows="2" [(ngModel)]="observacoes"></textarea>
      </mat-form-field>

      <p class="nota">Na versão real: baixa dos insumos pelo peso cru informado, entrada do produto acabado (Casa) e marcação de prontidão das personalizadas.</p>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="ref.close()">Cancelar</button>
      <button mat-flat-button class="btn-cta" (click)="ref.close(true)"><mat-icon>check</mat-icon> Finalizar (mock)</button>
    </mat-dialog-actions>
  `,
  styles: [`
    .aviso { display: flex; align-items: center; gap: 0.4rem; color: #8c6b3f; font-size: 0.85rem; margin: 0 0 0.75rem; }
    .aviso .mat-icon { font-size: 1.1rem; width: 1.1rem; height: 1.1rem; }
    .campo { margin: 0.5rem 0; display: flex; flex-direction: column; gap: 0.3rem; }
    .campo label { font-size: 0.85rem; font-weight: 600; color: var(--an-texto-titulo); }
    mat-radio-group { display: flex; gap: 1rem; }
    .secao { margin: 0.75rem 0 0.25rem; font-size: 0.78rem; font-weight: 800; text-transform: uppercase; color: var(--an-texto-secundario); }
    .grade { display: grid; grid-template-columns: repeat(2, 1fr); gap: 0.25rem 0.75rem; }
    .full { width: 100%; }
    .nota { margin: 0.5rem 0 0; font-size: 0.78rem; color: var(--an-texto-secundario); font-style: italic; }
    .btn-cta { background: var(--an-cta); color: #fff; }
    mat-dialog-content { min-width: 520px; }
    @media (max-width: 560px) { mat-dialog-content { min-width: auto; } .grade { grid-template-columns: 1fr; } }
  `],
})
export class ProducaoFinalizarDialogComponent {
  tudoProduzido: boolean | null = null;
  motivoNaoFeitas = '';
  cru: number | null = null;
  cozido: number | null = null;
  envasado: number | null = null;
  pacotes: number | null = null;
  houvePerda: boolean | null = null;
  observacoes = '';

  constructor(readonly ref: MatDialogRef<ProducaoFinalizarDialogComponent, boolean>) {}
}
