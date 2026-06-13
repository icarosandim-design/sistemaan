import { Component, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { fmtPeso, porDia } from './producao.mock';
import { ProducaoMockService } from './producao-mock.service';

/** ⚠️ PROTÓTIPO/MOCK: pré-visualização de impressão (Mapa de produção ou Fichas técnicas). */
@Component({
  selector: 'app-producao-impressao',
  standalone: true,
  imports: [MatButtonModule, MatIconModule],
  templateUrl: './impressao.component.html',
  styleUrl: './impressao.component.scss',
})
export class ProducaoImpressaoComponent {
  readonly mock = inject(ProducaoMockService);
  readonly fmtPeso = fmtPeso;
  readonly porDia = porDia;

  readonly tipo: 'mapa' | 'fichas' = inject(ActivatedRoute).snapshot.paramMap.get('tipo') === 'fichas' ? 'fichas' : 'mapa';

  falta(cru: number, estoque: number): number {
    return Math.max(0, cru - estoque);
  }

  imprimir(): void {
    window.print();
  }
}
