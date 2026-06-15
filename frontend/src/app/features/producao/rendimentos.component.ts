import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { firstValueFrom } from 'rxjs';
import { Rendimento, RendimentoIngrediente, fmtPeso } from './producao.model';
import { ProducaoService } from './producao.service';

@Component({
  selector: 'app-producao-rendimentos',
  standalone: true,
  imports: [FormsModule, MatButtonModule, MatIconModule, MatTooltipModule, MatProgressSpinnerModule],
  templateUrl: './rendimentos.component.html',
  styleUrl: './rendimentos.component.scss',
})
export class ProducaoRendimentosComponent implements OnInit {
  private readonly api = inject(ProducaoService);
  readonly fmtPeso = fmtPeso;

  inicio = '';
  fim = '';

  readonly dados = signal<Rendimento | null>(null);
  readonly carregando = signal(false);

  readonly mostrarDetalhe = signal(false);

  readonly revisarCount = computed(() => this.dados()?.porIngrediente.filter((i) => i.revisarCoeficiente).length ?? 0);

  ngOnInit(): void {
    const hoje = new Date();
    const ini = new Date(hoje);
    ini.setDate(hoje.getDate() - 29);
    this.fim = this.iso(hoje);
    this.inicio = this.iso(ini);
    this.carregar();
  }

  private iso(d: Date): string {
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
  }

  fmtData(iso: string): string {
    if (!iso) return '—';
    const [y, m, d] = iso.split('-');
    return `${d}/${m}/${y}`;
  }

  async carregar(): Promise<void> {
    if (!this.inicio || !this.fim) return;
    this.carregando.set(true);
    try {
      this.dados.set(await firstValueFrom(this.api.obterRendimentos(this.inicio, this.fim)));
    } finally {
      this.carregando.set(false);
    }
  }

  /** Texto de tendência do cru real vs previsto. */
  tendencia(i: RendimentoIngrediente): 'acima' | 'abaixo' | 'igual' {
    if (i.diferencaCruGramas > 0) return 'acima';
    if (i.diferencaCruGramas < 0) return 'abaixo';
    return 'igual';
  }

  /** Fator de correção legível (cru ÷ cozido) a partir do coeficiente (cozido ÷ cru). */
  fator(coef: number | null): string {
    if (!coef || coef <= 0) return '—';
    return (1 / coef).toLocaleString('pt-BR', { maximumFractionDigits: 2 });
  }
}
