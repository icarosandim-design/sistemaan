import { Component, Inject, inject } from '@angular/core';
import { FormArray, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import {
  BASE_GRAMAS,
  custoItem,
  fmtMoeda,
  IngredienteAtivo,
  ReceitaCasa,
} from './receitas.model';

export interface ReceitaDialogData {
  receita: ReceitaCasa | null;
  ingredientes: IngredienteAtivo[];
}

@Component({
  selector: 'app-receita-dialog',
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
  templateUrl: './receita-dialog.component.html',
  styleUrl: './receita-dialog.component.scss',
})
export class ReceitaDialogComponent {
  private readonly fb = inject(FormBuilder);

  readonly base = BASE_GRAMAS;
  readonly edicao: boolean;
  readonly ingredientes: IngredienteAtivo[];
  private readonly mapa: Map<number, IngredienteAtivo>;

  readonly form = this.fb.nonNullable.group({
    codigo: ['', [Validators.required]],
    nome: ['', [Validators.required]],
    ativo: [true],
    observacoes: [''],
    itens: this.fb.array<ReturnType<ReceitaDialogComponent['novoItem']>>([]),
  });

  constructor(
    private readonly ref: MatDialogRef<ReceitaDialogComponent, ReceitaCasa>,
    @Inject(MAT_DIALOG_DATA) readonly data: ReceitaDialogData,
  ) {
    this.ingredientes = data.ingredientes;
    this.mapa = new Map(data.ingredientes.map((i) => [i.id, i]));
    this.edicao = !!data.receita;

    if (data.receita) {
      const r = data.receita;
      this.form.patchValue({ codigo: r.codigo, nome: r.nome, ativo: r.ativo, observacoes: r.observacoes });
      r.itens.forEach((it) => this.itens.push(this.novoItem(it.ingredienteId, it.gramas)));
    } else {
      this.adicionar();
    }
  }

  private novoItem(ingredienteId: number | null = null, gramas = 0) {
    return this.fb.nonNullable.group({
      ingredienteId: [ingredienteId, [Validators.required]],
      gramas: [gramas, [Validators.required, Validators.min(1)]],
    });
  }

  get itens(): FormArray {
    return this.form.controls.itens;
  }

  adicionar(): void {
    this.itens.push(this.novoItem());
  }

  remover(i: number): void {
    this.itens.removeAt(i);
  }

  categoriaDe(id: number | null): string {
    return id ? this.mapa.get(id)?.categoria ?? '' : '';
  }

  custoDe(id: number | null, gramas: number): number {
    return custoItem(gramas || 0, id ? this.mapa.get(id) : undefined);
  }

  get total(): number {
    return this.itens.controls.reduce((s, c) => s + (Number(c.value.gramas) || 0), 0);
  }

  get custoTotal(): number {
    return this.itens.controls.reduce(
      (s, c) => s + this.custoDe(c.value.ingredienteId ?? null, Number(c.value.gramas) || 0),
      0,
    );
  }

  get fichaValida(): boolean {
    return this.total === this.base && this.itens.length > 0;
  }

  get podeSalvar(): boolean {
    return this.form.valid && this.fichaValida;
  }

  get statusFicha(): { classe: string; texto: string } {
    if (this.total < this.base) {
      return { classe: 'falta', texto: `Total: ${this.total} g — faltam ${this.base - this.total} g` };
    }
    if (this.total > this.base) {
      return { classe: 'excede', texto: `Total: ${this.total} g — excedeu ${this.total - this.base} g` };
    }
    return { classe: 'ok', texto: 'Total: 1.000 g — ficha válida' };
  }

  readonly fmtMoeda = fmtMoeda;

  salvar(): void {
    if (!this.podeSalvar) {
      this.form.markAllAsTouched();
      return;
    }
    const v = this.form.getRawValue();
    const resultado: ReceitaCasa = {
      id: this.data.receita?.id ?? 0,
      codigo: v.codigo.trim(),
      nome: v.nome.trim(),
      ativo: v.ativo,
      observacoes: v.observacoes.trim(),
      itens: this.itens.controls.map((c) => ({
        ingredienteId: c.value.ingredienteId as number,
        gramas: Number(c.value.gramas),
      })),
    };
    this.ref.close(resultado);
  }

  cancelar(): void {
    this.ref.close();
  }
}
