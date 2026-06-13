import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { fmtPeso, FichaMock, IngredienteConsolidadoMock, rotuloProntidao, rotuloStatusFicha } from './producao.mock';
import { ProducaoMockService } from './producao-mock.service';
import { ProducaoFinalizarDialogComponent } from './finalizar-dialog.component';
import { FichaMaxComponent, FichasMaxComponent, FULLSCREEN, IngredientesMaxComponent } from './producao-max-dialogs.component';

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

  private readonly ordemCategorias = ['Proteínas', 'Carboidratos', 'Legumes', 'Temperos', 'Óleos', 'Suplementos', 'Outros'];

  /** Consolidado agrupado por categoria, na ordem definida. */
  get gruposConsolidado(): { categoria: string; itens: IngredienteConsolidadoMock[] }[] {
    const map = new Map<string, IngredienteConsolidadoMock[]>();
    for (const i of this.mock.consolidado()) {
      const lista = map.get(i.categoria) ?? [];
      lista.push(i);
      map.set(i.categoria, lista);
    }
    const ordenadas = this.ordemCategorias.filter((c) => map.has(c));
    const extras = [...map.keys()].filter((c) => !this.ordemCategorias.includes(c));
    return [...ordenadas, ...extras].map((c) => ({ categoria: c, itens: map.get(c)! }));
  }

  falta(i: IngredienteConsolidadoMock): number {
    return this.mock.faltaConsolidado(i);
  }

  maximizarIngredientes(): void {
    this.dialog.open(IngredientesMaxComponent, FULLSCREEN);
  }

  maximizarFichas(): void {
    this.dialog.open(FichasMaxComponent, FULLSCREEN);
  }

  maximizarFicha(ficha: FichaMock): void {
    this.dialog.open(FichaMaxComponent, { ...FULLSCREEN, data: { ficha } });
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
