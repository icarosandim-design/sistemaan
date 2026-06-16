import { Component, Inject, inject } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { Doenca, SalvarDoencaRequest } from './doencas.model';

export interface DoencaDialogData {
  doenca: Doenca | null;
}

@Component({
  selector: 'app-doenca-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatSlideToggleModule],
  template: `
    <h2 mat-dialog-title>{{ edicao ? 'Editar doença' : 'Nova doença' }}</h2>
    <mat-dialog-content>
      <form [formGroup]="form" class="form">
        <mat-form-field appearance="outline" class="full">
          <mat-label>Nome</mat-label>
          <input matInput formControlName="nome" placeholder="Ex.: Diabetes, Alergia alimentar..." />
          @if (form.controls.nome.hasError('required')) { <mat-error>Informe o nome.</mat-error> }
        </mat-form-field>
        <mat-form-field appearance="outline" class="full">
          <mat-label>Ordem de exibição</mat-label>
          <input matInput type="number" step="1" formControlName="ordem" />
        </mat-form-field>
        <mat-slide-toggle formControlName="ativo" color="primary">Ativa</mat-slide-toggle>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="cancelar()">Cancelar</button>
      <button mat-flat-button class="btn-cta" (click)="salvar()">{{ edicao ? 'Salvar' : 'Cadastrar' }}</button>
    </mat-dialog-actions>
  `,
  styles: [`
    .form { display: flex; flex-direction: column; gap: 0.25rem; padding-top: 0.5rem; }
    .full { width: 100%; min-width: 340px; }
    .btn-cta { background: var(--an-cta); color: #fff; }
  `],
})
export class DoencaDialogComponent {
  private readonly fb = inject(FormBuilder);
  readonly edicao: boolean;

  readonly form = this.fb.nonNullable.group({
    nome: ['', [Validators.required]],
    ordem: [0],
    ativo: [true],
  });

  constructor(
    private readonly ref: MatDialogRef<DoencaDialogComponent, SalvarDoencaRequest>,
    @Inject(MAT_DIALOG_DATA) readonly data: DoencaDialogData,
  ) {
    this.edicao = !!data.doenca;
    if (data.doenca) {
      const d = data.doenca;
      this.form.patchValue({ nome: d.nome, ordem: d.ordem, ativo: d.ativo });
    }
  }

  salvar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const v = this.form.getRawValue();
    this.ref.close({ nome: v.nome.trim(), ordem: Number(v.ordem) || 0, ativo: v.ativo });
  }

  cancelar(): void {
    this.ref.close();
  }
}
