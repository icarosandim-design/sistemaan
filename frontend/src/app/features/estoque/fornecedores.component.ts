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
import { Fornecedor, rotulo, SalvarFornecedorRequest } from './estoque.model';
import { EstoqueService } from './estoque.service';
import { FornecedorDialogComponent } from './fornecedor-dialog.component';

@Component({
  selector: 'app-fornecedores',
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
  templateUrl: './fornecedores.component.html',
  styleUrl: './fornecedores.component.scss',
})
export class FornecedoresComponent implements OnInit {
  private readonly service = inject(EstoqueService);
  private readonly dialog = inject(MatDialog);
  private readonly snack = inject(MatSnackBar);

  readonly displayedColumns = ['nome', 'categoria', 'contato', 'cidade', 'ativo', 'acoes'];
  readonly rotulo = rotulo;
  todos: Fornecedor[] = [];
  carregando = false;
  filtroStatus = '';

  ngOnInit(): void {
    this.carregar();
  }

  get lista(): Fornecedor[] {
    if (!this.filtroStatus) {
      return this.todos;
    }
    return this.todos.filter((f) => (this.filtroStatus === 'ativo' ? f.ativo : !f.ativo));
  }

  carregar(): void {
    this.carregando = true;
    this.service.listarFornecedores().subscribe({
      next: (fs) => {
        this.todos = fs;
        this.carregando = false;
      },
      error: () => {
        this.carregando = false;
        this.erro('Falha ao carregar fornecedores.');
      },
    });
  }

  novo(): void {
    this.abrir(null);
  }

  editar(f: Fornecedor): void {
    this.abrir(f);
  }

  alternarStatus(f: Fornecedor, ev: Event): void {
    ev.stopPropagation();
    this.service.alternarStatusFornecedor(f.id, !f.ativo).subscribe({
      next: () => {
        this.snack.open(f.ativo ? 'Fornecedor inativado.' : 'Fornecedor reativado.', 'OK', { duration: 2500 });
        this.carregar();
      },
      error: (e: HttpErrorResponse) => this.erro(this.mensagemErro(e)),
    });
  }

  private abrir(f: Fornecedor | null): void {
    const ref = this.dialog.open(FornecedorDialogComponent, {
      data: { fornecedor: f },
      width: '640px',
      maxWidth: '96vw',
      autoFocus: false,
    });
    ref.afterClosed().subscribe((req: SalvarFornecedorRequest | undefined) => {
      if (!req) {
        return;
      }
      const obs = f ? this.service.atualizarFornecedor(f.id, req) : this.service.criarFornecedor(req);
      obs.subscribe({
        next: () => {
          this.snack.open(f ? 'Fornecedor atualizado.' : 'Fornecedor criado.', 'OK', { duration: 2500 });
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
    return e.error?.detail ?? 'Não foi possível salvar o fornecedor.';
  }

  private erro(msg: string): void {
    this.snack.open(msg, 'Fechar', { duration: 4000 });
  }
}
