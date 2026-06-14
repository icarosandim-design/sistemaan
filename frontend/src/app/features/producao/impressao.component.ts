import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { firstValueFrom } from 'rxjs';
import { ConsumoConsolidado, OrdemProducao, fmtPeso, porDia } from './producao.model';
import { ProducaoService } from './producao.service';

const ORDEM_CATS = ['Proteínas', 'Carboidratos', 'Legumes', 'Temperos', 'Óleos', 'Suplementos', 'Outros'];

/** Pré-visualização de impressão (Mapa de produção ou Fichas técnicas) de uma ordem. */
@Component({
  selector: 'app-producao-impressao',
  standalone: true,
  imports: [MatButtonModule, MatIconModule],
  templateUrl: './impressao.component.html',
  styleUrl: './impressao.component.scss',
})
export class ProducaoImpressaoComponent implements OnInit {
  private readonly api = inject(ProducaoService);
  private readonly route = inject(ActivatedRoute);
  readonly fmtPeso = fmtPeso;
  readonly porDia = porDia;

  readonly tipo: 'mapa' | 'fichas' = this.route.snapshot.paramMap.get('tipo') === 'fichas' ? 'fichas' : 'mapa';
  readonly ordem = signal<OrdemProducao | null>(null);

  async ngOnInit(): Promise<void> {
    const id = Number(this.route.snapshot.queryParamMap.get('ordem'));
    if (id) {
      try {
        this.ordem.set(await firstValueFrom(this.api.obter(id)));
      } catch {
        this.ordem.set(null);
      }
    }
  }

  fmtData(iso: string | null | undefined): string {
    if (!iso) return '—';
    const [y, m, d] = iso.split('-');
    return `${d}/${m}/${y}`;
  }

  grupos(): { categoria: string; itens: ConsumoConsolidado[] }[] {
    const map = new Map<string, ConsumoConsolidado[]>();
    for (const i of this.ordem()?.consolidado ?? []) {
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
