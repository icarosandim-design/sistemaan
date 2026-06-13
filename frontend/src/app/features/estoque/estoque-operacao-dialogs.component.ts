import { Component, Inject, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import {
  fmtMoeda,
  fmtQtd,
  ItemEstoque,
  OpcaoSimples,
  RegistrarAjusteRequest,
  RegistrarEntradaRequest,
  RegistrarSaidaRequest,
  rotulo,
  TIPOS_SAIDA,
} from './estoque.model';
import { EstoqueService } from './estoque.service';

export interface OperacaoDialogData {
  item: ItemEstoque;
}

/** Entrada pode vir do detalhe (item fixo) ou da tela de Compras (item a selecionar). */
export interface EntradaDialogData {
  item: ItemEstoque | null;
}

const HOJE = new Date().toISOString().slice(0, 10);

const MOTIVOS_SAIDA: { valor: string; label: string }[] = [
  { valor: 'Producao', label: 'Produção' },
  { valor: 'Descarte', label: 'Descarte' },
  { valor: 'Perda', label: 'Perda' },
  { valor: 'Vencimento', label: 'Vencimento' },
  { valor: 'Transferencia', label: 'Transferência' },
  { valor: 'ConsumoInterno', label: 'Consumo interno' },
  { valor: 'Outro', label: 'Outro' },
];

// ===================== Entrada =====================
@Component({
  selector: 'app-entrada-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatButtonModule],
  template: `
    <h2 mat-dialog-title>Nova entrada de estoque</h2>
    <mat-dialog-content>
      @if (itemFixo) {
        <p class="ctx">{{ itemFixo.nome }} · unidade: {{ rotulo(itemFixo.unidadeMedida) }}</p>
      }
      <form [formGroup]="form" class="form">
        <div class="grade">
          @if (!itemFixo) {
            <mat-form-field appearance="outline" class="full">
              <mat-label>Item de estoque</mat-label>
              <mat-select formControlName="itemEstoqueId">
                @for (i of itens(); track i.id) { <mat-option [value]="i.id">{{ i.nome }} ({{ rotulo(i.unidadeMedida) }})</mat-option> }
              </mat-select>
              @if (form.controls.itemEstoqueId.hasError('required')) { <mat-error>Selecione o item.</mat-error> }
            </mat-form-field>
          }
          <mat-form-field appearance="outline">
            <mat-label>Quantidade</mat-label>
            <input matInput type="number" step="0.001" formControlName="quantidade" />
            <span matTextSuffix>{{ unidadeLabel }}</span>
            @if (form.controls.quantidade.hasError('required') || form.controls.quantidade.hasError('min')) {
              <mat-error>Quantidade deve ser maior que zero.</mat-error>
            }
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>Valor unitário do produto (R$)</mat-label>
            <input matInput type="number" step="0.01" formControlName="valorUnitario" />
            @if (form.controls.valorUnitario.hasError('required')) { <mat-error>Informe o valor unitário.</mat-error> }
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>Frete (R$)</mat-label>
            <input matInput type="number" step="0.01" formControlName="frete" />
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>Fornecedor</mat-label>
            <mat-select formControlName="fornecedorId">
              <mat-option [value]="null">—</mat-option>
              @for (f of fornecedores(); track f.id) { <mat-option [value]="f.id">{{ f.nome }}</mat-option> }
            </mat-select>
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>Data da compra</mat-label>
            <input matInput type="date" formControlName="dataCompra" />
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>Data de entrada</mat-label>
            <input matInput type="date" formControlName="dataEntrada" />
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>Validade (opcional)</mat-label>
            <input matInput type="date" formControlName="validade" />
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>Código do lote (opcional)</mat-label>
            <input matInput formControlName="loteCodigo" />
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>Local de armazenamento (opcional)</mat-label>
            <input matInput formControlName="localArmazenamento" />
          </mat-form-field>
          <mat-form-field appearance="outline" class="full">
            <mat-label>Observações</mat-label>
            <input matInput formControlName="observacoes" />
          </mat-form-field>
        </div>

        <div class="resumo">
          <span>Valor produtos: <strong>{{ fmtMoeda(valorProdutos) }}</strong></span>
          <span>Frete: <strong>{{ fmtMoeda(freteValor) }}</strong></span>
          <span>Total pago: <strong>{{ fmtMoeda(totalPago) }}</strong></span>
        </div>
        <p class="dica">O frete fica registrado separado e <strong>não entra</strong> no custo do estoque.</p>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="cancelar()">Cancelar</button>
      <button mat-flat-button class="btn-cta" (click)="salvar()">Registrar entrada</button>
    </mat-dialog-actions>
  `,
  styles: [`
    .ctx { margin: 0 0 0.5rem; font-weight: 600; color: var(--an-texto-titulo); }
    .form { min-width: 520px; }
    .grade { display: grid; grid-template-columns: repeat(2, 1fr); gap: 0.25rem 0.75rem; }
    .full { grid-column: 1 / -1; }
    .resumo { display: flex; flex-wrap: wrap; gap: 0.4rem 1.5rem; margin-top: 0.5rem; font-size: 0.9rem; color: var(--an-texto-secundario); }
    .resumo strong { color: var(--an-primaria); }
    .dica { margin: 0.3rem 0 0; font-size: 0.8rem; color: var(--an-texto-secundario); }
    .btn-cta { background: var(--an-cta); color: #fff; }
    @media (max-width: 560px) { .form { min-width: auto; } .grade { grid-template-columns: 1fr; } }
  `],
})
export class EntradaDialogComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(EstoqueService);
  readonly rotulo = rotulo;
  readonly fmtMoeda = fmtMoeda;
  readonly fornecedores = signal<OpcaoSimples[]>([]);
  readonly itens = signal<ItemEstoque[]>([]);
  readonly itemFixo: ItemEstoque | null;

  readonly form = this.fb.nonNullable.group({
    itemEstoqueId: [null as number | null],
    quantidade: [null as number | null, [Validators.required, Validators.min(0.000001)]],
    valorUnitario: [null as number | null, [Validators.required, Validators.min(0)]],
    frete: [null as number | null],
    fornecedorId: [null as number | null],
    dataCompra: [HOJE, [Validators.required]],
    dataEntrada: [HOJE, [Validators.required]],
    validade: [''],
    loteCodigo: [''],
    localArmazenamento: [''],
    observacoes: [''],
  });

  constructor(
    private readonly ref: MatDialogRef<EntradaDialogComponent, RegistrarEntradaRequest>,
    @Inject(MAT_DIALOG_DATA) readonly data: EntradaDialogData,
  ) {
    this.itemFixo = data.item;
  }

  ngOnInit(): void {
    this.service.listarFornecedores(true).subscribe((fs) => this.fornecedores.set(fs.map((f) => ({ id: f.id, nome: f.nome }))));

    if (this.itemFixo) {
      this.form.controls.fornecedorId.setValue(this.itemFixo.fornecedorPrincipalId);
    } else {
      this.form.controls.itemEstoqueId.addValidators(Validators.required);
      this.form.controls.itemEstoqueId.updateValueAndValidity();
      this.service.listarItens().subscribe((xs) => this.itens.set(xs.filter((i) => i.ativo)));
      this.form.controls.itemEstoqueId.valueChanges.subscribe((id) => {
        const it = this.itens().find((x) => x.id === id);
        if (it) {
          this.form.controls.fornecedorId.setValue(it.fornecedorPrincipalId);
        }
      });
    }
  }

  get itemAtual(): ItemEstoque | null {
    return this.itemFixo ?? this.itens().find((x) => x.id === this.form.controls.itemEstoqueId.value) ?? null;
  }

  get unidadeLabel(): string {
    return this.itemAtual ? rotulo(this.itemAtual.unidadeMedida) : '';
  }

  get valorProdutos(): number {
    return (Number(this.form.controls.quantidade.value) || 0) * (Number(this.form.controls.valorUnitario.value) || 0);
  }

  get freteValor(): number {
    return Number(this.form.controls.frete.value) || 0;
  }

  get totalPago(): number {
    return this.valorProdutos + this.freteValor;
  }

  salvar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const v = this.form.getRawValue();
    const itemId = this.itemFixo ? this.itemFixo.id : v.itemEstoqueId!;
    this.ref.close({
      itemEstoqueId: itemId,
      quantidade: Number(v.quantidade),
      valorUnitario: Number(v.valorUnitario),
      valorTotal: null,
      fornecedorId: v.fornecedorId,
      dataCompra: v.dataCompra,
      dataEntrada: v.dataEntrada,
      validade: v.validade || null,
      loteCodigo: v.loteCodigo.trim() ? v.loteCodigo.trim() : null,
      localArmazenamento: v.localArmazenamento.trim() ? v.localArmazenamento.trim() : null,
      observacoes: v.observacoes.trim() ? v.observacoes.trim() : null,
      frete: v.frete != null ? Number(v.frete) : null,
    });
  }

  cancelar(): void {
    this.ref.close();
  }
}

