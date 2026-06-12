import { Component, Inject, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import {
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
    <h2 mat-dialog-title>Entrada de estoque</h2>
    <mat-dialog-content>
      <p class="ctx">{{ data.item.nome }} · unidade: {{ rotulo(data.item.unidadeMedida) }}</p>
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
            <mat-label>Valor unitário (R$)</mat-label>
            <input matInput type="number" step="0.01" formControlName="valorUnitario" />
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>Valor total (R$)</mat-label>
            <input matInput type="number" step="0.01" formControlName="valorTotal" />
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
          <mat-form-field appearance="outline" class="full">
            <mat-label>Observações</mat-label>
            <input matInput formControlName="observacoes" />
          </mat-form-field>
        </div>
        <p class="dica">Informe o valor unitário <strong>ou</strong> o valor total.</p>
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
    .dica { margin: 0.25rem 0 0; font-size: 0.8rem; color: var(--an-texto-secundario); }
    .btn-cta { background: var(--an-cta); color: #fff; }
    @media (max-width: 560px) { .form { min-width: auto; } .grade { grid-template-columns: 1fr; } }
  `],
})
export class EntradaDialogComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(EstoqueService);
  readonly rotulo = rotulo;
  readonly fornecedores = signal<OpcaoSimples[]>([]);

  readonly form = this.fb.nonNullable.group({
    quantidade: [null as number | null, [Validators.required, Validators.min(0.000001)]],
    valorUnitario: [null as number | null],
    valorTotal: [null as number | null],
    fornecedorId: [null as number | null],
    dataCompra: [HOJE, [Validators.required]],
    dataEntrada: [HOJE, [Validators.required]],
    validade: [''],
    loteCodigo: [''],
    observacoes: [''],
  });

  constructor(
    private readonly ref: MatDialogRef<EntradaDialogComponent, RegistrarEntradaRequest>,
    @Inject(MAT_DIALOG_DATA) readonly data: OperacaoDialogData,
  ) {}

  ngOnInit(): void {
    this.form.controls.fornecedorId.setValue(this.data.item.fornecedorPrincipalId);
    this.service.listarFornecedores(true).subscribe((fs) => this.fornecedores.set(fs.map((f) => ({ id: f.id, nome: f.nome }))));
  }

  salvar(): void {
    const v = this.form.getRawValue();
    if (this.form.invalid || (v.valorUnitario == null && v.valorTotal == null)) {
      this.form.markAllAsTouched();
      return;
    }
    this.ref.close({
      itemEstoqueId: this.data.item.id,
      quantidade: Number(v.quantidade),
      valorUnitario: v.valorUnitario != null ? Number(v.valorUnitario) : null,
      valorTotal: v.valorTotal != null ? Number(v.valorTotal) : null,
      fornecedorId: v.fornecedorId,
      dataCompra: v.dataCompra,
      dataEntrada: v.dataEntrada,
      validade: v.validade || null,
      loteCodigo: v.loteCodigo.trim() ? v.loteCodigo.trim() : null,
      localArmazenamento: null,
      observacoes: v.observacoes.trim() ? v.observacoes.trim() : null,
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
