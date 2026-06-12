import { Component, Inject, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSnackBar } from '@angular/material/snack-bar';
import {
  CATEGORIAS_ESTOQUE,
  ItemEstoque,
  OpcaoSimples,
  rotulo,
  TipoItemEstoque,
  UNIDADES_MEDIDA,
} from './estoque.model';
import { EstoqueService } from './estoque.service';

export interface ItemEstoqueDialogData {
  item: ItemEstoque | null;
}

@Component({
  selector: 'app-item-estoque-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatButtonToggleModule,
    MatSlideToggleModule,
  ],
  templateUrl: './item-estoque-dialog.component.html',
  styleUrl: './item-estoque-dialog.component.scss',
})
export class ItemEstoqueDialogComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(EstoqueService);
  private readonly snack = inject(MatSnackBar);

  readonly edicao: boolean;
  readonly categorias = CATEGORIAS_ESTOQUE;
  readonly unidades = UNIDADES_MEDIDA;
  readonly rotulo = rotulo;

  readonly salvando = signal(false);
  readonly ingredientes = signal<OpcaoSimples[]>([]);
  readonly receitas = signal<OpcaoSimples[]>([]);
  readonly tamanhos = signal<OpcaoSimples[]>([]);
  readonly fornecedores = signal<OpcaoSimples[]>([]);

  readonly form = this.fb.nonNullable.group({
    tipo: ['Insumo' as TipoItemEstoque],
    nome: ['', [Validators.required]],
    categoria: ['Proteinas'],
    unidadeMedida: ['Kg'],
    ingredienteId: [null as number | null],
    receitaId: [null as number | null],
    tamanhoPacoteId: [null as number | null],
    fornecedorPrincipalId: [null as number | null],
    quantidadeMinima: [0, [Validators.required, Validators.min(0)]],
    localArmazenamento: [''],
    controlaValidade: [true],
    observacoes: [''],
    ativo: [true],
  });

  constructor(
    private readonly ref: MatDialogRef<ItemEstoqueDialogComponent, boolean>,
    @Inject(MAT_DIALOG_DATA) readonly data: ItemEstoqueDialogData,
  ) {
    this.edicao = !!data.item;
  }

  get tipo(): TipoItemEstoque {
    return this.form.controls.tipo.value;
  }

  ngOnInit(): void {
    this.service.listarFornecedores(true).subscribe((fs) => this.fornecedores.set(fs.map((f) => ({ id: f.id, nome: f.nome }))));

    if (this.edicao) {
      const i = this.data.item!;
      this.form.patchValue({
        tipo: i.tipo,
        nome: i.nome,
        categoria: i.categoria,
        unidadeMedida: i.unidadeMedida,
        ingredienteId: i.ingredienteId,
        receitaId: i.receitaId,
        tamanhoPacoteId: i.tamanhoPacoteId,
        fornecedorPrincipalId: i.fornecedorPrincipalId,
        quantidadeMinima: i.quantidadeMinima,
        localArmazenamento: i.localArmazenamento ?? '',
        controlaValidade: i.controlaValidade,
        observacoes: i.observacoes ?? '',
        ativo: i.ativo,
      });
      this.form.controls.tipo.disable();
    } else {
      this.service.listarIngredientes().subscribe((xs) => this.ingredientes.set(xs));
      this.service.listarReceitasCasa().subscribe((xs) => this.receitas.set(xs));
      this.service.listarTamanhos().subscribe((xs) => this.tamanhos.set(xs));

      // Nome do insumo é pré-preenchido a partir do ingrediente (editável).
      this.form.controls.ingredienteId.valueChanges.subscribe((id) => {
        if (this.tipo === 'Insumo' && id != null) {
          const ing = this.ingredientes().find((x) => x.id === id);
          if (ing) {
            this.form.controls.nome.setValue(ing.nome);
          }
        }
      });

      // Nome do produto acabado é derivado de receita + tamanho.
      const derivarProdutoAcabado = () => {
        if (this.tipo !== 'ProdutoAcabadoCasa') {
          return;
        }
        const r = this.receitas().find((x) => x.id === this.form.controls.receitaId.value);
        const t = this.tamanhos().find((x) => x.id === this.form.controls.tamanhoPacoteId.value);
        this.form.controls.nome.setValue([r?.nome, t?.nome].filter(Boolean).join(' · '));
      };
      this.form.controls.receitaId.valueChanges.subscribe(derivarProdutoAcabado);
      this.form.controls.tamanhoPacoteId.valueChanges.subscribe(derivarProdutoAcabado);
      this.form.controls.tipo.valueChanges.subscribe(() => {
        this.form.controls.nome.setValue('');
        derivarProdutoAcabado();
      });
    }

    this.aplicarValidadores(this.tipo);
    this.form.controls.tipo.valueChanges.subscribe((t) => this.aplicarValidadores(t));
  }

  private aplicarValidadores(tipo: TipoItemEstoque): void {
    const { categoria, unidadeMedida, receitaId, tamanhoPacoteId } = this.form.controls;
    if (tipo === 'Insumo') {
      categoria.addValidators(Validators.required);
      unidadeMedida.addValidators(Validators.required);
      receitaId.clearValidators();
      tamanhoPacoteId.clearValidators();
    } else {
      receitaId.addValidators(Validators.required);
      tamanhoPacoteId.addValidators(Validators.required);
      categoria.clearValidators();
      unidadeMedida.clearValidators();
    }
    categoria.updateValueAndValidity();
    unidadeMedida.updateValueAndValidity();
    receitaId.updateValueAndValidity();
    tamanhoPacoteId.updateValueAndValidity();
  }

  salvar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const v = this.form.getRawValue();
    const t = (s: string) => (s.trim() ? s.trim() : null);
    this.salvando.set(true);

    let req$;
    if (this.edicao) {
      req$ = this.service.atualizarItem(this.data.item!.id, {
        nome: v.nome.trim(),
        categoria: v.categoria,
        unidadeMedida: v.unidadeMedida,
        quantidadeMinima: Number(v.quantidadeMinima),
        fornecedorPrincipalId: v.fornecedorPrincipalId,
        localArmazenamento: t(v.localArmazenamento),
        controlaValidade: v.controlaValidade,
        observacoes: t(v.observacoes),
        ativo: v.ativo,
      });
    } else if (v.tipo === 'Insumo') {
      req$ = this.service.criarInsumo({
        nome: v.nome.trim(),
        categoria: v.categoria,
        unidadeMedida: v.unidadeMedida,
        ingredienteId: v.ingredienteId,
        quantidadeMinima: Number(v.quantidadeMinima),
        fornecedorPrincipalId: v.fornecedorPrincipalId,
        localArmazenamento: t(v.localArmazenamento),
        controlaValidade: v.controlaValidade,
        observacoes: t(v.observacoes),
        ativo: v.ativo,
      });
    } else {
      req$ = this.service.criarProdutoAcabado({
        nome: v.nome.trim(),
        receitaId: Number(v.receitaId),
        tamanhoPacoteId: Number(v.tamanhoPacoteId),
        quantidadeMinima: Number(v.quantidadeMinima),
        localArmazenamento: t(v.localArmazenamento),
        controlaValidade: v.controlaValidade,
        observacoes: t(v.observacoes),
        ativo: v.ativo,
      });
    }

    req$.subscribe({
      next: () => this.ref.close(true),
      error: (e: HttpErrorResponse) => {
        this.salvando.set(false);
        this.snack.open(this.mensagemErro(e), 'Fechar', { duration: 4500 });
      },
    });
  }

  cancelar(): void {
    this.ref.close();
  }

  private mensagemErro(e: HttpErrorResponse): string {
    const errors = e.error?.errors as Record<string, string[]> | undefined;
    if (errors) {
      const primeira = Object.values(errors)[0]?.[0];
      if (primeira) {
        return primeira;
      }
    }
    return e.error?.detail ?? 'Não foi possível salvar o item.';
  }
}
