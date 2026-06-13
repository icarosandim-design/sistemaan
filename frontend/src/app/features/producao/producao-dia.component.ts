import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { fmtPeso, IngredienteConsolidadoMock, rotuloProntidao, rotuloStatusFicha } from './producao.mock';
import { ProducaoMockService } from './producao-mock.service';
import { ProducaoFinalizarDialogComponent } from './finalizar-dialog.component';

@Component({
  selector: 'app-producao-dia',
  standalone: true,
  imports: [RouterLink, MatButtonModule, MatIconModule, MatTooltipModule],
  templateUrl: './producao-dia.component.html',
  styleUrl: './producao-dia.component.scss',
})
export class ProducaoDiaComponent {
  readonly mock = inject(ProducaoMockService);
  private readonly dialog = inject(MatDialog);
  private readonly snack = inject(MatSnackBar);

  readonly fmtPeso = fmtPeso;
  readonly rotuloProntidao = rotuloProntidao;
  readonly rotuloStatusFicha = rotuloStatusFicha;

  falta(i: IngredienteConsolidadoMock): number {
    return this.mock.faltaConsolidado(i);
  }

  imprimirMapa(): void {
    window.open('/producao/impressao/mapa', '_blank');
  }

  imprimirFichas(): void {
    window.open('/producao/impressao/fichas', '_blank');
  }

  finalizar(): void {
    const ref = this.dialog.open(ProducaoFinalizarDialogComponent, { width: '600px', maxWidth: '96vw', autoFocus: false });
    ref.afterClosed().subscribe((ok) => {
      if (ok) {
        this.snack.open('Produção finalizada (protótipo — nada foi salvo).', 'OK', { duration: 3500 });
      }
    });
  }
}
