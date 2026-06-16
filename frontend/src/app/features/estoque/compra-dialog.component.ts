import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { EstoqueService } from './estoque.service';
import { CompraResultado, fmtMoeda, ItemEstoque, OpcaoSimples, RegistrarCompraRequest, rotulo } from './estoque.model';

function msgErro(e: unknown): string {
  const err = (e as { error?: { errors?: Record<string, string[]>; detail?: string } })?.error;
  const first = err?.errors ? Object.values(err.errors)[0]?.[0] : undefined;
  return first ?? err?.detail ?? 'Não foi possível registrar a compra.';
}

interface LinhaCompra {
  itemEstoqueId: number | null;
  quantidade: number | null;
  valorUnitario: number | null;
}

@Component({
  selector: 'app-compra-dialog',
  standalone: true,
  imports: [
    FormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatTooltipModule,
  ],
  template: `
    <h2 mat-dialog-title>Nova compra (nota com vários itens)</h2>
    <mat-dialog-content>
      <!-- Cabeçalho da nota -->
      <div class="grade-cab">
        <mat-form-field appearance="outline">
          <mat-label>Fornecedor</mat-label>
          <mat-select [(ngModel)]="fornecedorId">
            <mat-option [value]="null">—</mat-option>
            @for (f of fornecedores(); track f.id) { <mat-option [value]="f.id">{{ f.nome }}</mat-option> }
          </mat-select>
        </mat-form-field>
        <mat-form-field appearance="outline">
          <mat-label>Nº da nota (opcional)</mat-label>
          <input matInput [(ngModel)]="notaFiscal" />
        </mat-form-field>
        <mat-form-field appearance="outline">
          <mat-label>Data da compra</mat-label>
          <input matInput type="date" [(ngModel)]="dataCompra" />
        </mat-form-field>
        <mat-form-field appearance="outline">
          <mat-label>Data de entrada</mat-label>
          <input matInput type="date" [(ngModel)]="dataEntrada" />
        </mat-form-field>
        <mat-form-field appearance="outline">
          <mat-label>Frete total (R$)</mat-label>
          <input matInput type="number" min="0" step="0.01" [(ngModel)]="frete" />
        </mat-form-field>
        <mat-form-field appearance="outline">
          <mat-label>Observações (opcional)</mat-label>
          <input matInput [(ngModel)]="observacoes" />
        </mat-form-field>
      </div>

      <!-- Cabeçalho fiscal / financeiro (opcional) — base para a futura Conta a Pagar -->
      <h3 class="sec">Dados da nota / pagamento (opcional)</h3>
      <div class="grade-cab">
        <mat-form-field appearance="outline">
          <mat-label>Série da NF</mat-label>
          <input matInput [(ngModel)]="serieNotaFiscal" />
        </mat-form-field>
        <mat-form-field appearance="outline">
          <mat-label>Emissão da NF</mat-label>
          <input matInput type="date" [(ngModel)]="dataEmissaoNotaFiscal" />
        </mat-form-field>
        <mat-form-field appearance="outline">
          <mat-label>Vencimento do pagamento</mat-label>
          <input matInput type="date" [(ngModel)]="dataVencimentoPagamento" />
        </mat-form-field>
        <mat-form-field appearance="outline">
          <mat-label>Forma de pagamento</mat-label>
          <input matInput [(ngModel)]="formaPagamento" placeholder="Boleto, Pix, ..." />
        </mat-form-field>
        <mat-form-field appearance="outline">
          <mat-label>Condição de pagamento</mat-label>
          <input matInput [(ngModel)]="condicaoPagamento" placeholder="Ex.: 30 dias" />
        </mat-form-field>
        <mat-form-field appearance="outline">
          <mat-label>Linha digitável (boleto)</mat-label>
          <input matInput [(ngModel)]="linhaDigitavelBoleto" />
        </mat-form-field>
      </div>

      <!-- Itens da nota -->
      <h3 class="sec">Itens</h3>
      @for (l of itens; track $index) {
        <div class="linha-item">
          <mat-form-field appearance="outline" class="f-item">
            <mat-label>Item de estoque</mat-label>
            <mat-select [(ngModel)]="l.itemEstoqueId">
              @for (i of opcoes(); track i.id) { <mat-option [value]="i.id">{{ i.nome }}</mat-option> }
            </mat-select>
          </mat-form-field>
          <mat-form-field appearance="outline" class="f-qtd">
            <mat-label>Qtd</mat-label>
            <input matInput type="number" min="0" step="0.001" [(ngModel)]="l.quantidade" />
          </mat-form-field>
          <mat-form-field appearance="outline" class="f-val">
            <mat-label>Valor un. (R$)</mat-label>
            <input matInput type="number" min="0" step="0.01" [(ngModel)]="l.valorUnitario" />
          </mat-form-field>
          <span class="f-sub">{{ fmtMoeda(subtotal(l)) }}</span>
          <button mat-icon-button type="button" (click)="remover($index)" matTooltip="Remover"><mat-icon>delete</mat-icon></button>
        </div>
      }
      <button mat-button type="button" class="add" (click)="adicionar()"><mat-icon>add</mat-icon> Adicionar item</button>

      <!-- Totais -->
      <div class="totais">
        <span>Produtos: <strong>{{ fmtMoeda(totalProdutos) }}</strong></span>
        <span>Frete: <strong>{{ fmtMoeda(freteValor) }}</strong></span>
        <span>Total pago: <strong>{{ fmtMoeda(totalProdutos + freteValor) }}</strong></span>
      </div>
      <p class="dica">O frete fica registrado na nota e <strong>não entra</strong> no custo dos itens.</p>

      @if (erro()) { <p class="erro"><mat-icon>error</mat-icon> {{ erro() }}</p> }
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="ref.close()">Cancelar</button>
      <button mat-flat-button class="btn-cta" [disabled]="salvando()" (click)="salvar()">
        {{ salvando() ? 'Registrando...' : 'Registrar compra' }}
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    .grade-cab { display: grid; grid-template-columns: repeat(2, 1fr); gap: 0.1rem 0.75rem; }
    .sec { margin: 0.6rem 0 0.4rem; font-size: 0.9rem; font-weight: 700; }
    .linha-item { display: flex; align-items: center; gap: 0.5rem; }
    .f-item { flex: 2; min-width: 180px; }
    .f-qtd { width: 90px; }
    .f-val { width: 120px; }
    .f-sub { min-width: 90px; text-align: right; font-weight: 600; color: var(--an-primaria); }
    .add { margin: 0 0 0.4rem; color: var(--an-cta-hover); font-weight: 600; }
    .totais { display: flex; flex-wrap: wrap; gap: 0.4rem 1.5rem; margin-top: 0.4rem; font-size: 0.9rem; color: var(--an-texto-secundario); }
    .totais strong { color: var(--an-primaria); }
    .dica { margin: 0.3rem 0 0; font-size: 0.8rem; color: var(--an-texto-secundario); }
    .erro { display: flex; align-items: center; gap: 0.4rem; color: #b3261e; font-size: 0.9rem; }
    .btn-cta { background: var(--an-cta); color: #fff; }
    mat-dialog-content { min-width: min(680px, 92vw); }
    @media (max-width: 600px) { .grade-cab { grid-template-columns: 1fr; } }
  `],
})
export class CompraDialogComponent implements OnInit {
  private readonly service = inject(EstoqueService);
  readonly ref = inject(MatDialogRef<CompraDialogComponent, CompraResultado>);

