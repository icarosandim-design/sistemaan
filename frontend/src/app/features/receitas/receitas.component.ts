import { AfterViewInit, Component, OnInit, ViewChild, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
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
import { custoReceita, fmtMoeda, IngredienteAtivo, ReceitaCasa } from './receitas.model';
import { ReceitasService } from './receitas.service';
import { ReceitaDialogComponent } from './receita-dialog.component';

@Component({
  selector: 'app-receitas',
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
  templateUrl: './receitas.component.html',
  styleUrl: './receitas.component.scss',
})
export class ReceitasComponent implements OnInit, AfterViewInit {
  private readonly service = inject(ReceitasService);
  private readonly dialog = inject(MatDialog);
  private readonly snack = inject(MatSnackBar);

  readonly displayedColumns = ['codigo', 'nome', 'base', 'custo', 'ativo', 'acoes'];
  readonly dataSource = new MatTableDataSource<ReceitaCasa>([]);
  readonly fmtMoeda = fmtMoeda;

  private ingredientes: IngredienteAtivo[] = [];
  private mapa = new Map<number, IngredienteAtivo>();

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
      const texto = `${d.codigo} ${d.nome}`.toLowerCase();
      const textoOk = !f.texto || texto.includes(f.texto);
      const statusOk = !f.status || (f.status === 'ativo' ? d.ativo : !d.ativo);
      return textoOk && statusOk;
    };
    this.dataSource.sortingDataAccessor = (item, prop) => {
      switch (prop) {
        case 'codigo':
          return item.codigo;
        case 'custo':
          return this.custoDe(item);
        default:
          return item.nome;
      }
    };
  }

  ngOnInit(): void {
    this.service.listarIngredientes().subscribe((ings) => {
      this.ingredientes = ings;
      this.mapa = new Map(ings.map((i) => [i.id, i]));
    });
    this.carregar();
  }

  ngAfterViewInit(): void {
    this.dataSource.sort = this.sort;
  }

  carregar(): void {
    this.carregando = true;
    this.service.listar().subscribe({
      next: (rs) => {
        this.dataSource.data = rs;
        this.carregando = false;
      },
      error: () => {
        this.carregando = false;
        this.snack.open('Falha ao carregar receitas.', 'Fechar', { duration: 4000 });
      },
    });
  }

  custoDe(r: ReceitaCasa): number {
    return custoReceita(r.itens, this.mapa);
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

  editar(r: ReceitaCasa): void {
    this.abrir(r);
  }

  alternarStatus(r: ReceitaCasa, ev: Event): void {
    ev.stopPropagation();
    this.service.alternarStatus(r.id).subscribe(() => {
      this.snack.open(r.ativo ? 'Receita inativada.' : 'Receita ativada.', 'OK', { duration: 2500 });
      this.carregar();
    });
  }

  private abrir(r: ReceitaCasa | null): void {
    const ref = this.dialog.open(ReceitaDialogComponent, {
      data: { receita: r, ingredientes: this.ingredientes },
      width: '720px',
      maxWidth: '96vw',
      autoFocus: false,
    });
    ref.afterClosed().subscribe((res: ReceitaCasa | undefined) => {
      if (!res) {
        return;
      }
      this.service.salvar(res).subscribe(() => {
        this.snack.open(r ? 'Receita atualizada.' : 'Receita criada.', 'OK', { duration: 2500 });
        this.carregar();
      });
    });
  }
}
