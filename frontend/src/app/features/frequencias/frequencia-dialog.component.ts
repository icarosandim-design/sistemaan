import { Component, Inject, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { FrequenciaEntrega, previewAgenda, SalvarFrequenciaRequest } from './frequencias.model';

export interface FrequenciaDialogData {
  frequencia: FrequenciaEntrega | null;
}

@Component({
  selector: 'app-frequencia-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSlideToggleModule,
  ],
  templateUrl: './frequencia-dialog.component.html',
  styleUrl: './frequencia-dialog.component.scss',
})
export class FrequenciaDialogComponent {
  private readonly fb = inject(FormBuilder);

  readonly edicao: boolean;

  readonly form = this.fb.nonNullable.group({
    nome: ['', [Validators.required]],
    personalizada: [false],
    diasCiclo: [7, [Validators.required, Validators.min(1)]],
    descricao: [''],
    ativo: [true],
  });

  constructor(
    private readonly ref: MatDialogRef<FrequenciaDialogComponent, SalvarFrequenciaRequest>,
    @Inject(MAT_DIALOG_DATA) readonly data: FrequenciaDialogData,
  ) {
    this.edicao = !!data.frequencia;
    if (data.frequencia) {
      const f = data.frequencia;
      this.form.patchValue({
        nome: f.nome,
        personalizada: f.personalizada,
        diasCiclo: f.diasCiclo ?? 7,
        descricao: f.descricao,
        ativo: f.ativo,
      });
    }
    this.ajustar(this.form.controls.personalizada.value);
    this.form.controls.personalizada.valueChanges.subscribe((v) => this.ajustar(v));
  }

  private ajustar(personalizada: boolean): void {
    const c = this.form.controls.diasCiclo;
    if (personalizada) {
      c.disable();
    } else if (c.disabled) {
      c.enable();
    }
  }

  get preview(): string[] {
    const v = this.form.getRawValue();
    return v.personalizada ? [] : previewAgenda(v.diasCiclo);
  }

  salvar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const v = this.form.getRawValue();
    const req: SalvarFrequenciaRequest = {
      nome: v.nome.trim(),
      personalizada: v.personalizada,
      diasCiclo: v.personalizada ? null : v.diasCiclo,
      descricao: v.descricao.trim(),
      ativo: v.ativo,
    };
    this.ref.close(req);
  }

  cancelar(): void {
    this.ref.close();
  }
}
