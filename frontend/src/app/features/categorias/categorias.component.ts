import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { CATEGORIAS_ESTOQUE, CATEGORIAS_FORNECEDOR, rotulo } from '../estoque/estoque.model';
import { CategoriaIngrediente, SalvarCategoriaIngredienteRequest } from './categorias.model';
import { CategoriasIngredientesService } from './categorias-ingredientes.service';
import { CategoriaIngredienteDialogComponent } from './categoria-ingrediente-dialog.component';

@Component({
  selector: 'app-categorias',
  standalone: true,
  imports: [
    FormsModule,
    MatTabsModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatTooltipModule,
    MatProgressBarModule,
  ],
  templateUrl: './categorias.component.html',
  styleUrl: './categorias.component.scss',
})
export class CategoriasComponent implements OnInit {
  private readonly service = inject(CategoriasIngredientesService);
  private readonly dialog = inject(MatDialog);
  private readonly snack = inject(MatSnackBar);

  readonly rotulo = rotulo;
  readonly displayedColumns = ['ordem', 'nome', 'descricao', 'ativo', 'acoes'];
  readonly categoriasEstoque = CATEGORIAS_ESTOQUE;
  readonly categoriasFornecedor = CATEGORIAS_FORNECEDOR;

  ingredientes: CategoriaIngrediente[] = [];
  carregando = false;

  ngOnInit(): void {
    this.carregar();
  }

  carregar(): void {
    this.carregando = true;
    this.service.listar(true).subscribe({
      next: (cs) => {
        this.ingredientes = cs;
        this.carregando = false;
      },
      error: () => {
        this.carregando = false;
        this.erro('Falha ao carregar categorias.');
      },
    });
  }

  nova(): void {
    this.abrir(null);
  }

  editar(c: CategoriaIngrediente): void {
    this.abrir(c);
  }

  alternarStatus(c: CategoriaIngrediente, ev: Event): void {
    ev.stopPropagation();
    this.service.alternarStatus(c.id, !c.ativo).subscribe({
      next: () => {
        this.snack.open(c.ativo ? 'Categoria inativada.' : 'Categoria reativada.', 'OK', { duration: 2500 });
        this.carregar();
      },
      error: (e: HttpErrorResponse) => this.erro(this.mensagemErro(e)),
    });
  }

  private abrir(c: CategoriaIngrediente | null): void {
    const ref = this.dialog.open(CategoriaIngredienteDialogComponent, {
      data: { categoria: c },
      width: '460px',
      maxWidth: '95vw',
      autoFocus: false,
    });
    ref.afterClosed().subscribe((req: SalvarCategoriaIngredienteRequest | undefined) => {
      if (!req) {
        return;
      }
      const obs = c ? this.service.atualizar(c.id, req) : this.service.criar(req);
      obs.subscribe({
        next: () => {
          this.snack.open(c ? 'Categoria atualizada.' : 'Categoria criada.', 'OK', { duration: 2500 });
          this.carregar();
        },
        error: (e: HttpErrorResponse) => this.erro(this.mensagemErro(e)),
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
    return e.error?.detail ?? 'Não foi possível salvar a categoria.';
  }

  private erro(msg: string): void {
    this.snack.open(msg, 'Fechar', { duration: 4000 });
  }
}
