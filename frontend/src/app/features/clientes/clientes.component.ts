import { AfterViewInit, Component, OnInit, ViewChild, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import {
  Cliente,
  labelStatusFinanceiro,
  labelTipo,
  SalvarClienteRequest,
} from './clientes.model';
import { ClientesService } from './clientes.service';
import { ClienteDialogComponent } from './cliente-dialog.component';

@Component({
  selector: 'app-clientes',
  standalone: true,
  imports: [
    FormsModule,
    MatTableModule,
    MatSortModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatTooltipModule,
    MatProgressBarModule,
  ],
  templateUrl: './clientes.component.html',
  styleUrl: './clientes.component.scss',
})
export class ClientesComponent implements OnInit, AfterViewInit {
  private readonly service = inject(ClientesService);
  private readonly dialog = inject(MatDialog);
  private readonly snack = inject(MatSnackBar);

  readonly displayedColumns = ['nome', 'telefone', 'cidade', 'tipo', 'financeiro', 'ativo', 'acoes'];
  readonly dataSource = new MatTableDataSource<Cliente>([]);
  readonly labelTipo = labelTipo;
  readonly labelStatusFinanceiro = labelStatusFinanceiro;

  carregando = false;
  filtroTexto = '';
  filtroStatus = '';

  @ViewChild(MatSort) sort!: MatSort;

  constructor() {
    this.dataSource.filterPredicate = (d, filter) => {
      if (!filter) {
        return true;
      }
      const f = JSON.parse(filter);
      const texto = `${d.nome} ${d.telefone ?? ''} ${d.email ?? ''} ${d.cidade ?? ''}`.toLowerCase();
      const textoOk = !f.texto || texto.includes(f.texto);
      const statusOk = !f.status || (f.status === 'ativo' ? d.ativo : !d.ativo);
      return textoOk && statusOk;
    };
    this.dataSource.sortingDataAccessor = (item, prop) => {
      switch (prop) {
        case 'cidade':
          return item.cidade ?? '';
        default:
          return item.nome;
      }
    };
  }

  ngOnInit(): void {
    this.carregar();
  }

  ngAfterViewInit(): void {
    this.dataSource.sort = this.sort;
  }

  carregar(): void {
    this.carregando = true;
    this.service.listar().subscribe({
      next: (cs) => {
        this.dataSource.data = cs;
        this.carregando = false;
      },
      error: () => {
        this.carregando = false;
        this.erro('Falha ao carregar clientes.');
      },
    });
  }

  aplicarFiltro(): void {
    this.dataSource.filter = JSON.stringify({
      texto: this.filtroTexto.trim().toLowerCase(),
      status: this.filtroStatus,
    });
  }

  get temFiltro(): boolean {
    return !!(this.filtroTexto || this.filtroStatus);
  }

  limparFiltros(): void {
    this.filtroTexto = '';
    this.filtroStatus = '';
    this.aplicarFiltro();
  }

  novo(): void {
    this.abrir(null);
  }

  editar(c: Cliente): void {
    this.abrir(c);
  }

  alternarStatus(c: Cliente, ev: Event): void {
    ev.stopPropagation();
    this.service.alternarStatus(c.id, !c.ativo).subscribe({
      next: () => {
        this.snack.open(c.ativo ? 'Cliente inativado.' : 'Cliente ativado.', 'OK', { duration: 2500 });
        this.carregar();
      },
      error: () => this.erro('Não foi possível alterar o status.'),
    });
  }

  statusClasse(c: Cliente): string {
    return c.statusFinanceiro === 'EmDia' ? 'fin-ok' : c.statusFinanceiro === 'Pendente' ? 'fin-pend' : 'fin-inad';
  }

  private abrir(c: Cliente | null): void {
    const ref = this.dialog.open(ClienteDialogComponent, {
      data: { cliente: c },
      width: '620px',
      maxWidth: '96vw',
      autoFocus: false,
    });
    ref.afterClosed().subscribe((req: SalvarClienteRequest | undefined) => {
      if (!req) {
        return;
      }
      const obs = c ? this.service.atualizar(c.id, req) : this.service.criar(req);
      obs.subscribe({
        next: () => {
          this.snack.open(c ? 'Cliente atualizado.' : 'Cliente criado.', 'OK', { duration: 2500 });
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
    return e.error?.detail ?? 'Não foi possível salvar o cliente.';
  }

  private erro(msg: string): void {
    this.snack.open(msg, 'Fechar', { duration: 4000 });
  }
}
