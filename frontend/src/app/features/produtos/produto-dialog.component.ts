import { Component, Inject, OnInit, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { Produto, SalvarProdutoRequest, TipoProduto, TIPOS_PRODUTO, UNIDADES_PRODUTO } from './produtos.model';
import { EstoqueService } from '../estoque/estoque.service';

interface OpcaoSimples { id: number; nome: string; }

export interface ProdutoDialogData {
  produto: Produto | null;
}

@Component({
  selector: 'app-produto-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatButtonModule, MatSlideToggleModule],
  template: `
    <h2 mat-dialog-title>{{ edicao ? 'Editar produto' : 'Novo produto' }}</h2>
    <mat-dialog-content>
      <form [formGroup]="form" class="form">
        <mat-form-field appearance="outline" class="full">
          <mat-label>Nome</mat-label>
          <input matInput formControlName="nome" />
          @if (form.controls.nome.hasError('required')) { <mat-error>Informe o nome.</mat-error> }
        </mat-form-field>

        <div class="linha">
          <mat-form-field appearance="outline">
            <mat-label>Tipo</mat-label>
            <mat-select formControlName="tipo">
              @for (t of tipos; track t.valor) { <mat-option [value]="t.valor">{{ t.label }}</mat-option> }
            </mat-select>
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>Código (opcional)</mat-label>
            <input matInput formControlName="codigo" />
          </mat-form-field>
        </div>

        @if (ehReceita()) {
          <div class="linha">
            <mat-form-field appearance="outline">
              <mat-label>Receita da Casa</mat-label>
              <mat-select formControlName="receitaCasaId">
                @for (r of receitas(); track r.id) { <mat-option [value]="r.id">{{ r.nome }}</mat-option> }
              </mat-select>
              @if (form.controls.receitaCasaId.hasError('required')) { <mat-error>Selecione a receita.</mat-error> }
            </mat-form-field>
            <mat-form-field appearance="outline">
              <mat-label>Tamanho de pacote</mat-label>
              <mat-select formControlName="tamanhoPacoteId">
                @for (t of tamanhos(); track t.id) { <mat-option [value]="t.id">{{ t.nome }}</mat-option> }
              </mat-select>
            </mat-form-field>
          </div>
        } @else {
          <mat-form-field appearance="outline">
            <mat-label>Unidade de medida</mat-label>
            <mat-select formControlName="unidadeMedida">
              @for (u of unidades; track u) { <mat-option [value]="u">{{ u }}</mat-option> }
            </mat-select>
          </mat-form-field>
        }

        <div class="linha">
          <mat-form-field appearance="outline">
            <mat-label>Preço venda PF (avulso)</mat-label>
            <input matInput type="number" step="0.01" min="0" formControlName="precoVendaAvulsaPF" />
            <span matTextPrefix>R$&nbsp;</span>
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>Preço venda PJ</mat-label>
            <input matInput type="number" step="0.01" min="0" formControlName="precoVendaPJ" />
            <span matTextPrefix>R$&nbsp;</span>
          </mat-form-field>
        </div>

        <div class="toggles">
          <mat-slide-toggle formControlName="controlaEstoque" color="primary">Controla estoque</mat-slide-toggle>
          @if (!ehReceita()) {
            <mat-slide-toggle formControlName="produzidoInternamente" color="primary">Produzido internamente</mat-slide-toggle>
          }
          <mat-slide-toggle formControlName="ativo" color="primary">Ativo</mat-slide-toggle>
        </div>

        <mat-form-field appearance="outline" class="full">
          <mat-label>Observações (opcional)</mat-label>
          <textarea matInput rows="2" formControlName="observacoes"></textarea>
        </mat-form-field>
        @if (ehReceita()) {
          <p class="nota">Produto de Receita da Casa é produzido internamente e entra no estoque pela produção.</p>
        }
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="cancelar()">Cancelar</button>
      <button mat-flat-button class="btn-cta" (click)="salvar()">{{ edicao ? 'Salvar' : 'Cadastrar' }}</button>
    </mat-dialog-actions>
  `,
  styles: [`
    .form { display: flex; flex-direction: column; gap: 0.4rem; padding-top: 0.5rem; min-width: 420px; }
    .full { width: 100%; }
    .linha { display: flex; gap: 0.6rem; } .linha mat-form-field { flex: 1; }
    .toggles { display: flex; flex-direction: column; gap: 0.4rem; margin: 0.3rem 0; }
    .btn-cta { background: var(--an-cta); color: #fff; }
    .nota { font-size: 0.78rem; color: var(--an-texto-secundario); }
  `],
})
export class ProdutoDialogComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly estoque = inject(EstoqueService);
  readonly edicao: boolean;
  readonly tipos = TIPOS_PRODUTO;
  readonly unidades = UNIDADES_PRODUTO;
  readonly receitas = signal<OpcaoSimples[]>([]);
  readonly tamanhos = signal<OpcaoSimples[]>([]);

  readonly form = this.fb.nonNullable.group({
    nome: ['', [Validators.required]],
    codigo: [''],
    tipo: ['ReceitaDaCasa' as TipoProduto],
    receitaCasaId: [null as number | null],
    tamanhoPacoteId: [null as number | null],
    unidadeMedida: ['Unidade'],
    precoVendaAvulsaPF: [0],
    precoVendaPJ: [0],
    controlaEstoque: [true],
    produzidoInternamente: [true],
    ativo: [true],
    observacoes: [''],
  });

  constructor(
    private readonly ref: MatDialogRef<ProdutoDialogComponent, SalvarProdutoRequest>,
    @Inject(MAT_DIALOG_DATA) readonly data: ProdutoDialogData,
  ) {
    this.edicao = !!data.produto;
    if (data.produto) {
      const p = data.produto;
      this.form.patchValue({
        nome: p.nome, codigo: p.codigo ?? '', tipo: p.tipo,
        receitaCasaId: p.receitaCasaId, tamanhoPacoteId: p.tamanhoPacoteId,
        unidadeMedida: p.unidadeMedida, precoVendaAvulsaPF: p.precoVendaAvulsaPF, precoVendaPJ: p.precoVendaPJ,
        controlaEstoque: p.controlaEstoque, produzidoInternamente: p.produzidoInternamente, ativo: p.ativo,
        observacoes: p.observacoes ?? '',
      });
    }
  }

  ngOnInit(): void {
    this.estoque.listarReceitasCasa().subscribe((r) => this.receitas.set(r));
    this.estoque.listarTamanhos().subscribe((t) => this.tamanhos.set(t));
  }

  ehReceita(): boolean {
    return this.form.controls.tipo.value === 'ReceitaDaCasa';
  }

  salvar(): void {
    const v = this.form.getRawValue();
    if (!v.nome.trim()) { this.form.controls.nome.markAsTouched(); return; }
    if (v.tipo === 'ReceitaDaCasa' && !v.receitaCasaId) { this.form.controls.receitaCasaId.setErrors({ required: true }); return; }
    const req: SalvarProdutoRequest = {
      nome: v.nome.trim(),
      codigo: v.codigo.trim() || null,
      tipo: v.tipo,
      receitaCasaId: v.tipo === 'ReceitaDaCasa' ? v.receitaCasaId : null,
      tamanhoPacoteId: v.tipo === 'ReceitaDaCasa' ? v.tamanhoPacoteId : null,
      unidadeMedida: v.tipo === 'ReceitaDaCasa' ? 'Pacote' : v.unidadeMedida,
      pesoGramas: null,
      precoVendaAvulsaPF: Number(v.precoVendaAvulsaPF) || 0,
      precoVendaPJ: Number(v.precoVendaPJ) || 0,
      controlaEstoque: v.controlaEstoque,
      produzidoInternamente: v.tipo === 'ReceitaDaCasa' ? true : v.produzidoInternamente,
      ativo: v.ativo,
      observacoes: v.observacoes.trim() || null,
    };
    this.ref.close(req);
  }

  cancelar(): void {
    this.ref.close();
  }
}
