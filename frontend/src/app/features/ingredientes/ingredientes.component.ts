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
import {
  Categoria,
  custoRealKg,
  fmtFatorCorrecao,
  fmtMoeda,
  Ingrediente,
  labelTipoConversao,
  SalvarIngredienteRequest,
} from './ingredientes.model';
import { IngredientesService } from './ingredientes.service';
import { IngredienteDialogComponent } from './ingrediente-dialog.component';
import { ConfirmDialogComponent } from '../../shared/confirm-dialog.component';

@Component({
  selector: 'app-ingredientes',
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
  templateUrl: './ingredientes.component.html',
  styleUrl: './ingredientes.component.scss',
})
export class IngredientesComponent implements OnInit, AfterViewInit {
  private readonly dialog = inject(MatDialog);
  private readonly service = inject(IngredientesService);
  private readonly snack = inject(MatSnackBar);

  readonly displayedColumns = ['nome', 'categoria', 'tipoConversao', 'coeficiente', 'custoKg', 'custoRealKg', 'ativo', 'acoes'];
  readonly dataSource = new MatTableDataSource<Ingrediente>([]);

  readonly fmtMoeda = fmtMoeda;
  readonly fmtFatorCorrecao = fmtFatorCorrecao;
  readonly custoRealKg = custoRealKg;
  readonly labelTipoConversao = labelTipoConversao;

  categorias: Categoria[] = [];
  carregando = false;

  filtroNome = '';
  filtroCategoria: number | '' = '';
  filtroStatus = '';

  @ViewChild(MatSort) sort!: MatSort;

  constructor() {
    this.dataSource.filterPredicate = (d, filter) => {
      if (!filter) {
        return true;
      }
      const f = JSON.parse(filter);
      const nomeOk = !f.nome || d.nome.toLowerCase().includes(f.nome);
      const catOk = !f.categoria || d.categoriaId === f.categoria;
      const statusOk = !f.status || (f.status === 'ativo' ? d.ativo : !d.ativo);
      return nomeOk && catOk && statusOk;
    };

    this.dataSource.sortingDataAccessor = (item, prop) => {
      switch (prop) {
        case 'custoKg':
          return item.custoKg;
        case 'custoRealKg':
          return custoRealKg(item.custoKg, item.coeficiente);
        case 'categoria':
          return item.categoria;
        default:
          return item.nome;
      }
    };
  }

  ngOnInit(): void {
    this.service.listarCategorias().subscribe({
      next: (cats) => (this.categorias = cats),
      error: () => this.erro('Falha ao carregar categorias.'),
    });
    this.carregar();
  }

  ngAfterViewInit(): void {
    this.dataSource.sort = this.sort;
  }

  carregar(): void {
    this.carregando = true;
    this.service.listar().subscribe({
      next: (itens) => {
        this.dataSource.data = itens;
        this.carregando = false;
      },
      error: () => {
        this.carregando = false;
        this.erro('Falha ao carregar ingredientes.');
      },
    });
  }

  aplicarFiltro(): void {
    this.dataSource.filter = JSON.stringify({
      nome: this.filtroNome.trim().toLowerCase(),
      categoria: this.filtroCategoria,
      status: this.filtroStatus,
    });
  }

  limparFiltros(): void {
    this.filtroNome = '';
    this.filtroCategoria = '';
    this.filtroStatus = '';
    this.aplicarFiltro();
  }

  get temFiltro(): boolean {
    return !!(this.filtroNome || this.filtroCategoria || this.filtroStatus);
  }

  novo(): void {
    this.abrir(null);
  }

  editar(ing: Ingrediente): void {
    this.abrir(ing);
  }

  excluir(ing: Ingrediente, ev: Event): void {
    ev.stopPropagation();
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: {
        titulo: 'Excluir ingrediente',
        mensagem: `Excluir "${ing.nome}"? Esta ação não pode ser desfeita.`,
        confirmar: 'Excluir',
        perigo: true,
      },
      width: '420px',
      maxWidth: '95vw',
      autoFocus: false,
    });
    ref.afterClosed().subscribe((ok: boolean | undefined) => {
      if (!ok) {
        return;
      }
      this.service.excluir(ing.id).subscribe({
        next: () => {
          this.snack.open('Ingrediente excluído.', 'OK', { duration: 2500 });
          this.carregar();
        },
        error: () => this.erro('Não foi possível excluir o ingrediente.'),
      });
    });
  }

  private abrir(ing: Ingrediente | null): void {
    const ref = this.dialog.open(IngredienteDialogComponent, {
      data: { ingrediente: ing, categorias: this.categorias },
      width: '560px',
      maxWidth: '95vw',
      autoFocus: false,
    });

    ref.afterClosed().subscribe((req: SalvarIngredienteRequest | undefined) => {
      if (!req) {
        return;
      }
      const obs = ing ? this.service.atualizar(ing.id, req) : this.service.criar(req);
      obs.subscribe({
        next: () => {
          this.snack.open(ing ? 'Ingrediente atualizado.' : 'Ingrediente criado.', 'OK', { duration: 2500 });
          this.carregar();
        },
        error: () => this.erro('Não foi possível salvar o ingrediente.'),
      });
    });
  }

  private erro(msg: string): void {
    this.snack.open(msg, 'Fechar', { duration: 4000 });
  }
}
