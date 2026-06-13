import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { MatTableModule } from '@angular/material/table';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import {
  CATEGORIAS_ESTOQUE,
  EntradaCompra,
  fmtMoeda,
  fmtQtd,
  OpcaoSimples,
  rotulo,
} from './estoque.model';
import { EstoqueService } from './estoque.service';
import { EntradaDialogComponent } from './estoque-operacao-dialogs.component';

@Component({
  selector: 'app-compras',
  standalone: true,
  imports: [
    DatePipe,
    FormsModule,
    MatTableModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatProgressBarModule,
  ],
  templateUrl: './compras.component.html',
  styleUrl: './compras.component.scss',
})
export class ComprasComponent implements OnInit {
  private readonly service = inject(EstoqueService);
  private readonly dialog = inject(MatDialog);
  private readonly snack = inject(MatSnackBar);

  readonly displayedColumns = ['data', 'fornecedor', 'item', 'quantidade', 'valorUnitario', 'custo', 'produtos', 'frete', 'total', 'lote'];
  readonly rotulo = rotulo;
  readonly fmtQtd = fmtQtd;
  readonly fmtMoeda = fmtMoeda;
  readonly categorias = CATEGORIAS_ESTOQUE;

  readonly linhas = signal<EntradaCompra[]>([]);
  readonly carregando = signal(false);
  readonly itens = signal<OpcaoSimples[]>([]);
  readonly fornecedores = signal<OpcaoSimples[]>([]);

  dataInicio = '';
  dataFim = '';
  fornecedorId: number | null = null;
  itemEstoqueId: number | null = null;
  categoria = '';
  comFrete = '';

  ngOnInit(): void {
    this.service.listarItens().subscribe((xs) => this.itens.set(xs.map((i) => ({ id: i.id, nome: i.nome }))));
    this.service.listarFornecedores(true).subscribe((fs) => this.fornecedores.set(fs.map((f) => ({ id: f.id, nome: f.nome }))));
    this.carregar();
  }

  carregar(): void {
    this.carregando.set(true);
    this.service
      .listarEntradas({
        dataInicio: this.dataInicio || null,
        dataFim: this.dataFim || null,
        fornecedorId: this.fornecedorId,
        itemEstoqueId: this.itemEstoqueId,
        categoria: this.categoria || null,
        comFrete: this.comFrete === '' ? null : this.comFrete === 'sim',
      })
      .subscribe({
        next: (r) => {
          this.linhas.set(r);
          this.carregando.set(false);
        },
        error: () => {
          this.carregando.set(false);
          this.snack.open('Falha ao carregar entradas.', 'Fechar', { duration: 4000 });
        },
      });
  }

  aplicar(): void {
    this.carregar();
  }

  limpar(): void {
    this.dataInicio = '';
    this.dataFim = '';
    this.fornecedorId = null;
    this.itemEstoqueId = null;
    this.categoria = '';
    this.comFrete = '';
    this.carregar();
  }

  novaEntrada(): void {
    const ref = this.dialog.open(EntradaDialogComponent, {
      data: { item: null },
      width: '620px',
      maxWidth: '96vw',
      autoFocus: false,
    });
    ref.afterClosed().subscribe((req) => {
      if (!req) {
        return;
      }
      this.carregando.set(true);
      this.service.registrarEntrada(req).subscribe({
        next: () => {
          this.snack.open('Entrada registrada.', 'OK', { duration: 2500 });
          this.carregar();
        },
        error: (e: HttpErrorResponse) => {
          this.carregando.set(false);
          this.snack.open(this.mensagemErro(e), 'Fechar', { duration: 5000 });
        },
      });
    });
  }

  private mensagemErro(e: HttpErrorResponse): string {
    const errors = e.error?.errors as Record<string, string[]> | undefined;
    if (errors) {
      const primeira = Object.values(errors)[0]?.[0];
      if (primeira) {
        return primeira;
      }
    }
    return e.error?.detail ?? 'Não foi possível registrar a entrada.';
  }
}