// ===================== Saída =====================
@Component({
  selector: 'app-saida-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatButtonModule],
  template: `
    <h2 mat-dialog-title>Saída de estoque</h2>
    <mat-dialog-content>
      <p class="ctx">{{ data.item.nome }} · saldo: {{ fmtQtd(data.item.quantidadeAtual) }} {{ rotulo(data.item.unidadeMedida) }}</p>
      <form [formGroup]="form" class="form">
        <div class="grade">
          <mat-form-field appearance="outline">
            <mat-label>Quantidade</mat-label>
            <input matInput type="number" step="0.001" formControlName="quantidade" />
            @if (form.controls.quantidade.hasError('required') || form.controls.quantidade.hasError('min')) {
              <mat-error>Quantidade deve ser maior que zero.</mat-error>
            }
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>Tipo</mat-label>
            <mat-select formControlName="tipo">
              @for (t of tipos; track t) { <mat-option [value]="t">{{ rotulo(t) }}</mat-option> }
            </mat-select>
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>Motivo (opcional)</mat-label>
            <mat-select formControlName="motivoCodigo">
              <mat-option [value]="null">—</mat-option>
              @for (m of motivos; track m.valor) { <mat-option [value]="m.valor">{{ m.label }}</mat-option> }
            </mat-select>
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>Detalhe do motivo (opcional)</mat-label>
            <input matInput formControlName="motivo" />
          </mat-form-field>
          <mat-form-field appearance="outline" class="full">
            <mat-label>Observação</mat-label>
            <input matInput formControlName="observacao" />
          </mat-form-field>
        </div>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="cancelar()">Cancelar</button>
      <button mat-flat-button class="btn-cta" (click)="salvar()">Registrar saída</button>
    </mat-dialog-actions>
  `,
  styles: [`
    .ctx { margin: 0 0 0.5rem; font-weight: 600; color: var(--an-texto-titulo); }
    .form { min-width: 480px; }
    .grade { display: grid; grid-template-columns: repeat(2, 1fr); gap: 0.25rem 0.75rem; }
    .full { grid-column: 1 / -1; }
    .btn-cta { background: var(--an-cta); color: #fff; }
    @media (max-width: 520px) { .form { min-width: auto; } .grade { grid-template-columns: 1fr; } }
  `],
})
export class SaidaDialogComponent {
  private readonly fb = inject(FormBuilder);
  readonly rotulo = rotulo;
  readonly fmtQtd = fmtQtd;
  readonly tipos = TIPOS_SAIDA;
  readonly motivos = MOTIVOS_SAIDA;

