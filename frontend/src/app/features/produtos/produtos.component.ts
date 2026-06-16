import { Component, OnInit, inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ProdutosService } from './produtos.service';
import { Produto, SalvarProdutoRequest, TIPOS_PRODUTO, labelTipoProduto } from './produtos.model';
import { ProdutoDialogComponent } from './produto-dialog.component';

@Component({
  selector: 'app-produtos',
  standalone: true,
  imports: [FormsModule, MatTableModule, MatButtonModule, MatIconModule, MatTooltipModule, MatProgressBarModule],
  template: `
    <section class="pagina">
      <header class="page-head">
        <div class="page-title">
          <h1>Produtos</h1>
          <p class="subtitulo">Itens comerciais vendidos/entregues e controlados em estoque (de Receita da Casa ou avulsos, como petiscos).</p>
        </div>
        <button mat-flat-button class="btn-cta" (click)="nova()"><mat-icon>add</mat-icon> Novo produto</button>
      </header>

      <div class="filtros">
        <label>Tipo
          <select [(ngModel)]="filtroTipo" (ngModelChange)="carregar()">
            <option value="">Todos</option>
            @for (t of tipos; track t.valor) { <option [value]="t.valor">{{ t.label }}</option> }
          </select>
        </label>
      </div>

      <div class="tabela-card">
        @if (carregando) { <mat-progress-bar mode="indeterminate"></mat-progress-bar> }
        <table mat-table [dataSource]="produtos" class="tabela">
          <ng-container matColumnDef="nome">
            <th mat-header-cell *matHeaderCellDef>Produto</th>
            <td mat-cell *matCellDef="let p" class="cel-nome">{{ p.nome }}</td>
          </ng-container>
          <ng-container matColumnDef="tipo">
            <th mat-header-cell *matHeaderCellDef>Tipo</th>
            <td mat-cell *matCellDef="let p" class="cel-sec">{{ label(p.tipo) }}</td>
          </ng-container>
          <ng-container matColumnDef="receita">
            <th mat-header-cell *matHeaderCellDef>Receita / Tamanho</th>
            <td mat-cell *matCellDef="let p" class="cel-sec">{{ p.receitaCasaNome ? p.receitaCasaNome + (p.tamanhoPacoteNome ? ' · ' + p.tamanhoPacoteNome : '') : '—' }}</td>
          </ng-container>
          <ng-container matColumnDef="pf">
            <th mat-header-cell *matHeaderCellDef class="t-r">Preço PF</th>
            <td mat-cell *matCellDef="let p" class="t-r">{{ moeda(p.precoVendaAvulsaPF) }}</td>
          </ng-container>
          <ng-container matColumnDef="pj">
            <th mat-header-cell *matHeaderCellDef class="t-r">Preço PJ</th>
            <td mat-cell *matCellDef="let p" class="t-r">{{ moeda(p.precoVendaPJ) }}</td>
          </ng-container>
          <ng-container matColumnDef="estoque">
            <th mat-header-cell *matHeaderCellDef class="t-center">Estoque</th>
            <td mat-cell *matCellDef="let p" class="t-center">{{ p.controlaEstoque ? 'Sim' : 'Não' }}</td>
          </ng-container>
          <ng-container matColumnDef="ativo">
            <th mat-header-cell *matHeaderCellDef class="t-center">Status</th>
            <td mat-cell *matCellDef="let p" class="t-center">
              <span class="chip" [class.chip-inativo]="!p.ativo">{{ p.ativo ? 'Ativo' : 'Inativo' }}</span>
            </td>
          </ng-container>
          <ng-container matColumnDef="acoes">
            <th mat-header-cell *matHeaderCellDef></th>
            <td mat-cell *matCellDef="let p" class="t-acoes">
              <button mat-icon-button (click)="editar(p); $event.stopPropagation()" matTooltip="Editar"><mat-icon>edit</mat-icon></button>
              <button mat-icon-button (click)="alternarStatus(p, $event)" [matTooltip]="p.ativo ? 'Inativar' : 'Reativar'">
                <mat-icon>{{ p.ativo ? 'block' : 'check_circle' }}</mat-icon>
              </button>
            </td>
          </ng-container>
          <tr mat-header-row *matHeaderRowDef="cols; sticky: true"></tr>
          <tr mat-row *matRowDef="let row; columns: cols" class="linha" (click)="editar(row)"></tr>
          <tr class="sem-dados" *matNoDataRow><td [attr.colspan]="cols.length">Nenhum produto cadastrado.</td></tr>
        </table>
      </div>
    </section>
  `,
  styles: [`
    .pagina { display: flex; flex-direction: column; gap: 1rem; max-width: 1100px; margin: 0 auto; }
    .page-head { display: flex; align-items: flex-start; justify-content: space-between; gap: 1rem; flex-wrap: wrap; }
    .page-title h1 { margin: 0; font-size: 1.5rem; font-weight: 700; }
    .subtitulo { margin: 0.35rem 0 0; font-size: 0.85rem; color: var(--an-texto-secundario); }
    .btn-cta { background: var(--an-cta); color: #fff; }
    .filtros label { display: flex; align-items: center; gap: 0.4rem; font-size: 0.82rem; color: var(--an-texto-secundario); }
    .filtros select { padding: 0.32rem 0.45rem; border: 1px solid var(--an-fundo-secundario); border-radius: 6px; }
    .tabela-card { background: var(--an-superficie); border: 1px solid var(--an-fundo-secundario); border-radius: var(--an-raio); overflow: hidden; }
    .tabela { width: 100%; }
    .t-center { text-align: center; } .t-r { text-align: right; } .t-acoes { text-align: right; white-space: nowrap; }
    .cel-nome { font-weight: 600; color: var(--an-texto-titulo); } .cel-sec { color: var(--an-texto-secundario); }
    .linha { cursor: pointer; }
    .chip { background: rgba(63,79,45,0.12); color: var(--an-primaria); border-radius: 12px; padding: 0.1rem 0.6rem; font-size: 0.78rem; font-weight: 600; }
    .chip-inativo { background: rgba(0,0,0,0.08); color: var(--an-texto-secundario); }
    .sem-dados td { padding: 1.5rem; text-align: center; color: var(--an-texto-secundario); }
  `],
})
export class ProdutosComponent implements OnInit {
  private readonly service = inject(ProdutosService);
  private readonly dialog = inject(MatDialog);
  private readonly snack = inject(MatSnackBar);

