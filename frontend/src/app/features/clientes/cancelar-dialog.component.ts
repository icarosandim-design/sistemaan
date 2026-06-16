import { Component, Inject, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MotivosCancelamentoService } from '../motivos-cancelamento/motivos-cancelamento.service';
import { MotivoCancelamento } from '../motivos-cancelamento/motivos-cancelamento.model';

export interface CancelarDialogData {
  nome: string;
}

export interface CancelarDialogResult {
  motivoId: number;
  motivo: string;
  observacao: string | null;
}

@Component({
  selector: 'app-cancelar-dialog',
  standalone: true,
  imports: [FormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatButtonModule],
  template: `
    <h2 mat-dialog-title>Cancelar cliente</h2>
    <mat-dialog-content>
      <p class="msg">Cancelar <strong>{{ data.nome }}</strong>? Selecione o motivo.</p>
      <mat-form-field appearance="outline" class="full">
        <mat-label>Motivo do cancelamento</mat-label>
        <mat-select [(ngModel)]="motivoId">
          @for (m of motivos; track m.id) {
            <mat-option [value]="m.id">{{ m.nome }}</mat-option>
          }
        </mat-select>
      </mat-form-field>
      <mat-form-field appearance="outline" class="full">
        <mat-label>Observação complementar (opcional)</mat-label>
        <textarea matInput rows="3" [(ngModel)]="observacao"></textarea>
      </mat-form-field>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="ref.close()">Voltar</button>
      <button mat-flat-button class="btn-perigo" [disabled]="!motivoId" (click)="confirmar()">
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
export class CancelarDialogComponent implements OnInit {
  private readonly service = inject(MotivosCancelamentoService);
  motivos: MotivoCancelamento[] = [];
  motivoId: number | null = null;
  observacao = '';

  constructor(
    public ref: MatDialogRef<CancelarDialogComponent, CancelarDialogResult>,
    @Inject(MAT_DIALOG_DATA) public data: CancelarDialogData,
  ) {}

  ngOnInit(): void {
    this.service.listar().subscribe({ next: (ms) => (this.motivos = ms) });
  }

  confirmar(): void {
    if (!this.motivoId) return;
    const m = this.motivos.find((x) => x.id === this.motivoId);
    this.ref.close({
      motivoId: this.motivoId,
      motivo: m?.nome ?? '',
      observacao: this.observacao.trim() || null,
    });
  }
}