  readonly form = this.fb.nonNullable.group({
    quantidade: [null as number | null, [Validators.required, Validators.min(0.000001)]],
    tipo: ['SaidaProducao'],
    motivoCodigo: [null as string | null],
    motivo: [''],
    observacao: [''],
  });

  constructor(
    private readonly ref: MatDialogRef<SaidaDialogComponent, RegistrarSaidaRequest>,
    @Inject(MAT_DIALOG_DATA) readonly data: OperacaoDialogData,
  ) {}

  salvar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const v = this.form.getRawValue();
    this.ref.close({
      itemEstoqueId: this.data.item.id,
      quantidade: Number(v.quantidade),
      tipo: v.tipo,
      motivoCodigo: v.motivoCodigo,
      motivo: v.motivo.trim() ? v.motivo.trim() : null,
      observacao: v.observacao.trim() ? v.observacao.trim() : null,
    });
  }

  cancelar(): void {
    this.ref.close();
  }
}

// ===================== Ajuste =====================
@Component({
  selector: 'app-ajuste-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatButtonModule],
  template: `
    <h2 mat-dialog-title>Ajuste manual de estoque</h2>
    <mat-dialog-content>
      <p class="ctx">{{ data.item.nome }} · saldo atual: {{ fmtQtd(data.item.quantidadeAtual) }} {{ rotulo(data.item.unidadeMedida) }}</p>
      <form [formGroup]="form" class="form">
        <mat-form-field appearance="outline" class="full">
          <mat-label>Nova quantidade</mat-label>
          <input matInput type="number" step="0.001" formControlName="novaQuantidade" />
          @if (form.controls.novaQuantidade.hasError('required') || form.controls.novaQuantidade.hasError('min')) {
            <mat-error>Informe a nova quantidade (não negativa).</mat-error>
          }
        </mat-form-field>
        <mat-form-field appearance="outline" class="full">
          <mat-label>Motivo</mat-label>
          <input matInput formControlName="motivo" placeholder="Ex.: Conferência física" />
          @if (form.controls.motivo.hasError('required')) { <mat-error>Informe o motivo.</mat-error> }
        </mat-form-field>
        <mat-form-field appearance="outline" class="full">
          <mat-label>Observação (opcional)</mat-label>
          <input matInput formControlName="observacao" />
        </mat-form-field>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="cancelar()">Cancelar</button>
      <button mat-flat-button class="btn-cta" (click)="salvar()">Registrar ajuste</button>
    </mat-dialog-actions>
  `,
  styles: [`
    .ctx { margin: 0 0 0.5rem; font-weight: 600; color: var(--an-texto-titulo); }
    .form { min-width: 420px; display: flex; flex-direction: column; }
    .full { width: 100%; }
    .btn-cta { background: var(--an-cta); color: #fff; }
    @media (max-width: 460px) { .form { min-width: auto; } }
  `],
})
export class AjusteDialogComponent {
  private readonly fb = inject(FormBuilder);
  readonly rotulo = rotulo;
  readonly fmtQtd = fmtQtd;

  readonly form = this.fb.nonNullable.group({
    novaQuantidade: [null as number | null, [Validators.required, Validators.min(0)]],
    motivo: ['', [Validators.required]],
    observacao: [''],
  });

  constructor(
    private readonly ref: MatDialogRef<AjusteDialogComponent, RegistrarAjusteRequest>,
    @Inject(MAT_DIALOG_DATA) readonly data: OperacaoDialogData,
  ) {}

  salvar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const v = this.form.getRawValue();
    this.ref.close({
      itemEstoqueId: this.data.item.id,
      loteEstoqueId: null,
      novaQuantidade: Number(v.novaQuantidade),
      motivo: v.motivo.trim(),
      observacao: v.observacao.trim() ? v.observacao.trim() : null,
    });
  }

  cancelar(): void {
    this.ref.close();
  }
}
