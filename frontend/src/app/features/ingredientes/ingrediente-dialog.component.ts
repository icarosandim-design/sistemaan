import { Component, Inject, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatTabsModule } from '@angular/material/tabs';
import {
  CATEGORIAS,
  fmtMoeda,
  Ingrediente,
  previewConversao,
  TIPOS_CONVERSAO,
  TipoConversao,
} from './ingredientes.model';

export interface IngredienteDialogData {
  ingrediente: Ingrediente | null;
}

@Component({
  selector: 'app-ingrediente-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatSlideToggleModule,
    MatTabsModule,
  ],
  templateUrl: './ingrediente-dialog.component.html',
  styleUrl: './ingrediente-dialog.component.scss',
})
export class IngredienteDialogComponent {
  private readonly fb = inject(FormBuilder);

  readonly categorias = CATEGORIAS;
  readonly tipos = TIPOS_CONVERSAO;
  readonly edicao: boolean;
  readonly fmtMoeda = fmtMoeda;

  readonly form = this.fb.nonNullable.group({
    nome: ['', [Validators.required]],
    categoria: ['', [Validators.required]],
    tipoConversao: ['perda' as TipoConversao, [Validators.required]],
    coeficiente: [1, [Validators.required, Validators.min(0.0001)]],
    custoKg: [0, [Validators.required, Validators.min(0)]],
    ativo: [true],
  });

  constructor(
    private readonly ref: MatDialogRef<IngredienteDialogComponent, Ingrediente>,
    @Inject(MAT_DIALOG_DATA) readonly data: IngredienteDialogData,
  ) {
    this.edicao = !!data.ingrediente;

    if (data.ingrediente) {
      const i = data.ingrediente;
      this.form.patchValue({
        nome: i.nome,
        categoria: i.categoria,
        tipoConversao: i.tipoConversao,
        coeficiente: i.coeficiente,
        custoKg: i.custoKg,
        ativo: i.ativo,
      });
    }

    this.ajustarCoeficiente(this.form.controls.tipoConversao.value);
    this.form.controls.tipoConversao.valueChanges.subscribe((t) => this.ajustarCoeficiente(t));
  }

  get preview(): string[] {
    const { tipoConversao, coeficiente } = this.form.getRawValue();
    return previewConversao(tipoConversao, coeficiente);
  }

  get historico() {
    return this.data.ingrediente?.historico ?? [];
  }

  private ajustarCoeficiente(tipo: TipoConversao): void {
    const c = this.form.controls.coeficiente;
    if (tipo === 'sem_conversao') {
      c.setValue(1);
      c.disable();
    } else if (c.disabled) {
      c.enable();
    }
  }

  salvar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const v = this.form.getRawValue();
    const original = this.data.ingrediente;
    const historico = original ? [...original.historico] : [];

    if (original && v.custoKg !== original.custoKg) {
      historico.unshift({
        data: this.hoje(),
        valorAnterior: original.custoKg,
        valorNovo: v.custoKg,
      });
    }

    const resultado: Ingrediente = {
      id: original?.id ?? Date.now(),
      nome: v.nome.trim(),
      categoria: v.categoria,
      tipoConversao: v.tipoConversao,
      coeficiente: v.tipoConversao === 'sem_conversao' ? 1 : v.coeficiente,
      custoKg: v.custoKg,
      ativo: v.ativo,
      historico,
    };

    this.ref.close(resultado);
  }

  cancelar(): void {
    this.ref.close();
  }

  private hoje(): string {
    return new Date().toLocaleDateString('pt-BR');
  }
}
