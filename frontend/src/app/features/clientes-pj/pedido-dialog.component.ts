import { Component, Inject, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { EstoqueService } from '../estoque/estoque.service';
import { OpcaoSimples } from '../estoque/estoque.model';
import { Pedido, SalvarPedidoRequest } from './clientes-pj.model';
import { ClientesPjService } from './clientes-pj.service';

function msgErro(e: unknown): string {
  const err = (e as { error?: { errors?: Record<string, string[]>; detail?: string } })?.error;
  const first = err?.errors ? Object.values(err.errors)[0]?.[0] : undefined;
  return first ?? err?.detail ?? 'Não foi possível salvar.';
}

interface LinhaItem {
  receitaId: number | null;
  tamanhoPacoteId: number | null;
  quantidade: number | null;
  observacao: string | null;
}

@Component({
  selector: 'app-pedido-dialog',
  standalone: true,
  imports: [FormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatButtonModule, MatIconModule],
  templateUrl: './pedido-dialog.component.html',
  styleUrl: './pedido-dialog.component.scss',
})
export class PedidoDialogComponent implements OnInit {
  private readonly service = inject(ClientesPjService);
  private readonly estoque = inject(EstoqueService);

  readonly edicao: boolean;
  readonly receitas = signal<OpcaoSimples[]>([]);
  readonly tamanhos = signal<OpcaoSimples[]>([]);
  readonly salvando = signal(false);
  readonly erro = signal<string | null>(null);

  dataPedido = '';
  dataEntrega = '';
  observacoes = '';
  itens: LinhaItem[] = [{ receitaId: null, tamanhoPacoteId: null, quantidade: null, observacao: null }];

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
        receitaId: i.receitaId, tamanhoPacoteId: i.tamanhoPacoteId, quantidade: i.quantidade, observacao: i.observacao,
      }));
      if (!this.itens.length) this.adicionarLinha();
    } else {
      this.dataPedido = hoje;
    }
  }

  ngOnInit(): void {
    this.estoque.listarReceitasCasa().subscribe((r) => this.receitas.set(r));
    this.estoque.listarTamanhos().subscribe((t) => this.tamanhos.set(t));
  }

  adicionarLinha(): void {
    this.itens = [...this.itens, { receitaId: null, tamanhoPacoteId: null, quantidade: null, observacao: null }];
  }

  removerLinha(i: number): void {
    this.itens = this.itens.filter((_, idx) => idx !== i);
    if (!this.itens.length) this.adicionarLinha();
  }

  salvar(): void {
    this.erro.set(null);
    if (!this.dataEntrega) { this.erro.set('Informe a data de entrega.'); return; }
    const itens = this.itens
      .filter((l) => l.receitaId && l.tamanhoPacoteId && (l.quantidade ?? 0) > 0)
      .map((l) => ({ receitaId: l.receitaId!, tamanhoPacoteId: l.tamanhoPacoteId!, quantidade: l.quantidade!, observacao: l.observacao }));
    if (!itens.length) { this.erro.set('Inclua ao menos um item (receita + tamanho + quantidade).'); return; }

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
