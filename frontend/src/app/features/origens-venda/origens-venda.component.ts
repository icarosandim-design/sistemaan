import { Component, OnInit, inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { OrigemVenda, SalvarOrigemVendaRequest } from './origens-venda.model';
import { OrigensVendaService } from './origens-venda.service';
import { OrigemVendaDialogComponent } from './origem-venda-dialog.component';

@Component({
  selector: 'app-origens-venda',
  standalone: true,
  imports: [MatTableModule, MatButtonModule, MatIconModule, MatTooltipModule, MatProgressBarModule],
  template: `
    <section class="pagina">
      <header class="page-head">
        <div class="page-title">
          <h1>Origens de Venda</h1>
          <p class="subtitulo">Gerencie as origens usadas no cadastro de clientes (para relatórios por origem).</p>
        </div>
        <button mat-flat-button class="btn-cta" (click)="nova()"><mat-icon>add</mat-icon> Nova origem</button>
      </header>

      <div class="tabela-card">
        @if (carregando) { <mat-progress-bar mode="indeterminate"></mat-progress-bar> }
        <table mat-table [dataSource]="origens" class="tabela">
          <ng-container matColumnDef="ordem">
            <th mat-header-cell *matHeaderCellDef class="t-center">Ordem</th>
            <td mat-cell *matCellDef="let o" class="t-center cel-sec">{{ o.ordem }}</td>
          </ng-container>
          <ng-container matColumnDef="nome">
            <th mat-header-cell *matHeaderCellDef>Nome</th>
            <td mat-cell *matCellDef="let o" class="cel-nome">{{ o.nome }}</td>
          </ng-container>
          <ng-container matColumnDef="ativo">
            <th mat-header-cell *matHeaderCellDef class="t-center">Status</th>
            <td mat-cell *matCellDef="let o" class="t-center">
              <span class="chip" [class.chip-inativo]="!o.ativo">{{ o.ativo ? 'Ativo' : 'Inativo' }}</span>
            </td>
          </ng-container>
          <ng-container matColumnDef="acoes">
            <th mat-header-cell *matHeaderCellDef></th>
            <td mat-cell *matCellDef="let o" class="t-acoes">
              <button mat-icon-button (click)="editar(o); $event.stopPropagation()" matTooltip="Editar"><mat-icon>edit</mat-icon></button>
              <button mat-icon-button (click)="alternarStatus(o, $event)" [matTooltip]="o.ativo ? 'Inativar' : 'Reativar'">
                <mat-icon>{{ o.ativo ? 'block' : 'check_circle' }}</mat-icon>
              </button>
            </td>
          </ng-container>
          <tr mat-header-row *matHeaderRowDef="displayedColumns; sticky: true"></tr>
          <tr mat-row *matRowDef="let row; columns: displayedColumns" class="linha" (click)="editar(row)"></tr>
          <tr class="sem-dados" *matNoDataRow>
            <td [attr.colspan]="displayedColumns.length">Nenhuma origem cadastrada.</td>
          </tr>
        </table>
      </div>
    </section>
  `,
  styles: [`
    .pagina { display: flex; flex-direction: column; gap: 1rem; max-width: 800px; margin: 0 auto; }
    .page-head { display: flex; align-items: flex-start; justify-content: space-between; gap: 1rem; flex-wrap: wrap; }
    .page-title h1 { margin: 0; font-size: 1.5rem; font-weight: 700; }
    .subtitulo { margin: 0.25rem 0 0; font-size: 0.85rem; color: var(--an-texto-secundario); }
    .btn-cta { background: var(--an-cta); color: #fff; &:hover { background: var(--an-cta-hover); } }
    .tabela-card { background: var(--an-superficie); border: 1px solid var(--an-fundo-secundario); border-radius: var(--an-raio); }
    .tabela { width: 100%; background: var(--an-superficie); }
    th.mat-mdc-header-cell { font-weight: 700; color: var(--an-texto-secundario); background: var(--an-fundo); }
    .cel-nome { font-weight: 600; color: var(--an-texto-titulo); }
    .cel-sec { color: var(--an-texto-secundario); }
    .t-center { text-align: center !important; }
    .t-acoes { text-align: right; white-space: nowrap; }
    .linha { cursor: pointer; }
    .chip { font-size: 0.7rem; font-weight: 600; padding: 0.12rem 0.55rem; border-radius: 999px; background: rgba(63,79,45,0.12); color: var(--an-primaria); }
    .chip-inativo { background: var(--an-fundo-secundario); color: var(--an-texto-secundario); }
    .sem-dados td { padding: 1.5rem; text-align: center; color: var(--an-texto-secundario); }
  `],
})
export class OrigensVendaComponent implements OnInit {
  private readonly service = inject(OrigensVendaService);
  private readonly dialog = inject(MatDialog);
  private readonly snack = inject(MatSnackBar);

  readonly displayedColumns = ['ordem', 'nome', 'ativo', 'acoes'];
  origens: OrigemVenda[] = [];
  carregando = false;

  ngOnInit(): void {
    this.carregar();
  }

  carregar(): void {
    this.carregando = true;
    this.service.listar(true).subscribe({
      next: (os) => { this.origens = os; this.carregando = false; },
      error: () => { this.carregando = false; this.erro('Falha ao carregar origens.'); },
    });
  }

  nova(): void {
    this.abrir(null);
  }

  editar(o: OrigemVenda): void {
    this.abrir(o);
  }

  alternarStatus(o: OrigemVenda, ev: Event): void {
    ev.stopPropagation();
    this.service.alternarStatus(o.id, !o.ativo).subscribe({
      next: () => { this.snack.open(o.ativo ? 'Origem inativada.' : 'Origem reativada.', 'OK', { duration: 2500 }); this.carregar(); },
      error: (e: HttpErrorResponse) => this.erro(this.mensagemErro(e)),
    });
  }

  private abrir(o: OrigemVenda | null): void {
    const ref = this.dialog.open(OrigemVendaDialogComponent, { data: { origem: o }, width: '440px', maxWidth: '95vw', autoFocus: false });
    ref.afterClosed().subscribe((req: SalvarOrigemVendaRequest | undefined) => {
      if (!req) {
        return;
      }
      const obs = o ? this.service.atualizar(o.id, req) : this.service.criar(req);
      obs.subscribe({
        next: () => { this.snack.open(o ? 'Origem atualizada.' : 'Origem criada.', 'OK', { duration: 2500 }); this.carregar(); },
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
    return e.error?.detail ?? 'Não foi possível salvar a origem.';
  }

  private erro(msg: string): void {
    this.snack.open(msg, 'Fechar', { duration: 4000 });
  }
}
