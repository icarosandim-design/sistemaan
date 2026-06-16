import { Component, Inject, inject } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MotivoCancelamento, SalvarMotivoCancelamentoRequest } from './motivos-cancelamento.model';

export interface MotivoCancelamentoDialogData {
  motivo: MotivoCancelamento | null;
}

@Component({
  selector: 'app-motivo-cancelamento-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatSlideToggleModule],
  template: `
    <h2 mat-dialog-title>{{ edicao ? 'Editar motivo' : 'Novo motivo de cancelamento' }}</h2>
    <mat-dialog-content>
      <form [formGroup]="form" class="form">
        <mat-form-field appearance="outline" class="full">
          <mat-label>Nome</mat-label>
          <input matInput formControlName="nome" placeholder="Ex.: Preço, Mudança de cidade..." />
          @if (form.controls.nome.hasError('required')) { <mat-error>Informe o nome.</mat-error> }
        </mat-form-field>
        <mat-form-field appearance="outline" class="full">
          <mat-label>Ordem de exibição</mat-label>
          <input matInput type="number" step="1" formControlName="ordem" />
        </mat-form-field>
        <mat-form-field appearance="outline" class="full">
          <mat-label>Observações (opcional)</mat-label>
          <textarea matInput rows="2" formControlName="observacoes"></textarea>
        </mat-form-field>
        <mat-slide-toggle formControlName="ativo" color="primary">Ativo</mat-slide-toggle>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="cancelar()">Cancelar</button>
      <button mat-flat-button class="btn-cta" (click)="salvar()">{{ edicao ? 'Salvar' : 'Cadastrar' }}</button>
    </mat-dialog-actions>
  `,
  styles: [`
    .form { display: flex; flex-direction: column; gap: 0.25rem; padding-top: 0.5rem; }
    .full { width: 100%; min-width: 360px; }
    .btn-cta { background: var(--an-cta); color: #fff; }
  `],
})
export class MotivoCancelamentoDialogComponent {
  private readonly fb = inject(FormBuilder);
  readonly edicao: boolean;

  readonly form = this.fb.nonNullable.group({
    nome: ['', [Validators.required]],
    ordem: [0],
    ativo: [true],
    observacoes: [''],
  });

  constructor(
    private readonly ref: MatDialogRef<MotivoCancelamentoDialogComponent, SalvarMotivoCancelamentoRequest>,
    @Inject(MAT_DIALOG_DATA) readonly data: MotivoCancelamentoDialogData,
  ) {
    this.edicao = !!data.motivo;
    if (data.motivo) {
      const m = data.motivo;
      this.form.patchValue({ nome: m.nome, ordem: m.ordem, ativo: m.ativo, observacoes: m.observacoes ?? '' });
    }
  }

  salvar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const v = this.form.getRawValue();
    this.ref.close({
      nome: v.nome.trim(),
      ordem: Number(v.ordem) || 0,
      ativo: v.ativo,
      observacoes: v.observacoes.trim() || null,
    });
  }

  cancelar(): void {
    this.ref.close();
  }
}
