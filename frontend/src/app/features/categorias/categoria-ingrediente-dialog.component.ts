import { Component, Inject, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { CategoriaIngrediente, ESCOPOS_CATEGORIA, SalvarCategoriaIngredienteRequest } from './categorias.model';

export interface CategoriaIngredienteDialogData {
  categoria: CategoriaIngrediente | null;
}

@Component({
  selector: 'app-categoria-ingrediente-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatButtonModule, MatSlideToggleModule],
  template: `
    <h2 mat-dialog-title>{{ edicao ? 'Editar categoria' : 'Nova categoria de ingrediente' }}</h2>
    <mat-dialog-content>
      <form [formGroup]="form" class="form">
        <mat-form-field appearance="outline" class="full">
          <mat-label>Nome</mat-label>
          <input matInput formControlName="nome" />
          @if (form.controls.nome.hasError('required')) { <mat-error>Informe o nome.</mat-error> }
        </mat-form-field>
        <mat-form-field appearance="outline" class="full">
          <mat-label>Descrição (opcional)</mat-label>
          <textarea matInput rows="2" formControlName="descricao"></textarea>
        </mat-form-field>
        <mat-form-field appearance="outline" class="full">
          <mat-label>Escopo (onde aparece)</mat-label>
          <mat-select formControlName="escopo">
            @for (e of escopos; track e.valor) { <mat-option [value]="e.valor">{{ e.label }}</mat-option> }
          </mat-select>
          <mat-hint>Alimento = ingredientes · Material = embalagens/etiquetas · Ambos = os dois</mat-hint>
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
    .form { display: flex; flex-direction: column; gap: 0.5rem; padding-top: 0.75rem; min-width: 380px; }
    .full { width: 100%; }
    .btn-cta { background: var(--an-cta); color: #fff; }
    @media (max-width: 440px) { .form { min-width: auto; } }
  `],
})
export class CategoriaIngredienteDialogComponent {
  private readonly fb = inject(FormBuilder);
  readonly edicao: boolean;
  readonly escopos = ESCOPOS_CATEGORIA;

  readonly form = this.fb.nonNullable.group({
    nome: ['', [Validators.required]],
    descricao: [''],
    escopo: ['Alimento'],
    ordem: [0],
    ativo: [true],
  });

  constructor(
    private readonly ref: MatDialogRef<CategoriaIngredienteDialogComponent, SalvarCategoriaIngredienteRequest>,
    @Inject(MAT_DIALOG_DATA) readonly data: CategoriaIngredienteDialogData,
  ) {
    this.edicao = !!data.categoria;
    if (data.categoria) {
      const c = data.categoria;
      this.form.patchValue({ nome: c.nome, descricao: c.descricao ?? '', escopo: c.escopo || 'Alimento', ordem: c.ordem, ativo: c.ativo });
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
      descricao: v.descricao.trim() ? v.descricao.trim() : null,
      escopo: v.escopo,
      ordem: Number(v.ordem) || 0,
      ativo: v.ativo,
    });
  }

  cancelar(): void {
    this.ref.close();
  }
}
