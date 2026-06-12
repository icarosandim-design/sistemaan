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
  fmtCpf,
  fmtMoeda,
  labelStatusFinanceiro,
  labelTipo,
  SalvarClienteRequest,
} from './clientes.model';
import { petsMockDoCliente } from './pets/pet.model';
import { ClientesService } from './clientes.service';
import { ClienteDialogComponent } from './cliente-dialog.component';
import { CancelarDialogComponent } from './cancelar-dialog.component';

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

  readonly displayedColumns = ['nome', 'pets', 'telefone', 'cidade', 'tipo', 'mensalidade', 'financeiro', 'situacao', 'acoes'];
  readonly dataSource = new MatTableDataSource<Cliente>([]);
  readonly labelTipo = labelTipo;
  readonly labelStatusFinanceiro = labelStatusFinanceiro;
  readonly fmtCpf = fmtCpf;
  readonly fmtMoeda = fmtMoeda;
  readonly petsDoCliente = petsMockDoCliente;

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
      const texto = `${d.nome} ${d.cpf ?? ''} ${d.telefone ?? ''} ${d.email ?? ''} ${d.cidade ?? ''}`.toLowerCase();
      const textoOk = !f.texto || texto.includes(f.texto);
      const statusOk = !f.status || (f.status === 'ativo' ? d.ativo : !d.ativo);
      return textoOk && statusOk;
    };
    this.dataSource.sortingDataAccessor = (item, prop) => (prop === 'cidade' ? item.cidade ?? '' : item.nome);
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

  cancelar(c: Cliente, ev: Event): void {
    ev.stopPropagation();
    const ref = this.dialog.open(CancelarDialogComponent, {
      data: { nome: c.nome },
      width: '460px',
      maxWidth: '95vw',
      autoFocus: false,
    });
    ref.afterClosed().subscribe((motivo: string | undefined) => {
      if (!motivo) {
        return;
      }
      this.service.cancelar(c.id, motivo).subscribe({
        next: () => {
          this.snack.open('Cliente cancelado.', 'OK', { duration: 2500 });
          this.carregar();
        },
        error: (e: HttpErrorResponse) => this.erro(this.mensagemErro(e)),
      });
    });
  }

  reativar(c: Cliente, ev: Event): void {
    ev.stopPropagation();
    this.service.reativar(c.id).subscribe({
      next: () => {
        this.snack.open('Cliente reativado.', 'OK', { duration: 2500 });
        this.carregar();
      },
      error: () => this.erro('Não foi possível reativar o cliente.'),
    });
  }

  statusFinClasse(c: Cliente): string {
    return c.statusFinanceiro === 'EmDia' ? 'fin-ok' : c.statusFinanceiro === 'Pendente' ? 'fin-pend' : 'fin-inad';
  }

  private abrir(c: Cliente | null): void {
    const ref = this.dialog.open(ClienteDialogComponent, {
      data: { cliente: c },
      width: '680px',
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
    return e.error?.detail ?? 'Não foi possível concluir a operação.';
  }

  private erro(msg: string): void {
    this.snack.open(msg, 'Fechar', { duration: 4000 });
  }
}
