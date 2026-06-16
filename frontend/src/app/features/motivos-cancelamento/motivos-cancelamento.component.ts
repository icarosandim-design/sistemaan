import { Component, OnInit, inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MotivosCancelamentoService } from './motivos-cancelamento.service';
import { MotivoCancelamento, SalvarMotivoCancelamentoRequest } from './motivos-cancelamento.model';
import { MotivoCancelamentoDialogComponent } from './motivo-cancelamento-dialog.component';

@Component({
  selector: 'app-motivos-cancelamento',
  standalone: true,
  imports: [MatTableModule, MatButtonModule, MatIconModule, MatTooltipModule, MatProgressBarModule],
  template: `
    <section class="pagina">
      <header class="page-head">
        <div class="page-title">
          <h1>Motivos de Cancelamento</h1>
          <p class="subtitulo">Lista usada ao cancelar clientes/assinaturas e no Relatório de Cancelamentos.</p>
        </div>
        <button mat-flat-button class="btn-cta" (click)="nova()"><mat-icon>add</mat-icon> Novo motivo</button>
      </header>

      <div class="tabela-card">
        @if (carregando) { <mat-progress-bar mode="indeterminate"></mat-progress-bar> }
        <table mat-table [dataSource]="motivos" class="tabela">
          <ng-container matColumnDef="ordem">
            <th mat-header-cell *matHeaderCellDef class="t-center">Ordem</th>
            <td mat-cell *matCellDef="let m" class="t-center cel-sec">{{ m.ordem }}</td>
          </ng-container>
          <ng-container matColumnDef="nome">
            <th mat-header-cell *matHeaderCellDef>Nome</th>
            <td mat-cell *matCellDef="let m" class="cel-nome">{{ m.nome }}</td>
          </ng-container>
          <ng-container matColumnDef="observacoes">
            <th mat-header-cell *matHeaderCellDef>Observações</th>
            <td mat-cell *matCellDef="let m" class="cel-sec">{{ m.observacoes || '—' }}</td>
          </ng-container>
          <ng-container matColumnDef="ativo">
            <th mat-header-cell *matHeaderCellDef class="t-center">Status</th>
            <td mat-cell *matCellDef="let m" class="t-center">
              <span class="chip" [class.chip-inativo]="!m.ativo">{{ m.ativo ? 'Ativo' : 'Inativo' }}</span>
            </td>
          </ng-container>
          <ng-container matColumnDef="acoes">
            <th mat-header-cell *matHeaderCellDef></th>
            <td mat-cell *matCellDef="let m" class="t-acoes">
              <button mat-icon-button (click)="editar(m); $event.stopPropagation()" matTooltip="Editar"><mat-icon>edit</mat-icon></button>
              <button mat-icon-button (click)="alternarStatus(m, $event)" [matTooltip]="m.ativo ? 'Inativar' : 'Reativar'">
                <mat-icon>{{ m.ativo ? 'block' : 'check_circle' }}</mat-icon>
              </button>
            </td>
          </ng-container>
          <tr mat-header-row *matHeaderRowDef="displayedColumns; sticky: true"></tr>
          <tr mat-row *matRowDef="let row; columns: displayedColumns" class="linha" (click)="editar(row)"></tr>
          <tr class="sem-dados" *matNoDataRow>
            <td [attr.colspan]="displayedColumns.length">Nenhum motivo cadastrado.</td>
          </tr>
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
export class MotivosCancelamentoComponent implements OnInit {
  private readonly service = inject(MotivosCancelamentoService);
  private readonly dialog = inject(MatDialog);
  private readonly snack = inject(MatSnackBar);

  readonly displayedColumns = ['ordem', 'nome', 'observacoes', 'ativo', 'acoes'];
  motivos: MotivoCancelamento[] = [];
  carregando = false;

  ngOnInit(): void {
    this.carregar();
  }

  carregar(): void {
    this.carregando = true;
    this.service.listar(true).subscribe({
      next: (ms) => { this.motivos = ms; this.carregando = false; },
      error: () => { this.carregando = false; this.erro('Falha ao carregar motivos.'); },
    });
  }

  nova(): void { this.abrir(null); }
  editar(m: MotivoCancelamento): void { this.abrir(m); }

  alternarStatus(m: MotivoCancelamento, ev: Event): void {
    ev.stopPropagation();
    this.service.alternarStatus(m.id, !m.ativo).subscribe({
      next: () => { this.snack.open(m.ativo ? 'Motivo inativado.' : 'Motivo reativado.', 'OK', { duration: 2500 }); this.carregar(); },
      error: (e: HttpErrorResponse) => this.erro(this.mensagemErro(e)),
    });
  }

  private abrir(m: MotivoCancelamento | null): void {
    const ref = this.dialog.open(MotivoCancelamentoDialogComponent, { data: { motivo: m }, width: '460px', maxWidth: '95vw', autoFocus: false });
    ref.afterClosed().subscribe((req: SalvarMotivoCancelamentoRequest | undefined) => {
      if (!req) return;
      const obs = m ? this.service.atualizar(m.id, req) : this.service.criar(req);
      obs.subscribe({
        next: () => { this.snack.open(m ? 'Motivo atualizado.' : 'Motivo criado.', 'OK', { duration: 2500 }); this.carregar(); },
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
    return e.error?.detail ?? 'Não foi possível salvar o motivo.';
  }

  private erro(msg: string): void {
    this.snack.open(msg, 'Fechar', { duration: 4000 });
  }
}
