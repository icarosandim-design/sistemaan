import { Component, Inject, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { SalvarTamanhoPacoteRequest, TamanhoPacote } from './tamanhos-pacote.model';

export interface TamanhoPacoteDialogData {
  tamanho: TamanhoPacote | null;
}

@Component({
  selector: 'app-tamanho-pacote-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSlideToggleModule,
  ],
  templateUrl: './tamanho-pacote-dialog.component.html',
  styleUrl: './tamanho-pacote-dialog.component.scss',
})
export class TamanhoPacoteDialogComponent {
  private readonly fb = inject(FormBuilder);

  readonly edicao: boolean;

  readonly form = this.fb.nonNullable.group({
    nome: ['', [Validators.required]],
    pesoGramas: [250, [Validators.required, Validators.min(1)]],
    observacao: [''],
    ativo: [true],
  });

  constructor(
    private readonly ref: MatDialogRef<TamanhoPacoteDialogComponent, SalvarTamanhoPacoteRequest>,
    @Inject(MAT_DIALOG_DATA) readonly data: TamanhoPacoteDialogData,
  ) {
    this.edicao = !!data.tamanho;
    if (data.tamanho) {
      const t = data.tamanho;
      this.form.patchValue({
        nome: t.nome,
        pesoGramas: t.pesoGramas,
        observacao: t.observacao ?? '',
        ativo: t.ativo,
      });
    }
  }

  salvar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const v = this.form.getRawValue();
    const req: SalvarTamanhoPacoteRequest = {
      nome: v.nome.trim(),
      pesoGramas: Number(v.pesoGramas),
      observacao: v.observacao.trim() ? v.observacao.trim() : null,
      ativo: v.ativo,
    };
    this.ref.close(req);
  }

  cancelar(): void {
    this.ref.close();
  }
}
