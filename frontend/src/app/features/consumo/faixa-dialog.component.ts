import { Component, Inject, inject } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { FaixaConsumo, SalvarFaixaConsumoRequest } from './consumo.model';

export interface FaixaDialogData {
  faixa: FaixaConsumo | null;
}

function ordemValida(group: AbstractControl): ValidationErrors | null {
  const ini = group.get('pesoInicial')?.value;
  const fim = group.get('pesoFinal')?.value;
  return ini != null && fim != null && fim <= ini ? { ordem: true } : null;
}

@Component({
  selector: 'app-faixa-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSlideToggleModule,
  ],
  templateUrl: './faixa-dialog.component.html',
  styleUrl: './faixa-dialog.component.scss',
})
export class FaixaDialogComponent {
  private readonly fb = inject(FormBuilder);

  readonly edicao: boolean;

  readonly form = this.fb.nonNullable.group(
    {
      pesoInicial: [0, [Validators.required, Validators.min(0.01)]],
      pesoFinal: [0, [Validators.required, Validators.min(0.01)]],
      gramasPorDia: [0, [Validators.required, Validators.min(1)]],
      ativo: [true],
    },
    { validators: ordemValida },
  );

  constructor(
    private readonly ref: MatDialogRef<FaixaDialogComponent, SalvarFaixaConsumoRequest>,
    @Inject(MAT_DIALOG_DATA) readonly data: FaixaDialogData,
  ) {
    this.edicao = !!data.faixa;
    if (data.faixa) {
      const f = data.faixa;
      this.form.patchValue({
        pesoInicial: f.pesoInicial,
        pesoFinal: f.pesoFinal,
        gramasPorDia: f.gramasPorDia,
        ativo: f.ativo,
      });
    }
  }

  salvar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.ref.close(this.form.getRawValue());
  }

  cancelar(): void {
    this.ref.close();
  }
}
