import { Component, Inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';

export interface CancelarDialogData {
  nome: string;
}

@Component({
  selector: 'app-cancelar-dialog',
  standalone: true,
  imports: [FormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatButtonModule],
  template: `
    <h2 mat-dialog-title>Cancelar cliente</h2>
    <mat-dialog-content>
      <p class="msg">Cancelar <strong>{{ data.nome }}</strong>? Informe o motivo.</p>
      <mat-form-field appearance="outline" class="full">
        <mat-label>Motivo do cancelamento</mat-label>
        <textarea matInput rows="3" [(ngModel)]="motivo"></textarea>
      </mat-form-field>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="ref.close()">Voltar</button>
      <button mat-flat-button class="btn-perigo" [disabled]="!motivo.trim()" (click)="ref.close(motivo.trim())">
        Confirmar cancelamento
      </button>
    </mat-dialog-actions>
  `,
  styles: [
    `
      .msg { color: var(--an-texto-corpo); margin: 0.25rem 0 0.75rem; }
      .full { width: 100%; min-width: 360px; }
      .btn-perigo { background: #b3261e; color: #fff; }
    `,
  ],
})
export class CancelarDialogComponent {
  motivo = '';

  constructor(
    public ref: MatDialogRef<CancelarDialogComponent, string>,
    @Inject(MAT_DIALOG_DATA) public data: CancelarDialogData,
  ) {}
}
