import { Component, OnInit, inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { DoencasService } from './doencas.service';
import { Doenca, SalvarDoencaRequest } from './doencas.model';
import { DoencaDialogComponent } from './doenca-dialog.component';

@Component({
  selector: 'app-doencas',
  standalone: true,
  imports: [MatTableModule, MatButtonModule, MatIconModule, MatTooltipModule, MatProgressBarModule],
  template: `
    <section class="pagina">
      <header class="page-head">
        <div class="page-title">
          <h1>Doenças</h1>
          <p class="subtitulo">Lista de doenças de cães. Vinculadas ao pet no cadastro (um pet pode ter mais de uma).</p>
        </div>
        <button mat-flat-button class="btn-cta" (click)="nova()"><mat-icon>add</mat-icon> Nova doença</button>
      </header>

      <div class="tabela-card">
        @if (carregando) { <mat-progress-bar mode="indeterminate"></mat-progress-bar> }
        <table mat-table [dataSource]="doencas" class="tabela">
          <ng-container matColumnDef="ordem">
            <th mat-header-cell *matHeaderCellDef class="t-center">Ordem</th>
            <td mat-cell *matCellDef="let d" class="t-center cel-sec">{{ d.ordem }}</td>
          </ng-container>
          <ng-container matColumnDef="nome">
            <th mat-header-cell *matHeaderCellDef>Nome</th>
            <td mat-cell *matCellDef="let d" class="cel-nome">{{ d.nome }}</td>
          </ng-container>
          <ng-container matColumnDef="ativo">
            <th mat-header-cell *matHeaderCellDef class="t-center">Status</th>
            <td mat-cell *matCellDef="let d" class="t-center">
              <span class="chip" [class.chip-inativo]="!d.ativo">{{ d.ativo ? 'Ativa' : 'Inativa' }}</span>
            </td>
          </ng-container>
          <ng-container matColumnDef="acoes">
            <th mat-header-cell *matHeaderCellDef></th>
            <td mat-cell *matCellDef="let d" class="t-acoes">
              <button mat-icon-button (click)="editar(d); $event.stopPropagation()" matTooltip="Editar"><mat-icon>edit</mat-icon></button>
              <button mat-icon-button (click)="alternarStatus(d, $event)" [matTooltip]="d.ativo ? 'Inativar' : 'Reativar'">
                <mat-icon>{{ d.ativo ? 'block' : 'check_circle' }}</mat-icon>
              </button>
            </td>
          </ng-container>
          <tr mat-header-row *matHeaderRowDef="displayedColumns; sticky: true"></tr>
          <tr mat-row *matRowDef="let row; columns: displayedColumns" class="linha" (click)="editar(row)"></tr>
          <tr class="sem-dados" *matNoDataRow>
            <td [attr.colspan]="displayedColumns.length">Nenhuma doença cadastrada.</td>
          </tr>
        </table>
      </div>
    </section>
  `,
  styles: [`
    .pagina { display: flex; flex-direction: column; gap: 1rem; max-width: 900px; margin: 0 auto; }
    .page-head { display: flex; align-items: flex-start; justify-content: space-between; gap: 1rem; flex-wrap: wrap; }
    .page-title h1 { margin: 0; font-size: 1.5rem; font-weight: 700; }
    .subtitulo { margin: 0.35rem 0 0; font-size: 0.85rem; color: var(--an-texto-secundario); }
    .btn-cta { background: var(--an-cta); color: #fff; }
    .tabela-card { background: var(--an-superficie); border: 1px solid var(--an-fundo-secundario); border-radius: var(--an-raio); overflow: hidden; }
    .tabela { width: 100%; }
    .t-center { text-align: center; }
    .t-acoes { text-align: right; white-space: nowrap; }
    .cel-nome { font-weight: 600; color: var(--an-texto-titulo); }
    .cel-sec { color: var(--an-texto-secundario); }
    .linha { cursor: pointer; }
    .chip { background: rgba(63,79,45,0.12); color: var(--an-primaria); border-radius: 12px; padding: 0.1rem 0.6rem; font-size: 0.78rem; font-weight: 600; }
    .chip-inativo { background: rgba(0,0,0,0.08); color: var(--an-texto-secundario); }
    .sem-dados td { padding: 1.5rem; text-align: center; color: var(--an-texto-secundario); }
  `],
})
export class DoencasComponent implements OnInit {
  private readonly service = inject(DoencasService);
  private readonly dialog = inject(MatDialog);
  private readonly snack = inject(MatSnackBar);

  readonly displayedColumns = ['ordem', 'nome', 'ativo', 'acoes'];
  doencas: Doenca[] = [];
  carregando = false;

  ngOnInit(): void {
    this.carregar();
  }

  carregar(): void {
    this.carregando = true;
    this.service.listar(true).subscribe({
      next: (ds) => { this.doencas = ds; this.carregando = false; },
      error: () => { this.carregando = false; this.erro('Falha ao carregar doenças.'); },
    });
  }

  nova(): void { this.abrir(null); }
  editar(d: Doenca): void { this.abrir(d); }

  alternarStatus(d: Doenca, ev: Event): void {
    ev.stopPropagation();
    this.service.alternarStatus(d.id, !d.ativo).subscribe({
      next: () => { this.snack.open(d.ativo ? 'Doença inativada.' : 'Doença reativada.', 'OK', { duration: 2500 }); this.carregar(); },
      error: (e: HttpErrorResponse) => this.erro(this.mensagemErro(e)),
    });
  }

  private abrir(d: Doenca | null): void {
    const ref = this.dialog.open(DoencaDialogComponent, { data: { doenca: d }, width: '420px', maxWidth: '95vw', autoFocus: false });
    ref.afterClosed().subscribe((req: SalvarDoencaRequest | undefined) => {
      if (!req) return;
      const obs = d ? this.service.atualizar(d.id, req) : this.service.criar(req);
      obs.subscribe({
        next: () => { this.snack.open(d ? 'Doença atualizada.' : 'Doença criada.', 'OK', { duration: 2500 }); this.carregar(); },
        error: (e: HttpErrorResponse) => this.erro(this.mensagemErro(e)),
      });
    });
  }

  private mensagemErro(e: HttpErrorResponse): string {
    const errors = e.error?.errors as Record<string, string[]> | undefined;
    if (errors) {
      const primeira = Object.values(errors)[0]?.[0];
      if (primeira) return primeira;
    }
    return e.error?.detail ?? 'Não foi possível salvar a doença.';
  }

  private erro(msg: string): void {
    this.snack.open(msg, 'Fechar', { duration: 4000 });
  }
}
