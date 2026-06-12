import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { MatTableModule } from '@angular/material/table';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { fmtMoeda, fmtQtd, ItemEstoque, rotulo } from './estoque.model';
import { EstoqueService } from './estoque.service';
import { ItemEstoqueDialogComponent } from './item-estoque-dialog.component';
import { ItemEstoqueDetalheComponent } from './item-estoque-detalhe.component';

@Component({
  selector: 'app-itens-estoque',
  standalone: true,
  imports: [
    FormsModule,
    MatTableModule,
    MatFormFieldModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatTooltipModule,
    MatProgressBarModule,
  ],
  templateUrl: './itens-estoque.component.html',
  styleUrl: './itens-estoque.component.scss',
})
export class ItensEstoqueComponent implements OnInit {
  private readonly service = inject(EstoqueService);
  private readonly dialog = inject(MatDialog);
  private readonly snack = inject(MatSnackBar);

  readonly displayedColumns = ['nome', 'tipo', 'categoria', 'saldo', 'custo', 'minimo', 'ativo', 'acoes'];
  readonly rotulo = rotulo;
  readonly fmtQtd = fmtQtd;
  readonly fmtMoeda = fmtMoeda;

  todos: ItemEstoque[] = [];
  carregando = false;
  filtroTipo = '';
  filtroStatus = '';
  filtroAlerta = '';

  ngOnInit(): void {
    this.carregar();
  }

  get lista(): ItemEstoque[] {
    return this.todos.filter((i) => {
      if (this.filtroTipo && i.tipo !== this.filtroTipo) {
        return false;
      }
      if (this.filtroStatus && (this.filtroStatus === 'ativo') !== i.ativo) {
        return false;
      }
      if (this.filtroAlerta === 'minimo' && !i.abaixoDoMinimo) {
        return false;
      }
      return true;
    });
  }

  get qtdAbaixoMinimo(): number {
    return this.todos.filter((i) => i.abaixoDoMinimo && i.ativo).length;
  }

  carregar(): void {
    this.carregando = true;
    this.service.listarItens().subscribe({
      next: (xs) => {
        this.todos = xs;
        this.carregando = false;
      },
      error: () => {
        this.carregando = false;
        this.erro('Falha ao carregar itens de estoque.');
      },
    });
  }

  novo(): void {
    const ref = this.dialog.open(ItemEstoqueDialogComponent, {
      data: { item: null },
      width: '620px',
      maxWidth: '96vw',
      autoFocus: false,
    });
    ref.afterClosed().subscribe((salvou) => {
      if (salvou) {
        this.snack.open('Item criado.', 'OK', { duration: 2500 });
        this.carregar();
      }
    });
  }

  editar(item: ItemEstoque, ev?: Event): void {
    ev?.stopPropagation();
    const ref = this.dialog.open(ItemEstoqueDialogComponent, {
      data: { item },
      width: '620px',
      maxWidth: '96vw',
      autoFocus: false,
    });
    ref.afterClosed().subscribe((salvou) => {
      if (salvou) {
        this.snack.open('Item atualizado.', 'OK', { duration: 2500 });
        this.carregar();
      }
    });
  }

  abrirDetalhe(item: ItemEstoque): void {
    const ref = this.dialog.open(ItemEstoqueDetalheComponent, {
      data: { id: item.id },
      width: '820px',
      maxWidth: '96vw',
      autoFocus: false,
    });
    ref.afterClosed().subscribe((alterado) => {
      if (alterado) {
        this.carregar();
      }
    });
  }

  alternarStatus(item: ItemEstoque, ev: Event): void {
    ev.stopPropagation();
    this.service.alternarStatusItem(item.id, !item.ativo).subscribe({
      next: () => {
        this.snack.open(item.ativo ? 'Item inativado.' : 'Item reativado.', 'OK', { duration: 2500 });
        this.carregar();
      },
      error: (e: HttpErrorResponse) => this.erro(this.mensagemErro(e)),
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
    return e.error?.detail ?? 'Não foi possível concluir a ação.';
  }

  private erro(msg: string): void {
    this.snack.open(msg, 'Fechar', { duration: 4000 });
  }
}
