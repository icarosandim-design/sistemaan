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
import { fmtPeso, SalvarTamanhoPacoteRequest, TamanhoPacote } from './tamanhos-pacote.model';
import { TamanhosPacoteService } from './tamanhos-pacote.service';
import { TamanhoPacoteDialogComponent } from './tamanho-pacote-dialog.component';

@Component({
  selector: 'app-tamanhos-pacote',
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
  templateUrl: './tamanhos-pacote.component.html',
  styleUrl: './tamanhos-pacote.component.scss',
})
export class TamanhosPacoteComponent implements OnInit {
  private readonly service = inject(TamanhosPacoteService);
  private readonly dialog = inject(MatDialog);
  private readonly snack = inject(MatSnackBar);

  readonly displayedColumns = ['nome', 'peso', 'observacao', 'ativo', 'acoes'];
  readonly fmtPeso = fmtPeso;
  todos: TamanhoPacote[] = [];
  carregando = false;
  filtroStatus = '';

  ngOnInit(): void {
    this.carregar();
  }

  get lista(): TamanhoPacote[] {
    if (!this.filtroStatus) {
      return this.todos;
    }
    return this.todos.filter((t) => (this.filtroStatus === 'ativo' ? t.ativo : !t.ativo));
  }

  carregar(): void {
    this.carregando = true;
    this.service.listar().subscribe({
      next: (ts) => {
        this.todos = ts;
        this.carregando = false;
      },
      error: () => {
        this.carregando = false;
        this.erro('Falha ao carregar tamanhos de pacote.');
      },
    });
  }

  novo(): void {
    this.abrir(null);
  }

  editar(t: TamanhoPacote): void {
    this.abrir(t);
  }

  alternarStatus(t: TamanhoPacote, ev: Event): void {
    ev.stopPropagation();
    this.service.alternarStatus(t.id, !t.ativo).subscribe({
      next: () => {
        this.snack.open(t.ativo ? 'Tamanho inativado.' : 'Tamanho reativado.', 'OK', { duration: 2500 });
        this.carregar();
      },
      error: () => this.erro('Não foi possível alterar o status.'),
    });
  }

  private abrir(t: TamanhoPacote | null): void {
    const ref = this.dialog.open(TamanhoPacoteDialogComponent, {
      data: { tamanho: t },
      width: '460px',
      maxWidth: '95vw',
      autoFocus: false,
    });
    ref.afterClosed().subscribe((req: SalvarTamanhoPacoteRequest | undefined) => {
      if (!req) {
        return;
      }
      const obs = t ? this.service.atualizar(t.id, req) : this.service.criar(req);
      obs.subscribe({
        next: () => {
          this.snack.open(t ? 'Tamanho atualizado.' : 'Tamanho criado.', 'OK', { duration: 2500 });
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
    return e.error?.detail ?? 'Não foi possível salvar o tamanho.';
  }

  private erro(msg: string): void {
    this.snack.open(msg, 'Fechar', { duration: 4000 });
  }
}
