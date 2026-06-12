import { DatePipe } from '@angular/common';
import { Component, Inject, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { MAT_DIALOG_DATA, MatDialog, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar } from '@angular/material/snack-bar';
import {
  classeStatus,
  EntregaDetalhe,
  fmtPeso,
  labelStatus,
  MOTIVOS_NAO_ENTREGA,
  operacionalDeDetalhe,
  ProntidaoEntrega,
  prontidaoEntrega,
} from './entregas.model';
import { EntregasService } from './entregas.service';
import { MotivoDialogComponent } from './motivo-dialog.component';
import { ReagendarDialogComponent, ReagendarDialogResult } from './reagendar-dialog.component';

export interface EntregaDetalheDialogData {
  id: number;
}

@Component({
  selector: 'app-entrega-detalhe-dialog',
  standalone: true,
  imports: [
    DatePipe,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatTooltipModule,
    MatProgressBarModule,
  ],
  templateUrl: './entrega-detalhe-dialog.component.html',
  styleUrl: './entrega-detalhe-dialog.component.scss',
})
export class EntregaDetalheDialogComponent {
  private readonly service = inject(EntregasService);
  private readonly dialog = inject(MatDialog);
  private readonly snack = inject(MatSnackBar);

  readonly labelStatus = labelStatus;
  readonly classeStatus = classeStatus;
  readonly fmtPeso = fmtPeso;

  readonly detalhe = signal<EntregaDetalhe | null>(null);
  readonly carregando = signal(true);
  readonly salvando = signal(false);
  readonly mostrarHistorico = signal(false);
  private alterado = false;

  alternarHistorico(): void {
    this.mostrarHistorico.update((v) => !v);
  }

  constructor(
    private readonly ref: MatDialogRef<EntregaDetalheDialogComponent, boolean>,
    @Inject(MAT_DIALOG_DATA) readonly data: EntregaDetalheDialogData,
  ) {
    this.recarregar();
  }

  get podeFluxo(): boolean {
    const s = this.detalhe()?.status;
    return s !== 'Entregue' && s !== 'Cancelada' && s !== 'Reagendada';
  }

  /** Prontidão consolidada (estoque da Casa + prontidão das Personalizadas). */
  prontidao(): ProntidaoEntrega {
    const d = this.detalhe();
    return prontidaoEntrega(d ? operacionalDeDetalhe(d) : undefined);
  }

  /** Bloqueia o avanço de status enquanto houver pendência (não pronta / sem estoque). */
  get bloqueadoPorPendencia(): boolean {
    const p = this.prontidao();
    return p.temConteudo && !p.tudoPronto;
  }

  proximoStatus(): { status: string; label: string } | null {
    switch (this.detalhe()?.status) {
      case 'Programada':
        return { status: 'ConfirmadaCliente', label: 'Confirmar com cliente' };
      case 'ConfirmadaCliente':
        return { status: 'SaiuParaEntrega', label: 'Saiu para entrega' };
      case 'SaiuParaEntrega':
        return { status: 'Entregue', label: 'Marcar entregue' };
      default:
        return null;
    }
  }

  avancar(status: string): void {
    if (this.bloqueadoPorPendencia) {
      this.snack.open('Resolva as pendências (produção/estoque) antes de avançar o status.', 'OK', { duration: 4000 });
      return;
    }
    this.executar(this.service.mudarStatus(this.data.id, status));
  }

  naoEntregue(): void {
    const ref = this.dialog.open(MotivoDialogComponent, {
      data: { titulo: 'Marcar como Não entregue', confirmar: 'Confirmar', motivos: MOTIVOS_NAO_ENTREGA },
      width: '440px',
      autoFocus: false,
    });
    ref.afterClosed().subscribe((motivo: string | undefined) => {
      if (motivo) {
        this.executar(this.service.naoEntregue(this.data.id, motivo));
      }
    });
  }

  cancelar(): void {
    const ref = this.dialog.open(MotivoDialogComponent, {
      data: { titulo: 'Cancelar entrega', confirmar: 'Cancelar entrega' },
      width: '440px',
      autoFocus: false,
    });
    ref.afterClosed().subscribe((motivo: string | undefined) => {
      if (motivo) {
        this.executar(this.service.cancelar(this.data.id, motivo));
      }
    });
  }

  alterarData(): void {
    const ref = this.dialog.open(ReagendarDialogComponent, {
      data: { dataAtual: this.detalhe()?.dataPrevista },
      width: '520px',
      autoFocus: false,
    });
    ref.afterClosed().subscribe((res: ReagendarDialogResult | undefined) => {
      if (!res) {
        return;
      }
      this.salvando.set(true);
      if (res.escopo === 'pontual') {
        this.service.reagendar(this.data.id, res.novaData, res.motivo).subscribe({
          next: () => this.recarregar(true),
          error: (e: HttpErrorResponse) => this.erroAcao(e),
        });
      } else {
        this.service.alterarAgenda(this.data.id, { novaData: res.novaData, frequenciaEntregaId: res.frequenciaEntregaId, motivo: res.motivo }).subscribe({
          next: () => {
            this.salvando.set(false);
            this.snack.open('Agenda do cliente atualizada e próximas entregas regeradas.', 'OK', { duration: 3000 });
            this.alterado = true;
            this.ref.close(true); // esta entrega pode ter sido regerada
          },
          error: (e: HttpErrorResponse) => this.erroAcao(e),
        });
      }
    });
  }

  fechar(): void {
    this.ref.close(this.alterado);
  }

  private executar(obs: ReturnType<EntregasService['mudarStatus']>): void {
    this.salvando.set(true);
    obs.subscribe({
      next: (d) => {
        this.detalhe.set(d);
        this.salvando.set(false);
        this.alterado = true;
        this.snack.open('Entrega atualizada.', 'OK', { duration: 2000 });
      },
      error: (e: HttpErrorResponse) => this.erroAcao(e),
    });
  }

  private recarregar(marcarAlterado = false): void {
    if (marcarAlterado) {
      this.alterado = true;
    }
    this.carregando.set(true);
    this.service.obter(this.data.id).subscribe({
      next: (d) => {
        this.detalhe.set(d);
        this.carregando.set(false);
        this.salvando.set(false);
      },
      error: () => {
        this.carregando.set(false);
        this.salvando.set(false);
        this.erro('Falha ao carregar a entrega.');
      },
    });
  }

  private erroAcao(e: HttpErrorResponse): void {
    this.salvando.set(false);
    this.erro(this.mensagemErro(e));
  }

  private mensagemErro(e: HttpErrorResponse): string {
    const errors = e.error?.errors as Record<string, string[]> | undefined;
    if (errors) {
      const primeira = Object.values(errors)[0]?.[0];
      if (primeira) {
        return primeira;
      }
    }
    return e.error?.detail ?? 'Não foi possível concluir a ação.';
  }

  private erro(msg: string): void {
    this.snack.open(msg, 'Fechar', { duration: 4000 });
  }
}