  readonly cols = ['nome', 'tipo', 'receita', 'pf', 'pj', 'estoque', 'ativo', 'acoes'];
  readonly tipos = TIPOS_PRODUTO;
  readonly label = labelTipoProduto;
  produtos: Produto[] = [];
  carregando = false;
  filtroTipo = '';

  ngOnInit(): void { this.carregar(); }

  carregar(): void {
    this.carregando = true;
    this.service.listar(true, this.filtroTipo || undefined).subscribe({
      next: (ps) => { this.produtos = ps; this.carregando = false; },
      error: () => { this.carregando = false; this.erro('Falha ao carregar produtos.'); },
    });
  }

  moeda(v: number): string { return v.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' }); }

  nova(): void { this.abrir(null); }
  editar(p: Produto): void { this.abrir(p); }

  alternarStatus(p: Produto, ev: Event): void {
    ev.stopPropagation();
    this.service.alternarStatus(p.id, !p.ativo).subscribe({
      next: () => { this.snack.open(p.ativo ? 'Produto inativado.' : 'Produto reativado.', 'OK', { duration: 2500 }); this.carregar(); },
      error: (e: HttpErrorResponse) => this.erro(this.msg(e)),
    });
  }

  private abrir(p: Produto | null): void {
    const ref = this.dialog.open(ProdutoDialogComponent, { data: { produto: p }, width: '560px', maxWidth: '95vw', autoFocus: false });
    ref.afterClosed().subscribe((req: SalvarProdutoRequest | undefined) => {
      if (!req) return;
      const obs = p ? this.service.atualizar(p.id, req) : this.service.criar(req);
      obs.subscribe({
        next: () => { this.snack.open(p ? 'Produto atualizado.' : 'Produto criado.', 'OK', { duration: 2500 }); this.carregar(); },
        error: (e: HttpErrorResponse) => this.erro(this.msg(e)),
      });
    });
  }

  private msg(e: HttpErrorResponse): string {
    const errors = e.error?.errors as Record<string, string[]> | undefined;
    if (errors) { const f = Object.values(errors)[0]?.[0]; if (f) return f; }
    return e.error?.detail ?? 'Não foi possível salvar o produto.';
  }
  private erro(m: string): void { this.snack.open(m, 'Fechar', { duration: 4000 }); }
}
