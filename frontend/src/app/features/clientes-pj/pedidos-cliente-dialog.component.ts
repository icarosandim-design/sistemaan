import { Component, Inject, OnInit, inject, signal } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialog, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { PedidoResumo } from './clientes-pj.model';
import { ClientesPjService } from './clientes-pj.service';
import { PedidoDialogComponent } from './pedido-dialog.component';

@Component({
  selector: 'app-pedidos-cliente-dialog',
  standalone: true,
  imports: [MatDialogModule, MatButtonModule, MatIconModule, MatTooltipModule, MatProgressSpinnerModule],
  template: `
    <h2 mat-dialog-title>Pedidos — {{ data.nomeFantasia }}</h2>
    <mat-dialog-content>
      <div class="topo">
        <button mat-flat-button class="btn-cta" (click)="novo()"><mat-icon>add_shopping_cart</mat-icon> Novo pedido</button>
      </div>
      @if (carregando()) {
        <div class="carregando"><mat-spinner diameter="28"></mat-spinner></div>
      } @else {
        <div class="tabela">
          <div class="thead"><span>Pedido</span><span>Entrega</span><span>Status pedido</span><span>Status entrega</span><span class="t-r">Itens</span><span class="t-r">Pacotes</span><span class="t-r">Ações</span></div>
          @for (p of pedidos(); track p.id) {
            <div class="trow">
              <span>{{ fmtData(p.dataPedido) }}</span>
              <span>{{ fmtData(p.dataEntrega) }}</span>
              <span><span class="chip" [attr.data-s]="p.status">{{ p.status }}</span></span>
              <span>{{ p.entregaStatus || '—' }}</span>
              <span class="t-r">{{ p.totalItens }}</span>
              <span class="t-r">{{ p.totalPacotes }}</span>
              <span class="acoes t-r">
                @if (p.status === 'Rascunho') {
                  <button mat-icon-button matTooltip="Editar" (click)="editar(p)"><mat-icon>edit</mat-icon></button>
                  <button mat-icon-button matTooltip="Confirmar (gera entrega)" (click)="confirmar(p)"><mat-icon>check_circle</mat-icon></button>
                }
                @if (p.status !== 'Cancelado') {
                  <button mat-icon-button matTooltip="Cancelar" (click)="cancelar(p)"><mat-icon>cancel</mat-icon></button>
                }
              </span>
            </div>
          } @empty {
            <p class="vazio">Nenhum pedido para este cliente.</p>
          }
        </div>
      }
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="ref.close(alterou)">Fechar</button>
    </mat-dialog-actions>
  `,
  styles: [`
    .topo { margin-bottom: 0.6rem; }
    .btn-cta { background: var(--an-cta); color: #fff; }
    .carregando { display: flex; justify-content: center; padding: 2rem 0; }
    .tabela { display: flex; flex-direction: column; }
    .thead, .trow { display: grid; grid-template-columns: 100px 100px 110px 130px 60px 70px 110px; gap: 0.4rem; align-items: center; padding: 0.4rem 0.2rem; }
    .thead { font-size: 0.68rem; font-weight: 700; text-transform: uppercase; color: var(--an-texto-secundario); border-bottom: 1px solid var(--an-fundo-secundario); }
    .trow { font-size: 0.82rem; border-bottom: 1px dashed var(--an-fundo-secundario); }
    .t-r { text-align: right; }
    .chip { font-size: 0.7rem; font-weight: 700; padding: 0.1rem 0.45rem; border-radius: 999px; background: var(--an-fundo-secundario); color: var(--an-texto-secundario); }
    .chip[data-s='Confirmado'] { background: rgba(63,79,45,0.16); color: var(--an-primaria); }
    .chip[data-s='Cancelado'] { background: #fbeceb; color: #b3261e; }
    .acoes .mat-icon { font-size: 1.1rem; }
    .vazio { color: var(--an-texto-secundario); padding: 1rem; text-align: center; }
    mat-dialog-content { min-width: 720px; max-width: 92vw; }
    @media (max-width: 760px) { mat-dialog-content { min-width: auto; } }
  `],
})
export class PedidosClienteDialogComponent implements OnInit {
  private readonly service = inject(ClientesPjService);
  private readonly dialog = inject(MatDialog);
  private readonly snack = inject(MatSnackBar);

  readonly pedidos = signal<PedidoResumo[]>([]);
  readonly carregando = signal(false);
  alterou = false;

  constructor(
    readonly ref: MatDialogRef<PedidosClienteDialogComponent, boolean>,
    @Inject(MAT_DIALOG_DATA) readonly data: { clienteId: number; nomeFantasia: string },
  ) {}

  ngOnInit(): void {
    this.carregar();
  }

  fmtData(iso: string): string {
    const [y, m, d] = iso.split('-');
    return `${d}/${m}/${y}`;
  }

  carregar(): void {
    this.carregando.set(true);
    this.service.listarPedidos(this.data.clienteId).subscribe({
      next: (ps) => { this.pedidos.set(ps); this.carregando.set(false); },
      error: () => this.carregando.set(false),
    });
  }

  novo(): void {
    const ref = this.dialog.open(PedidoDialogComponent, { data: { clienteId: this.data.clienteId }, autoFocus: false });
    ref.afterClosed().subscribe((p) => { if (p) { this.alterou = true; this.carregar(); } });
  }

  editar(p: PedidoResumo): void {
    this.service.obterPedido(p.id).subscribe((pedido) => {
      const ref = this.dialog.open(PedidoDialogComponent, { data: { clienteId: this.data.clienteId, pedido }, autoFocus: false });
      ref.afterClosed().subscribe((res) => { if (res) { this.alterou = true; this.carregar(); } });
    });
  }

  confirmar(p: PedidoResumo): void {
    this.service.confirmarPedido(p.id).subscribe({
      next: () => { this.alterou = true; this.snack.open('Pedido confirmado — entrega gerada.', 'OK', { duration: 3000 }); this.carregar(); },
      error: (e) => this.snack.open(this.erro(e), 'OK', { duration: 4000 }),
    });
  }

  cancelar(p: PedidoResumo): void {
    this.service.cancelarPedido(p.id).subscribe({
      next: () => { this.alterou = true; this.snack.open('Pedido cancelado.', 'OK', { duration: 2500 }); this.carregar(); },
      error: (e) => this.snack.open(this.erro(e), 'OK', { duration: 4000 }),
    });
  }

  private erro(e: unknown): string {
    const err = (e as { error?: { errors?: Record<string, string[]>; detail?: string } })?.error;
    const first = err?.errors ? Object.values(err.errors)[0]?.[0] : undefined;
    return first ?? err?.detail ?? 'Operação não permitida.';
  }
}
