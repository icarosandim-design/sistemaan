import { Component, Inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';

export interface MotivoDialogData {
  titulo: string;
  confirmar: string;
  motivos?: string[];
}

@Component({
  selector: 'app-motivo-dialog',
  standalone: true,
  imports: [FormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatButtonModule],
  template: `
    <h2 mat-dialog-title>{{ data.titulo }}</h2>
    <mat-dialog-content>
      <div class="form">
        @if (data.motivos?.length) {
          <mat-form-field appearance="outline" class="full">
            <mat-label>Motivo</mat-label>
            <mat-select [(ngModel)]="selecionado" (ngModelChange)="aoSelecionar()">
              @for (m of data.motivos; track m) {
                <mat-option [value]="m">{{ m }}</mat-option>
              }
            </mat-select>
          </mat-form-field>
        }
        <mat-form-field appearance="outline" class="full">
          <mat-label>{{ data.motivos?.length ? 'Detalhe / observação' : 'Motivo' }}</mat-label>
          <textarea matInput rows="2" [(ngModel)]="motivo"></textarea>
        </mat-form-field>
      </div>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="ref.close()">Cancelar</button>
      <button mat-flat-button class="btn-cta" [disabled]="!motivo.trim()" (click)="confirmar()">
        {{ data.confirmar }}
      </button>
    </mat-dialog-actions>
  `,
  styles: [
    `.form { display: flex; flex-direction: column; gap: 0.25rem; padding-top: 0.75rem; min-width: 380px; }
     .full { width: 100%; }
     .btn-cta { background: var(--an-cta); color: #fff; }
     @media (max-width: 440px) { .form { min-width: auto; } }`,
  ],
})
export class MotivoDialogComponent {
  selecionado = '';
  motivo = '';

  constructor(
    readonly ref: MatDialogRef<MotivoDialogComponent, string>,
    @Inject(MAT_DIALOG_DATA) readonly data: MotivoDialogData,
  ) {}

  aoSelecionar(): void {
    this.motivo = this.selecionado === 'Outro' ? '' : this.selecionado;
  }

  confirmar(): void {
    const m = this.motivo.trim();
    if (m) {
      this.ref.close(m);
    }
  }
}
