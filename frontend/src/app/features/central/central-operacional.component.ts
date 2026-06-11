import { Component, OnInit, inject, signal } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { CentralResumo } from './central.model';
import { CentralService } from './central.service';

@Component({
  selector: 'app-central-operacional',
  standalone: true,
  imports: [MatCardModule, MatIconModule, MatButtonModule, MatTooltipModule, MatProgressSpinnerModule],
  templateUrl: './central-operacional.component.html',
  styleUrl: './central-operacional.component.scss',
})
export class CentralOperacionalComponent implements OnInit {
  private readonly service = inject(CentralService);

  readonly resumo = signal<CentralResumo | null>(null);
  readonly carregando = signal(true);
  readonly erro = signal(false);

  /** Card de produção começa minimizado. */
  readonly producaoExpandida = signal(false);

  /** Ações rápidas (configuração de UI; ligadas às telas quando existirem). */
  readonly acoes = [
    { label: 'Novo cliente', icone: 'person_add' },
    { label: 'Planejar produção', icone: 'factory' },
    { label: 'Planejar rotas', icone: 'route' },
  ];

  ngOnInit(): void {
    this.carregar();
  }

  carregar(): void {
    this.carregando.set(true);
    this.erro.set(false);
    this.service.obterResumo().subscribe({
      next: (r) => {
        this.resumo.set(r);
        this.carregando.set(false);
      },
      error: () => {
        this.erro.set(true);
        this.carregando.set(false);
      },
    });
  }

  // ===== Acessores resilientes ao resumo ausente =====
  get diaSelecionado(): string {
    return this.resumo()?.diaSelecionado ?? '';
  }

  get kpis() {
    return this.resumo()?.kpis ?? [];
  }

  get entregas7() {
    return this.resumo()?.entregas7 ?? [];
  }

  get producaoCasa() {
    return this.resumo()?.producaoCasa ?? [];
  }

  get producaoPersonalizada() {
    return this.resumo()?.producaoPersonalizada ?? [];
  }

  get ingredientes() {
    return this.resumo()?.ingredientes ?? [];
  }

  get estoque() {
    return this.resumo()?.estoque ?? [];
  }

  get alertas() {
    return this.resumo()?.alertas ?? [];
  }

  get temProducao(): boolean {
    return this.producaoCasa.length > 0 || this.producaoPersonalizada.length > 0;
  }

  // ===== Derivados de produção =====
  get subtotalCasa(): number {
    return this.producaoCasa.reduce((s, c) => s + c.pacotes, 0);
  }

  get subtotalPersonalizada(): number {
    return this.producaoPersonalizada.reduce((s, p) => s + p.pacotes, 0);
  }

  get totalPacotes(): number {
    return this.subtotalCasa + this.subtotalPersonalizada;
  }

  get totalReceitas(): number {
    return this.producaoCasa.length + this.producaoPersonalizada.length;
  }

  private readonly maxCasaVisivel = 8;
  private readonly maxPersVisivel = 12;

  get casaVisivel() {
    return this.producaoCasa.slice(0, this.maxCasaVisivel);
  }

  get casaExtra(): number {
    return Math.max(0, this.producaoCasa.length - this.maxCasaVisivel);
  }

  get persVisivel() {
    return this.producaoPersonalizada.slice(0, this.maxPersVisivel);
  }

  get persExtra(): number {
    return Math.max(0, this.producaoPersonalizada.length - this.maxPersVisivel);
  }

  get casaDensidade(): string {
    const n = this.producaoCasa.length;
    return n <= 3 ? 'd1' : n <= 6 ? 'd2' : 'd3';
  }

  get persDensidade(): string {
    const n = this.producaoPersonalizada.length;
    return n <= 6 ? 'd1' : n <= 12 ? 'd2' : 'd3';
  }

  get faltantes(): { nome: string; falta: number }[] {
    return this.ingredientes
      .filter((i) => i.estoque < i.crus)
      .map((i) => ({ nome: i.nome, falta: i.crus - i.estoque }));
  }

  get bannerTexto(): string {
    const partes = this.faltantes.map((f) => `${this.fmtNum(f.falta)} kg de ${f.nome.toLowerCase()}`);
    return `Comprar ${this.juntar(partes)}.`;
  }

  fmtKg(v: number): string {
    return `${this.fmtNum(v)}kg`;
  }

  private fmtNum(v: number): string {
    return v.toLocaleString('pt-BR', { maximumFractionDigits: 1 });
  }

  private juntar(itens: string[]): string {
    if (itens.length <= 1) {
      return itens.join('');
    }
    return `${itens.slice(0, -1).join(', ')} e ${itens[itens.length - 1]}`;
  }
}
