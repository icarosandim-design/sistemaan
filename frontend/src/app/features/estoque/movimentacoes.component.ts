import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSnackBar } from '@angular/material/snack-bar';
import {
  CATEGORIAS_ESTOQUE,
  fmtMoeda,
  fmtQtd,
  MovimentacaoGeral,
  OpcaoSimples,
  ORIGENS_MOVIMENTACAO,
  rotulo,
  TIPOS_MOVIMENTACAO,
} from './estoque.model';
import { EstoqueService } from './estoque.service';

@Component({
  selector: 'app-movimentacoes',
  standalone: true,
  imports: [
    DatePipe,
    FormsModule,
    MatTableModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatTooltipModule,
    MatProgressBarModule,
    MatPaginatorModule,
  ],
  templateUrl: './movimentacoes.component.html',
  styleUrl: './movimentacoes.component.scss',
})
export class MovimentacoesComponent implements OnInit {
  private readonly service = inject(EstoqueService);
  private readonly snack = inject(MatSnackBar);

  readonly displayedColumns = ['dataHora', 'item', 'tipo', 'quantidade', 'lote', 'custo', 'valor', 'saldo', 'usuario', 'origem'];
  readonly rotulo = rotulo;
  readonly fmtQtd = fmtQtd;
  readonly fmtMoeda = fmtMoeda;
  readonly categorias = CATEGORIAS_ESTOQUE;
  readonly tipos = TIPOS_MOVIMENTACAO;
  readonly origens = ORIGENS_MOVIMENTACAO;

  readonly linhas = signal<MovimentacaoGeral[]>([]);
  readonly total = signal(0);
  readonly carregando = signal(false);
  readonly itens = signal<OpcaoSimples[]>([]);
  readonly fornecedores = signal<OpcaoSimples[]>([]);

  // Filtros
  dataInicio = '';
  dataFim = '';
  itemEstoqueId: number | null = null;
  categoria = '';
  tipo = '';
  fornecedorId: number | null = null;
  origem = '';
  usuario = '';
  motivo = '';

  pagina = 1;
  tamanho = 50;

  ngOnInit(): void {
    this.service.listarItens().subscribe((xs) => this.itens.set(xs.map((i) => ({ id: i.id, nome: i.nome }))));
    this.service.listarFornecedores(true).subscribe((fs) => this.fornecedores.set(fs.map((f) => ({ id: f.id, nome: f.nome }))));
    this.carregar();
  }

  carregar(): void {
    this.carregando.set(true);
    this.service
      .listarMovimentacoesGeral({
        dataInicio: this.dataInicio || null,
        dataFim: this.dataFim || null,
        itemEstoqueId: this.itemEstoqueId,
        categoria: this.categoria || null,
        tipo: this.tipo || null,
        fornecedorId: this.fornecedorId,
        origem: this.origem || null,
        usuario: this.usuario.trim() || null,
        motivo: this.motivo.trim() || null,
        pagina: this.pagina,
        tamanhoPagina: this.tamanho,
      })
      .subscribe({
        next: (r) => {
          this.linhas.set(r.itens);
          this.total.set(r.total);
          this.carregando.set(false);
        },
        error: () => {
          this.carregando.set(false);
          this.snack.open('Falha ao carregar movimentações.', 'Fechar', { duration: 4000 });
        },
      });
  }

  aplicar(): void {
    this.pagina = 1;
    this.carregar();
  }

  limpar(): void {
    this.dataInicio = '';
    this.dataFim = '';
    this.itemEstoqueId = null;
    this.categoria = '';
    this.tipo = '';
    this.fornecedorId = null;
    this.origem = '';
    this.usuario = '';
    this.motivo = '';
    this.pagina = 1;
    this.carregar();
  }

  onPage(e: PageEvent): void {
    this.pagina = e.pageIndex + 1;
    this.tamanho = e.pageSize;
    this.carregar();
  }
}
