import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { Rota, rotuloPeriodo } from './rotas.model';
import { RotasService } from './rotas.service';
import { rotuloPreferenciaHorario } from '../clientes/clientes.model';

@Component({
  selector: 'app-rota-impressao',
  standalone: true,
  imports: [MatButtonModule, MatIconModule],
  template: `
    <div class="barra-no-print">
      <span><mat-icon>print</mat-icon> Rota para impressão / WhatsApp</span>
      <button mat-flat-button class="btn-cta" (click)="imprimir()" [disabled]="!rota()"><mat-icon>print</mat-icon> Imprimir / Salvar PDF</button>
    </div>
    @if (!rota()) {
      <div class="folha"><p>Rota não encontrada.</p></div>
    } @else {
      @let r = rota()!;
      <div class="folha">
        <h1>Rota — {{ fmtData(r.data) }}</h1>
        <p class="sub">Saída: <strong>{{ r.nome }}</strong> ({{ rotuloPeriodo(r.periodo) }}) · Entregador: <strong>{{ r.entregador || '—' }}</strong></p>
        @for (p of r.paradas; track p.entregaId; let i = $index) {
          <article class="parada">
            <div class="cab"><strong>{{ i + 1 }}. {{ p.clienteNome }}</strong>{{ p.ehPj ? ' [Cliente PJ]' : '' }} — {{ rotuloPreferencia(p.preferenciaHorario) }}</div>
            <div>{{ p.endereco }}{{ p.bairro ? ' — ' + p.bairro : '' }}{{ p.cidade ? ', ' + p.cidade : '' }}</div>
            @if (p.telefone) { <div>Tel/WhatsApp: {{ p.telefone }}</div> }
            @if (p.itensResumo) { <div>Itens: {{ p.itensResumo }}</div> }
          </article>
        }
      </div>
    }
  `,
  styles: [`
    .barra-no-print { display: flex; align-items: center; justify-content: space-between; padding: 0.8rem 1.2rem; background: var(--an-superficie); border-bottom: 1px solid var(--an-fundo-secundario);
      span { display: inline-flex; align-items: center; gap: 0.4rem; color: var(--an-texto-secundario); } }
    .btn-cta { background: var(--an-cta); color: #fff; }
    .folha { max-width: 720px; margin: 1.5rem auto; padding: 1.5rem; background: #fff; }
    h1 { font-size: 1.4rem; margin: 0 0 0.3rem; }
    .sub { color: #444; margin: 0 0 1rem; }
    .parada { padding: 0.5rem 0; border-bottom: 1px solid #eee; font-size: 0.95rem; line-height: 1.4; }
    .cab { font-size: 1.05rem; }
    @media print { .barra-no-print { display: none; } .folha { margin: 0; } }
  `],
})
export class RotaImpressaoComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly service = inject(RotasService);
  readonly rota = signal<Rota | null>(null);
  readonly rotuloPeriodo = rotuloPeriodo;
  readonly rotuloPreferencia = rotuloPreferenciaHorario;

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (id) {
      this.service.obter(id).subscribe({ next: (r) => this.rota.set(r), error: () => this.rota.set(null) });
    }
  }

  fmtData(iso: string): string {
    const [y, m, d] = iso.split('-');
    return `${d}/${m}/${y}`;
  }

  imprimir(): void {
    window.print();
  }
}
