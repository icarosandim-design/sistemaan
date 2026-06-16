import { Component, OnInit, inject, signal } from '@angular/core';
import { NgTemplateOutlet } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RelatoriosService } from './relatorios.service';
import {
  RelatorioCancelamentos, RelatorioCustosInsumos, RelatorioEstoque, RelatorioFiltro,
  RelatorioPerdas, RelatorioProducao, RelatorioVendas,
} from './relatorios.model';
import { IngredientesService } from '../ingredientes/ingredientes.service';
import { Ingrediente } from '../ingredientes/ingredientes.model';
import { MotivosCancelamentoService } from '../motivos-cancelamento/motivos-cancelamento.service';
import { MotivoCancelamento } from '../motivos-cancelamento/motivos-cancelamento.model';
import { exportarCsv, num } from '../../shared/csv.util';

type RelTipo = 'vendas' | 'cancelamentos' | 'producao' | 'perdas' | 'custos' | 'estoque';

interface RelOpcao { id: RelTipo; nome: string; icone: string; desc: string; }

@Component({
  selector: 'app-relatorios',
  standalone: true,
  imports: [NgTemplateOutlet, FormsModule, MatIconModule, MatButtonModule, MatProgressSpinnerModule, MatTooltipModule],
  templateUrl: './relatorios.component.html',
  styleUrl: './relatorios.component.scss',
})
export class RelatoriosComponent implements OnInit {
  private readonly service = inject(RelatoriosService);
  private readonly ingService = inject(IngredientesService);
  private readonly motivosService = inject(MotivosCancelamentoService);

  readonly opcoes: RelOpcao[] = [
    { id: 'vendas', nome: 'Vendas', icone: 'sell', desc: 'Assinatura PF, Avulsa PF e Pedido PJ' },
    { id: 'cancelamentos', nome: 'Cancelamentos', icone: 'cancel', desc: 'Churn por motivo e receita perdida' },
    { id: 'producao', nome: 'Produção planejado × real', icone: 'factory', desc: 'Eficiência da produção' },
    { id: 'perdas', nome: 'Perdas de ingredientes', icone: 'delete_sweep', desc: 'Perda e sobra em kg e R$' },
    { id: 'custos', nome: 'Evolução de custo dos insumos', icone: 'trending_up', desc: 'Preço de compra e custo médio' },
    { id: 'estoque', nome: 'Estoque e produto acabado', icone: 'inventory_2', desc: 'Saldo, mínimo e valor estimado' },
  ];

  readonly ativo = signal<RelTipo | null>(null);
  readonly carregando = signal(false);
  readonly erro = signal(false);

  // Filtros
  inicio = '';
  fim = '';
  tipoVenda = '';
  ingredienteId: number | null = null;
  motivoId: number | null = null;
  tipoEstoque = '';
  statusEstoque = '';
  pagina = 1;

  // Listas auxiliares
  readonly ingredientes = signal<Ingrediente[]>([]);
  readonly motivos = signal<MotivoCancelamento[]>([]);
  readonly tiposVenda = ['Assinatura PF', 'Venda Avulsa PF', 'Pedido PJ'];

  // Resultados
  readonly vendas = signal<RelatorioVendas | null>(null);
  readonly cancel = signal<RelatorioCancelamentos | null>(null);
  readonly producao = signal<RelatorioProducao | null>(null);
  readonly perdas = signal<RelatorioPerdas | null>(null);
  readonly custos = signal<RelatorioCustosInsumos | null>(null);
  readonly estoque = signal<RelatorioEstoque | null>(null);

  readonly num = num;

  ngOnInit(): void {
    const hoje = new Date();
    const ini = new Date(hoje);
    ini.setDate(hoje.getDate() - 29);
    this.fim = iso(hoje);
    this.inicio = iso(ini);
    this.ingService.listar().subscribe({ next: (is) => this.ingredientes.set(is) });
    this.motivosService.listar(true).subscribe({ next: (ms) => this.motivos.set(ms) });
  }

  abrir(id: RelTipo): void {
    this.ativo.set(id);
    this.pagina = 1;
    this.carregar();
  }

  fechar(): void {
    this.ativo.set(null);
  }

  private filtro(): RelatorioFiltro {
    return {
      inicio: this.inicio,
      fim: this.fim,
      pagina: this.pagina,
      tamanhoPagina: 50,
      tipo: this.ativo() === 'vendas' ? (this.tipoVenda || undefined) : this.ativo() === 'estoque' ? (this.tipoEstoque || undefined) : undefined,
      ingredienteId: this.ingredienteId ?? undefined,
      motivoId: this.motivoId ?? undefined,
      status: this.ativo() === 'estoque' ? (this.statusEstoque || undefined) : undefined,
    };
  }

