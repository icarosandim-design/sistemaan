import { Component, Inject, OnInit, inject, signal } from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { ProdutosService } from '../produtos/produtos.service';
import { Produto } from '../produtos/produtos.model';
import { SituacaoEstoqueItem } from '../entregas/entregas.model';
import { Pedido, SalvarPedidoRequest } from './clientes-pj.model';
import { ClientesPjService } from './clientes-pj.service';

function msgErro(e: unknown): string {
  const err = (e as { error?: { errors?: Record<string, string[]>; detail?: string } })?.error;
  const first = err?.errors ? Object.values(err.errors)[0]?.[0] : undefined;
  return first ?? err?.detail ?? 'Não foi possível salvar.';
}

interface LinhaItem {
  produtoId: number | null;
  quantidade: number | null;
  precoUnitario: number | null;
  observacao: string | null;
}

@Component({
  selector: 'app-pedido-dialog',
  standalone: true,
  imports: [CurrencyPipe, FormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatButtonModule, MatIconModule],
  templateUrl: './pedido-dialog.component.html',
  styleUrl: './pedido-dialog.component.scss',
})
export class PedidoDialogComponent implements OnInit {
  private readonly service = inject(ClientesPjService);
  private readonly produtosService = inject(ProdutosService);

  readonly edicao: boolean;
  readonly produtos = signal<Produto[]>([]);
  readonly salvando = signal(false);
  readonly erro = signal<string | null>(null);
  readonly situacao = signal<SituacaoEstoqueItem[]>([]);

  dataPedido = '';
  dataEntrega = '';
  observacoes = '';
  itens: LinhaItem[] = [{ produtoId: null, quantidade: null, precoUnitario: null, observacao: null }];

  constructor(
    readonly ref: MatDialogRef<PedidoDialogComponent, Pedido>,
    @Inject(MAT_DIALOG_DATA) readonly data: { clienteId: number; pedido?: Pedido },
  ) {
    this.edicao = !!data.pedido;
    const hoje = new Date().toISOString().slice(0, 10);
    if (data.pedido) {
      this.dataPedido = data.pedido.dataPedido;
      this.dataEntrega = data.pedido.dataEntrega;
      this.observacoes = data.pedido.observacoes ?? '';
      this.itens = data.pedido.itens.map((i) => ({
        produtoId: i.produtoId, quantidade: i.quantidade, precoUnitario: i.precoUnitario, observacao: i.observacao,
      }));
      if (!this.itens.length) this.adicionarLinha();
    } else {
      this.dataPedido = hoje;
    }
  }

  ngOnInit(): void {
    this.produtosService.listar(false, 'ReceitaDaCasa').subscribe((ps) => this.produtos.set(ps));
    if (this.data.pedido) {
      this.service.situacaoPedido(this.data.pedido.id).subscribe({ next: (s) => this.situacao.set(s), error: () => {} });
    }
  }

  aoEscolherProduto(l: LinhaItem): void {
    const p = this.produtos().find((x) => x.id === l.produtoId);
    if (p && (l.precoUnitario === null || l.precoUnitario === undefined)) {
      l.precoUnitario = p.precoVendaPJ;
    }
  }

  totalPedido(): number {
    return this.itens.reduce((s, l) => s + (l.quantidade ?? 0) * (l.precoUnitario ?? 0), 0);
  }

  adicionarLinha(): void {
    this.itens = [...this.itens, { produtoId: null, quantidade: null, precoUnitario: null, observacao: null }];
  }

  removerLinha(i: number): void {
    this.itens = this.itens.filter((_, idx) => idx !== i);
    if (!this.itens.length) this.adicionarLinha();
  }

  salvar(): void {
    this.erro.set(null);
    if (!this.dataEntrega) { this.erro.set('Informe a data de entrega.'); return; }
    const itens = this.itens
      .filter((l) => l.produtoId && (l.quantidade ?? 0) > 0)
      .map((l) => ({ receitaId: 0, tamanhoPacoteId: 0, quantidade: l.quantidade!, observacao: l.observacao, produtoId: l.produtoId!, precoUnitario: l.precoUnitario ?? null }));
    if (!itens.length) { this.erro.set('Inclua ao menos um item (produto + quantidade).'); return; }

    const req: SalvarPedidoRequest = {
      clienteId: this.data.clienteId,
      dataPedido: this.dataPedido || new Date().toISOString().slice(0, 10),
      dataEntrega: this.dataEntrega,
      observacoes: this.observacoes.trim() || null,
      itens,
    };
    this.salvando.set(true);
    const obs = this.edicao ? this.service.atualizarPedido(this.data.pedido!.id, req) : this.service.criarPedido(req);
    obs.subscribe({
      next: (p) => this.ref.close(p),
      error: (e) => { this.salvando.set(false); this.erro.set(msgErro(e)); },
    });
  }
}
