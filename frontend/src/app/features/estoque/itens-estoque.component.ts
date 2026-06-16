import { Component, OnInit, inject } from '@angular/core';
import { NgTemplateOutlet } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { MatTableModule } from '@angular/material/table';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTabsModule } from '@angular/material/tabs';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { fmtMoeda, fmtQtd, ItemEstoque, PersonalizadaPronta, rotulo } from './estoque.model';
import { EstoqueService } from './estoque.service';
import { ItemEstoqueDialogComponent } from './item-estoque-dialog.component';
import { ItemEstoqueDetalheComponent } from './item-estoque-detalhe.component';

@Component({
  selector: 'app-itens-estoque',
  standalone: true,
  imports: [
    NgTemplateOutlet,
    FormsModule,
    MatTableModule,
    MatFormFieldModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatTooltipModule,
    MatProgressBarModule,
    MatTabsModule,
  ],
  templateUrl: './itens-estoque.component.html',
  styleUrl: './itens-estoque.component.scss',
})
export class ItensEstoqueComponent implements OnInit {
  private readonly service = inject(EstoqueService);
  private readonly dialog = inject(MatDialog);
  private readonly snack = inject(MatSnackBar);

  /** Colunas das abas de cadastro (Insumos / Produto acabado) — sem a coluna "tipo" (a aba já é o tipo). */
  readonly colsCadastro = ['nome', 'categoria', 'saldo', 'custo', 'minimo', 'ativo', 'acoes'];
  readonly colsPersonalizada = ['receita', 'pet', 'cliente', 'tamanho', 'prontos', 'data', 'status'];
  readonly rotulo = rotulo;
  readonly fmtQtd = fmtQtd;
  readonly fmtMoeda = fmtMoeda;

  todos: ItemEstoque[] = [];
  personalizadas: PersonalizadaPronta[] = [];
  carregando = false;
  abaSelecionada = 0;
  filtroStatus = '';
  filtroAlerta = '';

  ngOnInit(): void {
    this.carregar();
  }

  /** Insumos filtrados por status/alerta. */
  get insumos(): ItemEstoque[] {
    return this.filtrar(this.todos.filter((i) => i.tipo === 'Insumo'));
  }

  /** Produto acabado da casa filtrado por status/alerta. */
  get produtosAcabados(): ItemEstoque[] {
    return this.filtrar(this.todos.filter((i) => i.tipo === 'ProdutoAcabadoCasa'));
  }

  private filtrar(itens: ItemEstoque[]): ItemEstoque[] {
    return itens.filter((i) => {
      if (this.filtroStatus && (this.filtroStatus === 'ativo') !== i.ativo) {
        return false;
      }
      if (this.filtroAlerta === 'minimo' && !i.abaixoDoMinimo) {
        return false;
      }
      return true;
    });
  }

  get totalPersonalizadasProntas(): number {
    return this.personalizadas.reduce((s, p) => s + p.pacotesProntos, 0);
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
    this.service.listarPersonalizadasProntas().subscribe({
      next: (xs) => (this.personalizadas = xs),
      error: () => this.erro('Falha ao carregar personalizadas prontas.'),
    });
  }

  fmtPeso(g: number): string {
    return g >= 1000 ? `${(g / 1000).toLocaleString('pt-BR', { maximumFractionDigits: 3 })} kg` : `${g} g`;
  }

  fmtData(iso: string): string {
    if (!iso) {
      return '—';
    }
    const [y, m, d] = iso.split('-');
    return `${d}/${m}/${y}`;
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
