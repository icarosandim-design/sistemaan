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
import { FaixaConsumo, fmtGramas, fmtKg, SalvarFaixaConsumoRequest } from './consumo.model';
import { ConsumoService } from './consumo.service';
import { FaixaDialogComponent } from './faixa-dialog.component';

@Component({
  selector: 'app-consumo',
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
  templateUrl: './consumo.component.html',
  styleUrl: './consumo.component.scss',
})
export class ConsumoComponent implements OnInit, AfterViewInit {
  private readonly service = inject(ConsumoService);
  private readonly dialog = inject(MatDialog);
  private readonly snack = inject(MatSnackBar);

  readonly displayedColumns = ['pesoInicial', 'pesoFinal', 'gramasPorDia', 'ativo', 'acoes'];
  readonly dataSource = new MatTableDataSource<FaixaConsumo>([]);

  readonly fmtKg = fmtKg;
  readonly fmtGramas = fmtGramas;

  carregando = false;
  filtroStatus = '';

  // Ferramenta de consulta por peso
  pesoConsulta: number | null = null;
  consultaFeita = false;
  resultadoConsulta: FaixaConsumo | null = null;

  @ViewChild(MatSort) sort!: MatSort;

  constructor() {
    this.dataSource.filterPredicate = (d, filter) => {
      if (!filter) {
        return true;
      }
      return filter === 'ativo' ? d.ativo : !d.ativo;
    };
    this.dataSource.sortingDataAccessor = (item, prop) => {
      switch (prop) {
        case 'pesoFinal':
          return item.pesoFinal;
        case 'gramasPorDia':
          return item.gramasPorDia;
        default:
          return item.pesoInicial;
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
      next: (itens) => {
        this.dataSource.data = itens;
        this.carregando = false;
      },
      error: () => {
        this.carregando = false;
        this.erro('Falha ao carregar as faixas de consumo.');
      },
    });
  }

  aplicarFiltro(): void {
    this.dataSource.filter = this.filtroStatus;
  }

  consultarPeso(): void {
    if (this.pesoConsulta == null || this.pesoConsulta <= 0) {
      return;
    }
    this.service.consultar(this.pesoConsulta).subscribe({
      next: (faixa) => {
        this.consultaFeita = true;
        this.resultadoConsulta = faixa;
      },
      error: () => this.erro('Falha ao consultar o peso.'),
    });
  }

  novo(): void {
    this.abrir(null);
  }

  editar(faixa: FaixaConsumo): void {
    this.abrir(faixa);
  }

  private abrir(faixa: FaixaConsumo | null): void {
    const ref = this.dialog.open(FaixaDialogComponent, {
      data: { faixa },
      width: '440px',
      maxWidth: '95vw',
      autoFocus: false,
    });

    ref.afterClosed().subscribe((req: SalvarFaixaConsumoRequest | undefined) => {
      if (!req) {
        return;
      }
      const obs = faixa ? this.service.atualizar(faixa.id, req) : this.service.criar(req);
      obs.subscribe({
        next: () => {
          this.snack.open(faixa ? 'Faixa atualizada.' : 'Faixa criada.', 'OK', { duration: 2500 });
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
    return e.error?.detail ?? 'Não foi possível salvar a faixa.';
  }

  private erro(msg: string): void {
    this.snack.open(msg, 'Fechar', { duration: 4000 });
  }
}
