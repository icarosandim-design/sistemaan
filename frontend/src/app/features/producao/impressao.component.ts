import { Component, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { fmtPeso, IngredienteConsolidadoMock, porDia } from './producao.mock';
import { ProducaoMockService } from './producao-mock.service';

const ORDEM_CATS = ['Proteínas', 'Carboidratos', 'Legumes', 'Temperos', 'Óleos', 'Suplementos', 'Outros'];

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

  grupos(): { categoria: string; itens: IngredienteConsolidadoMock[] }[] {
    const map = new Map<string, IngredienteConsolidadoMock[]>();
    for (const i of this.mock.consolidado()) {
      const l = map.get(i.categoria) ?? [];
      l.push(i);
      map.set(i.categoria, l);
    }
    const ord = ORDEM_CATS.filter((c) => map.has(c));
    const extras = [...map.keys()].filter((c) => !ORDEM_CATS.includes(c));
    return [...ord, ...extras].map((c) => ({ categoria: c, itens: map.get(c)! }));
  }

  imprimir(): void {
    window.print();
  }
}
