import { Component, Inject, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import {
  Categoria,
  coefDeFator,
  custoRealKg,
  fatorCorrecaoPct,
  fmtFatorCorrecao,
  fmtMoeda,
  Ingrediente,
  previewConversao,
  SalvarIngredienteRequest,
  TIPOS_CONVERSAO,
  TipoConversao,
} from './ingredientes.model';

export interface IngredienteDialogData {
  ingrediente: Ingrediente | null;
  categorias: Categoria[];
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
  ],
  templateUrl: './ingrediente-dialog.component.html',
  styleUrl: './ingrediente-dialog.component.scss',
})
export class IngredienteDialogComponent {
  private readonly fb = inject(FormBuilder);

  readonly categorias: Categoria[];
  readonly tipos = TIPOS_CONVERSAO;
  readonly edicao: boolean;
  readonly fmtMoeda = fmtMoeda;

  readonly form = this.fb.nonNullable.group({
    nome: ['', [Validators.required]],
    categoriaId: [null as number | null, [Validators.required]],
    tipoConversao: ['perda' as TipoConversao, [Validators.required]],
    fatorPct: [0, [Validators.required, Validators.min(0)]],
    custoKg: [0, [Validators.required, Validators.min(0)]],
    ativo: [true],
  });

  constructor(
    private readonly ref: MatDialogRef<IngredienteDialogComponent, SalvarIngredienteRequest>,
    @Inject(MAT_DIALOG_DATA) readonly data: IngredienteDialogData,
  ) {
    // Lista única com escopo: o cadastro de ingrediente mostra só categorias de Alimento (ou Ambos).
    this.categorias = data.categorias.filter((c) => c.escopo !== 'Material');
    this.edicao = !!data.ingrediente;

    if (data.ingrediente) {
      const i = data.ingrediente;
      this.form.patchValue({
        nome: i.nome,
        categoriaId: i.categoriaId,
        tipoConversao: i.tipoConversao,
        fatorPct: fatorCorrecaoPct(i.tipoConversao, i.coeficiente) ?? 0,
        custoKg: i.custoKg,
        ativo: i.ativo,
      });
    }

    this.ajustarFator(this.form.controls.tipoConversao.value);
    this.form.controls.tipoConversao.valueChanges.subscribe((t) => this.ajustarFator(t));
  }

  private get coeficiente(): number {
    const v = this.form.getRawValue();
    return coefDeFator(v.tipoConversao, v.fatorPct);
  }

  get preview(): string[] {
    return previewConversao(this.form.getRawValue().tipoConversao, this.coeficiente);
  }

  get fatorLabel(): string {
    return fmtFatorCorrecao(this.form.getRawValue().tipoConversao, this.coeficiente);
  }

  get custoRealLabel(): string {
    return fmtMoeda(custoRealKg(this.form.getRawValue().custoKg, this.coeficiente));
  }

  private ajustarFator(tipo: TipoConversao): void {
    const c = this.form.controls.fatorPct;
    if (tipo === 'sem_conversao') {
      c.setValue(0);
      c.disable();
      return;
    }
    if (c.disabled) {
      c.enable();
    }
    if (tipo === 'perda') {
      c.setValidators([Validators.required, Validators.min(0), Validators.max(99.99)]);
    } else {
      c.setValidators([Validators.required, Validators.min(0.01)]);
    }
    c.updateValueAndValidity({ emitEvent: false });
  }

  salvar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const v = this.form.getRawValue();

    const request: SalvarIngredienteRequest = {
      nome: v.nome.trim(),
      categoriaId: v.categoriaId!,
      tipoConversao: v.tipoConversao,
      coeficiente: coefDeFator(v.tipoConversao, v.fatorPct),
      custoKg: v.custoKg,
      ativo: v.ativo,
    };

    this.ref.close(request);
  }

  cancelar(): void {
    this.ref.close();
  }
}
