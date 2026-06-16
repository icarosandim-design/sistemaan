import { Component, Inject, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { OrigemVenda, SalvarOrigemVendaRequest } from './origens-venda.model';

export interface OrigemVendaDialogData {
  origem: OrigemVenda | null;
}

@Component({
  selector: 'app-origem-venda-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatSlideToggleModule],
  template: `
    <h2 mat-dialog-title>{{ edicao ? 'Editar origem' : 'Nova origem de venda' }}</h2>
    <mat-dialog-content>
      <form [formGroup]="form" class="form">
        <mat-form-field appearance="outline" class="full">
          <mat-label>Nome</mat-label>
          <input matInput formControlName="nome" placeholder="Ex.: Instagram, Indicação..." />
          @if (form.controls.nome.hasError('required')) { <mat-error>Informe o nome.</mat-error> }
        </mat-form-field>
        <mat-form-field appearance="outline" class="full">
          <mat-label>Ordem de exibição</mat-label>
          <input matInput type="number" step="1" formControlName="ordem" />
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
    .form { display: flex; flex-direction: column; gap: 0.5rem; padding-top: 0.75rem; min-width: 360px; }
    .full { width: 100%; }
    .btn-cta { background: var(--an-cta); color: #fff; }
    @media (max-width: 420px) { .form { min-width: auto; } }
  `],
})
export class OrigemVendaDialogComponent {
  private readonly fb = inject(FormBuilder);
  readonly edicao: boolean;

  readonly form = this.fb.nonNullable.group({
    nome: ['', [Validators.required]],
    ordem: [0],
    ativo: [true],
  });

  constructor(
    private readonly ref: MatDialogRef<OrigemVendaDialogComponent, SalvarOrigemVendaRequest>,
    @Inject(MAT_DIALOG_DATA) readonly data: OrigemVendaDialogData,
  ) {
    this.edicao = !!data.origem;
    if (data.origem) {
      const o = data.origem;
      this.form.patchValue({ nome: o.nome, ordem: o.ordem, ativo: o.ativo });
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