  readonly opcoes = signal<OpcaoSimples[]>([]);
  readonly fornecedores = signal<OpcaoSimples[]>([]);
  readonly salvando = signal(false);
  readonly erro = signal<string | null>(null);
  readonly fmtMoeda = fmtMoeda;
  readonly rotulo = rotulo;

  private readonly hoje = new Date().toISOString().slice(0, 10);
  fornecedorId: number | null = null;
  notaFiscal = '';
  dataCompra = this.hoje;
  dataEntrada = this.hoje;
  frete: number | null = null;
  observacoes = '';
  serieNotaFiscal = '';
  dataEmissaoNotaFiscal = '';
  dataVencimentoPagamento = '';
  formaPagamento = '';
  condicaoPagamento = '';
  linhaDigitavelBoleto = '';
  itens: LinhaCompra[] = [{ itemEstoqueId: null, quantidade: null, valorUnitario: null }];

  ngOnInit(): void {
    this.service.listarItens().subscribe((xs: ItemEstoque[]) =>
      this.opcoes.set(xs.filter((i) => i.ativo).map((i) => ({ id: i.id, nome: `${i.nome} (${rotulo(i.unidadeMedida)})` }))),
    );
    this.service.listarFornecedores(true).subscribe((fs) => this.fornecedores.set(fs.map((f) => ({ id: f.id, nome: f.nome }))));
  }

  get freteValor(): number {
    return this.frete ?? 0;
  }

  get totalProdutos(): number {
    return this.itens.reduce((s, l) => s + this.subtotal(l), 0);
  }

  subtotal(l: LinhaCompra): number {
    return (l.quantidade ?? 0) * (l.valorUnitario ?? 0);
  }

  adicionar(): void {
    this.itens = [...this.itens, { itemEstoqueId: null, quantidade: null, valorUnitario: null }];
  }

  remover(i: number): void {
    this.itens = this.itens.filter((_, idx) => idx !== i);
    if (!this.itens.length) {
      this.adicionar();
    }
  }

  salvar(): void {
    this.erro.set(null);
    if (!this.dataCompra || !this.dataEntrada) {
      this.erro.set('Informe a data da compra e a data de entrada.');
      return;
    }
    const itens = this.itens
      .filter((l) => l.itemEstoqueId && (l.quantidade ?? 0) > 0 && (l.valorUnitario ?? 0) > 0)
      .map((l) => ({ itemEstoqueId: l.itemEstoqueId!, quantidade: l.quantidade!, valorUnitario: l.valorUnitario! }));
    if (!itens.length) {
      this.erro.set('Inclua ao menos um item (item + quantidade + valor unitário).');
      return;
    }

    const req: RegistrarCompraRequest = {
      fornecedorId: this.fornecedorId,
      dataCompra: this.dataCompra,
      dataEntrada: this.dataEntrada,
      notaFiscal: this.notaFiscal.trim() || null,
      frete: this.frete ?? null,
      observacoes: this.observacoes.trim() || null,
      itens,
      serieNotaFiscal: this.serieNotaFiscal.trim() || null,
      dataEmissaoNotaFiscal: this.dataEmissaoNotaFiscal || null,
      dataVencimentoPagamento: this.dataVencimentoPagamento || null,
      formaPagamento: this.formaPagamento.trim() || null,
      condicaoPagamento: this.condicaoPagamento.trim() || null,
      linhaDigitavelBoleto: this.linhaDigitavelBoleto.trim() || null,
    };
    this.salvando.set(true);
    this.service.registrarCompra(req).subscribe({
      next: (r) => this.ref.close(r),
      error: (e) => {
        this.salvando.set(false);
        this.erro.set(msgErro(e));
      },
    });
  }
}