  carregar(): void {
    const tipo = this.ativo();
    if (!tipo) return;
    this.carregando.set(true);
    this.erro.set(false);
    const f = this.filtro();
    const done = () => this.carregando.set(false);
    const fail = () => { this.erro.set(true); this.carregando.set(false); };
    switch (tipo) {
      case 'vendas': this.service.vendas(f).subscribe({ next: (r) => { this.vendas.set(r); done(); }, error: fail }); break;
      case 'cancelamentos': this.service.cancelamentos(f).subscribe({ next: (r) => { this.cancel.set(r); done(); }, error: fail }); break;
      case 'producao': this.service.producao(f).subscribe({ next: (r) => { this.producao.set(r); done(); }, error: fail }); break;
      case 'perdas': this.service.perdas(f).subscribe({ next: (r) => { this.perdas.set(r); done(); }, error: fail }); break;
      case 'custos': this.service.custosInsumos(f).subscribe({ next: (r) => { this.custos.set(r); done(); }, error: fail }); break;
      case 'estoque': this.service.estoque(f).subscribe({ next: (r) => { this.estoque.set(r); done(); }, error: fail }); break;
    }
  }

  aplicar(): void {
    this.pagina = 1;
    this.carregar();
  }

  paginar(delta: number, total: number): void {
    const max = Math.max(1, Math.ceil(total / 50));
    const nova = Math.min(max, Math.max(1, this.pagina + delta));
    if (nova !== this.pagina) {
      this.pagina = nova;
      this.carregar();
    }
  }

  totalPaginas(total: number): number {
    return Math.max(1, Math.ceil(total / 50));
  }

  fmtData(iso: string | null): string {
    if (!iso) return '—';
    const [y, m, d] = iso.split('-');
    return `${d}/${m}/${y}`;
  }

  // -------- Exportações CSV --------
  exportarVendas(): void {
    const r = this.vendas();
    if (!r) return;
    exportarCsv('relatorio-vendas',
      ['Data', 'Tipo', 'Cliente', 'Pet', 'Valor da venda', 'Custo receitas', 'Origem'],
      r.linhas.map((l) => [this.fmtData(l.data), l.tipo, l.cliente, l.pet ?? '', l.valor != null ? num(l.valor) : '', num(l.custo), l.origem ?? '']));
  }

  exportarCancelamentos(): void {
    const r = this.cancel();
    if (!r) return;
    exportarCsv('relatorio-cancelamentos',
      ['Data', 'Cliente', 'Pets', 'Motivo', 'Observação', 'Valor mensal perdido', 'Kg mensal perdido', 'Dias como cliente', 'Usuário'],
      r.linhas.map((l) => [this.fmtData(l.data), l.cliente, l.pets ?? '', l.motivo, l.observacao ?? '', num(l.valorMensalPerdido), num(l.kgMensalPerdido), l.diasComoCliente ?? '', l.usuario ?? '']));
  }

  exportarProducao(): void {
    const r = this.producao();
    if (!r) return;
    exportarCsv('relatorio-producao',
      ['Data', 'Ingrediente', 'Plan. cru (g)', 'Real cru (g)', 'Plan. cozido (g)', 'Real cozido (g)', 'Dif. cru (g)', 'Custo plan.', 'Custo real est.', 'Status'],
      r.linhas.map((l) => [this.fmtData(l.data), l.ingrediente, num(l.planejadoCruGramas), num(l.realCruGramas), num(l.planejadoCozidoGramas), num(l.realCozidoGramas), num(l.diferencaCruGramas), num(l.custoPlanejado), num(l.custoRealEstimado), l.status]));
  }

  exportarPerdas(): void {
    const r = this.perdas();
    if (!r) return;
    exportarCsv('relatorio-perdas',
      ['Data', 'Ingrediente', 'Perda kg', 'Perda R$', 'Sobra kg', 'Sobra R$', 'Custo médio', 'Fator cad.', 'Fator real'],
      r.linhas.map((l) => [this.fmtData(l.data), l.ingrediente, num(l.perdaKg), num(l.perdaValor), num(l.sobraKg), num(l.sobraValor), num(l.custoMedioUsado), num(l.fatorCadastrado, 3), num(l.fatorReal, 3)]));
  }

  exportarCustos(): void {
    const r = this.custos();
    if (!r) return;
    exportarCsv('relatorio-custos-insumos',
      ['Data', 'Ingrediente', 'Fornecedor', 'Quantidade', 'Unidade', 'Valor unitário', 'Valor total', 'Origem'],
      r.linhas.map((l) => [this.fmtData(l.data), l.ingrediente, l.fornecedor ?? '', num(l.quantidade), l.unidade, num(l.valorUnitario), num(l.valorTotal), l.origem]));
  }

  exportarEstoque(): void {
    const r = this.estoque();
    if (!r) return;
    exportarCsv('relatorio-estoque',
      ['Item', 'Tipo', 'Saldo', 'Unidade', 'Mínimo', 'Custo médio', 'Valor estimado', 'Status'],
      r.linhas.map((l) => [l.item, l.tipo, num(l.saldoFisico), l.unidade, num(l.minimo), num(l.custoMedio), num(l.valorEstimado), l.status]));
  }
}

function iso(d: Date): string {
  return d.toISOString().slice(0, 10);
}
