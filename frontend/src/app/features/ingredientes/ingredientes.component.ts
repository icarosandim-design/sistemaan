import { AfterViewInit, Component, ViewChild, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialog } from '@angular/material/dialog';
import {
  CATEGORIAS,
  custoRealKg,
  fmtFatorCorrecao,
  fmtMoeda,
  Ingrediente,
  labelTipoConversao,
  MOCK_INGREDIENTES,
} from './ingredientes.model';
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
  ],
  templateUrl: './ingredientes.component.html',
  styleUrl: './ingredientes.component.scss',
})
export class IngredientesComponent implements AfterViewInit {
  private readonly dialog = inject(MatDialog);

  readonly categorias = CATEGORIAS;
  readonly displayedColumns = [
    'nome',
    'categoria',
    'tipoConversao',
    'coeficiente',
    'custoKg',
    'custoRealKg',
    'ativo',
    'acoes',
  ];
  readonly dataSource = new MatTableDataSource<Ingrediente>([...MOCK_INGREDIENTES]);

  readonly fmtMoeda = fmtMoeda;
  readonly fmtFatorCorrecao = fmtFatorCorrecao;
  readonly custoRealKg = custoRealKg;
  readonly labelTipoConversao = labelTipoConversao;

  filtroNome = '';
  filtroCategoria = '';
  filtroStatus = '';

  @ViewChild(MatSort) sort!: MatSort;

  constructor() {
    this.dataSource.filterPredicate = (d, filter) => {
      if (!filter) {
        return true;
      }
      const f = JSON.parse(filter);
      const nomeOk = !f.nome || d.nome.toLowerCase().includes(f.nome);
      const catOk = !f.categoria || d.categoria === f.categoria;
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

  ngAfterViewInit(): void {
    this.dataSource.sort = this.sort;
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
      if (ok) {
        this.dataSource.data = this.dataSource.data.filter((d) => d.id !== ing.id);
      }
    });
  }

  private abrir(ing: Ingrediente | null): void {
    const ref = this.dialog.open(IngredienteDialogComponent, {
      data: { ingrediente: ing },
      width: '560px',
      maxWidth: '95vw',
      autoFocus: false,
    });

    ref.afterClosed().subscribe((res: Ingrediente | undefined) => {
      if (!res) {
        return;
      }
      const dados = this.dataSource.data;
      const idx = dados.findIndex((d) => d.id === res.id);
      if (idx >= 0) {
        dados[idx] = res;
      } else {
        dados.unshift(res);
      }
      this.dataSource.data = [...dados];
    });
  }
}
