import { DatePipe } from '@angular/common';
import { Component, Inject, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { MAT_DIALOG_DATA, MatDialog, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTabsModule } from '@angular/material/tabs';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar } from '@angular/material/snack-bar';
import { forkJoin } from 'rxjs';
import {
  fmtMoeda,
  fmtQtd,
  ItemEstoque,
  LoteEstoque,
  MovimentacaoEstoque,
  rotulo,
} from './estoque.model';
import { EstoqueService } from './estoque.service';
import { AjusteDialogComponent, EntradaDialogComponent, SaidaDialogComponent } from './estoque-operacao-dialogs.component';

export interface ItemEstoqueDetalheData {
  id: number;
}

@Component({
  selector: 'app-item-estoque-detalhe',
  standalone: true,
  imports: [
    DatePipe,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatTabsModule,
    MatProgressBarModule,
  ],
  templateUrl: './item-estoque-detalhe.component.html',
  styleUrl: './item-estoque-detalhe.component.scss',
})
export class ItemEstoqueDetalheComponent {
  private readonly service = inject(EstoqueService);
  private readonly dialog = inject(MatDialog);
  private readonly snack = inject(MatSnackBar);

  readonly rotulo = rotulo;
  readonly fmtQtd = fmtQtd;
  readonly fmtMoeda = fmtMoeda;

  readonly item = signal<ItemEstoque | null>(null);
  readonly lotes = signal<LoteEstoque[]>([]);
  readonly movimentacoes = signal<MovimentacaoEstoque[]>([]);
  readonly carregando = signal(true);
  private alterado = false;

  constructor(
    private readonly ref: MatDialogRef<ItemEstoqueDetalheComponent, boolean>,
    @Inject(MAT_DIALOG_DATA) readonly data: ItemEstoqueDetalheData,
  ) {
    this.recarregar();
  }

  private recarregar(): void {
    this.carregando.set(true);
    forkJoin({
      item: this.service.obterItem(this.data.id),
      lotes: this.service.listarLotes(this.data.id),
      movs: this.service.listarMovimentacoes(this.data.id),
    }).subscribe({
      next: ({ item, lotes, movs }) => {
        this.item.set(item);
        this.lotes.set(lotes);
        this.movimentacoes.set(movs);
        this.carregando.set(false);
      },
      error: () => {
        this.carregando.set(false);
        this.snack.open('Falha ao carregar o item.', 'Fechar', { duration: 4000 });
      },
    });
  }

  entrada(): void {
    const item = this.item();
    if (!item) {
      return;
    }
    const ref = this.dialog.open(EntradaDialogComponent, { data: { item }, autoFocus: false, maxWidth: '96vw' });
    ref.afterClosed().subscribe((req) => {
      if (req) {
        this.executar(this.service.registrarEntrada(req), 'Entrada registrada.');
      }
    });
  }

  saida(): void {
    const item = this.item();
    if (!item) {
      return;
    }
    const ref = this.dialog.open(SaidaDialogComponent, { data: { item }, autoFocus: false, maxWidth: '96vw' });
    ref.afterClosed().subscribe((req) => {
      if (req) {
        this.executar(this.service.registrarSaida(req), 'Saída registrada.');
      }
    });
  }

  ajuste(): void {
    const item = this.item();
    if (!item) {
      return;
    }
    const ref = this.dialog.open(AjusteDialogComponent, { data: { item }, autoFocus: false, maxWidth: '96vw' });
    ref.afterClosed().subscribe((req) => {
      if (req) {
        this.executar(this.service.registrarAjuste(req), 'Ajuste registrado.');
      }
    });
  }

  private executar(obs: ReturnType<EstoqueService['registrarEntrada']>, msg: string): void {
    this.carregando.set(true);
    obs.subscribe({
      next: () => {
        this.alterado = true;
        this.snack.open(msg, 'OK', { duration: 2500 });
        this.recarregar();
      },
      error: (e: HttpErrorResponse) => {
        this.carregando.set(false);
        this.snack.open(this.mensagemErro(e), 'Fechar', { duration: 5000 });
      },
    });
  }

  fechar(): void {
    this.ref.close(this.alterado);
  }

  private mensagemErro(e: HttpErrorResponse): string {
    const errors = e.error?.errors as Record<string, string[]> | undefined;
    if (errors) {
      const primeira = Object.values(errors)[0]?.[0];
      if (primeira) {
        return primeira;
      }
    }
    return e.error?.detail ?? 'Não foi possível concluir a operação.';
  }
}
