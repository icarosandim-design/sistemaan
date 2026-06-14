import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Observable, forkJoin, map, of } from 'rxjs';
import {
  classeStatus,
  enderecoResumo,
  EntregaResumo,
  fmtPeso,
  gerarEntregasMock,
  labelStatus,
  operacionalDeDetalhe,
  ProntidaoEntrega,
  prontidaoEntrega,
  ResumoCasaDia,
  ResumoPersonalizadasDia,
  resumoCasaDoDia,
  resumoPersonalizadasDoDia,
  STATUS_ENTREGA,
} from './entregas.model';
import { EntregasService } from './entregas.service';
import { EstoqueService } from '../estoque/estoque.service';
import { EntregaDetalheDialogComponent } from './entrega-detalhe-dialog.component';

interface DiaCalendario {
  dia: number;
  dataIso: string;
  total: number;
  confirmadas: number;
  programadas: number;
  pendencias: number;
}

@Component({
  selector: 'app-entregas',
  standalone: true,
  imports: [
    FormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatTooltipModule,
    MatProgressBarModule,
  ],
  templateUrl: './entregas.component.html',
  styleUrl: './entregas.component.scss',
})
export class EntregasComponent implements OnInit {
  private readonly service = inject(EntregasService);
  private readonly estoque = inject(EstoqueService);
  private readonly dialog = inject(MatDialog);
  private readonly snack = inject(MatSnackBar);

  // Saldo real de produto acabado (Casa): `${receitaId}-${pesoGramas}` → pacotes.
  readonly estoqueProdutoAcabado = signal<Map<string, number>>(new Map());
  private estoqueCarregado = false;

  readonly statusOpcoes = STATUS_ENTREGA;
  readonly labelStatus = labelStatus;
  readonly classeStatus = classeStatus;
  readonly enderecoResumo = enderecoResumo;
  readonly fmtPeso = fmtPeso;
  readonly prontidao = (e: EntregaResumo): ProntidaoEntrega => prontidaoEntrega(e.operacional);
  readonly diasSemana = ['Dom', 'Seg', 'Ter', 'Qua', 'Qui', 'Sex', 'Sáb'];

  // Cartões expandidos (por id). Por padrão todos minimizados.
  readonly expandidas = signal<Set<number>>(new Set());

  readonly todas = signal<EntregaResumo[]>([]);
  readonly carregando = signal(false);
  readonly gerando = signal(false);
  // Carregamento sob demanda do resumo operacional (detalhe) das entregas do dia.
  readonly carregandoOperacional = signal(false);
  // ⚠️ TEMPORÁRIO: indica que a tela está exibindo dados MOCK (não reais).
  readonly usandoMock = signal(false);

  // Quantidade mínima de entregas reais para dispensar o mock de validação visual.
  private static readonly MIN_REAIS = 3;

  mesRef = new Date(new Date().getFullYear(), new Date().getMonth(), 1);
  diaSelecionado = '';

  // Filtros compactos (data é controlada pelo calendário)
  filtroStatus = '';
  filtroCliente = '';
  filtroPet = '';
  filtroBairro = '';
  filtroCidade = '';
  atalho = '';

  ngOnInit(): void {
    this.carregar(true);
  }

  carregar(inicial = false): void {
    this.carregando.set(true);
    this.service.listar().subscribe({
      next: (lista) => {
        this.aplicarDados(lista);
        this.carregando.set(false);
        if (inicial || !this.diaSelecionado) {
          this.selecionarDiaInicial();
        }
        this.carregarOperacionalDoDia();
      },
      error: () => {
        this.carregando.set(false);
        // Sem backend disponível: cai no mock para validação visual.
        this.aplicarDados([]);
        this.selecionarDiaInicial();
      },
    });
  }

  // Usa dados reais quando existirem; o mock só entra como fallback de validação
  // visual quando ainda não há entregas reais suficientes.
  private aplicarDados(reais: EntregaResumo[]): void {
    if (reais.length >= EntregasComponent.MIN_REAIS) {
      this.usandoMock.set(false);
      this.todas.set(reais);
    } else {
      this.usandoMock.set(true);
      this.todas.set(gerarEntregasMock());
    }
  }

  gerar(): void {
    this.gerando.set(true);
    this.service.gerar(45).subscribe({
      next: (r) => {
        this.gerando.set(false);
        this.snack.open(`${r.geradas} entrega(s) gerada(s) para ${r.clientes} cliente(s).`, 'OK', { duration: 3000 });
        this.carregar();
      },
      error: () => {
        this.gerando.set(false);
        this.erro('Falha ao gerar entregas.');
      },
    });
  }

  private static iso(d: Date): string {
    const p = (n: number) => String(n).padStart(2, '0');
    return `${d.getFullYear()}-${p(d.getMonth() + 1)}-${p(d.getDate())}`;
  }

  private selecionarDiaInicial(): void {
    const hoje = EntregasComponent.iso(new Date());
    const datas = [...new Set(this.todas().map((e) => e.dataPrevista))].sort();
    let alvo = '';
    if (datas.includes(hoje)) {
      alvo = hoje;
    } else {
      alvo = datas.find((d) => d >= hoje) ?? datas[datas.length - 1] ?? '';
    }
    this.diaSelecionado = alvo;
    if (alvo) {
      const [y, m] = alvo.split('-').map(Number);
      this.mesRef = new Date(y, m - 1, 1);
    }
  }

  // ===== Lista do dia =====
  get tituloLista(): string {
    if (!this.diaSelecionado) {
      return 'Selecione um dia no calendário';
    }
    const [y, m, d] = this.diaSelecionado.split('-').map(Number);
    return `Entregas de ${String(d).padStart(2, '0')}/${String(m).padStart(2, '0')}/${y}`;
  }

  get lista(): EntregaResumo[] {
    if (!this.diaSelecionado) {
      return [];
    }
    const txt = (s: string) => s.trim().toLowerCase();
    let r = this.todas().filter((e) => e.dataPrevista === this.diaSelecionado);
    if (this.filtroStatus) {
      r = r.filter((e) => e.status === this.filtroStatus);
    }
    if (this.filtroCliente) {
      r = r.filter((e) => e.clienteNome.toLowerCase().includes(txt(this.filtroCliente)));
    }
    if (this.filtroPet) {
      r = r.filter((e) => e.pets.some((p) => p.toLowerCase().includes(txt(this.filtroPet))));
    }
    if (this.filtroBairro) {
      r = r.filter((e) => (e.bairro ?? '').toLowerCase().includes(txt(this.filtroBairro)));
    }
    if (this.filtroCidade) {
      r = r.filter((e) => (e.cidade ?? '').toLowerCase().includes(txt(this.filtroCidade)));
    }
    if (this.atalho === 'naoConfirmadas') {
      r = r.filter((e) => e.status === 'Programada');
    } else if (this.atalho === 'reagendadas') {
      r = r.filter((e) => e.status === 'Reagendada');
    } else if (this.atalho === 'naoEntregues') {
      r = r.filter((e) => e.status === 'NaoEntregue');
    }
    return r;
  }

  get temFiltro(): boolean {
    return !!(this.filtroStatus || this.filtroCliente || this.filtroPet || this.filtroBairro || this.filtroCidade || this.atalho);
  }

  limparFiltros(): void {
    this.filtroStatus = '';
    this.filtroCliente = '';
    this.filtroPet = '';
    this.filtroBairro = '';
    this.filtroCidade = '';
    this.atalho = '';
  }

  alternarAtalho(a: string): void {
    this.atalho = this.atalho === a ? '' : a;
  }

  // ===== Calendário =====
  get tituloMes(): string {
    return this.mesRef.toLocaleDateString('pt-BR', { month: 'long', year: 'numeric' });
  }

  mudarMes(delta: number): void {
    this.mesRef = new Date(this.mesRef.getFullYear(), this.mesRef.getMonth() + delta, 1);
  }

  get celulasCalendario(): (DiaCalendario | null)[] {
    const ano = this.mesRef.getFullYear();
    const mes = this.mesRef.getMonth();
    const primeiroDiaSemana = new Date(ano, mes, 1).getDay();
    const diasNoMes = new Date(ano, mes + 1, 0).getDate();
    const pad = (n: number) => String(n).padStart(2, '0');

    const celulas: (DiaCalendario | null)[] = [];
    for (let i = 0; i < primeiroDiaSemana; i++) {
      celulas.push(null);
    }
    for (let d = 1; d <= diasNoMes; d++) {
      const dataIso = `${ano}-${pad(mes + 1)}-${pad(d)}`;
      const doDia = this.todas().filter((e) => e.dataPrevista === dataIso);
      celulas.push({
        dia: d,
        dataIso,
        total: doDia.length,
        confirmadas: doDia.filter((e) => e.status === 'ConfirmadaCliente').length,
        programadas: doDia.filter((e) => e.status === 'Programada').length,
        pendencias: doDia.filter((e) => e.status === 'NaoEntregue' || e.status === 'Reagendada').length,
      });
    }
    return celulas;
  }

  selecionarDia(c: DiaCalendario | null): void {
    if (c && c.total > 0) {
      this.diaSelecionado = c.dataIso;
      this.carregarOperacionalDoDia();
    }
  }

  hojeIso = EntregasComponent.iso(new Date());

  // ===== Resumo operacional (Casa / Personalizada) =====
  /**
   * Carrega o detalhe das entregas reais do dia selecionado (sob demanda)
   * para montar o resumo operacional. Mock já traz o operacional embutido.
   */
  private carregarOperacionalDoDia(): void {
    if (this.usandoMock() || !this.diaSelecionado) {
      return;
    }
    const pendentes = this.todas().filter((e) => e.dataPrevista === this.diaSelecionado && !e.operacional);
    if (!pendentes.length) {
      return;
    }
    this.carregandoOperacional.set(true);
    this.garantirEstoqueProdutoAcabado().subscribe(() => {
      forkJoin(pendentes.map((e) => this.service.obter(e.id))).subscribe({
        next: (detalhes) => {
          const ops = new Map(detalhes.map((d) => [d.id, operacionalDeDetalhe(d, this.estoqueProdutoAcabado())]));
          this.todas.update((arr) =>
            arr.map((e) => (ops.has(e.id) ? { ...e, operacional: ops.get(e.id) } : e)),
          );
          this.carregandoOperacional.set(false);
        },
        error: () => this.carregandoOperacional.set(false),
      });
    });
  }

  /** Carrega (uma vez) o saldo real de produto acabado da Casa, por receita + peso. */
  private garantirEstoqueProdutoAcabado(): Observable<void> {
    if (this.estoqueCarregado) {
      return of(void 0);
    }
    return forkJoin([this.estoque.listarItens(), this.estoque.listarTamanhosComPeso()]).pipe(
      map(([itens, tamanhos]) => {
        const pesoPorTamanho = new Map(tamanhos.map((t) => [t.id, t.pesoGramas]));
        const mapa = new Map<string, number>();
        for (const i of itens) {
          if (i.tipo === 'ProdutoAcabadoCasa' && i.receitaId != null && i.tamanhoPacoteId != null) {
            const peso = pesoPorTamanho.get(i.tamanhoPacoteId);
            if (peso != null) {
              mapa.set(`${i.receitaId}-${peso}`, Math.floor(i.quantidadeAtual));
            }
          }
        }
        this.estoqueProdutoAcabado.set(mapa);
        this.estoqueCarregado = true;
      }),
    );
  }

  get resumoCasaDia(): ResumoCasaDia[] {
    return resumoCasaDoDia(this.lista);
  }

  get resumoPersonalizadasDia(): ResumoPersonalizadasDia {
    return resumoPersonalizadasDoDia(this.lista);
  }

  // ===== Expandir / minimizar cartão =====
  estaExpandida(id: number): boolean {
    return this.expandidas().has(id);
  }

  alternarExpandir(id: number): void {
    this.expandidas.update((set) => {
      const novo = new Set(set);
      if (novo.has(id)) {
        novo.delete(id);
      } else {
        novo.add(id);
      }
      return novo;
    });
  }

  // ===== Detalhe =====
  abrir(e: EntregaResumo): void {
    const ref = this.dialog.open(EntregaDetalheDialogComponent, {
      data: { id: e.id },
      width: '760px',
      maxWidth: '96vw',
      autoFocus: false,
    });
    ref.afterClosed().subscribe((alterado) => {
      if (alterado) {
        this.carregar();
      }
    });
  }

  // ===== Ações do dia =====
  /** Entregas do dia selecionado ainda não incluídas em uma rota planejada. */
  get foraDaRotaCount(): number {
    return this.lista.filter((e) => e.foraDaRota).length;
  }

  /** Entregas do dia ainda não confirmadas com o cliente. */
  get naoConfirmadasCount(): number {
    return this.lista.filter((e) => e.status === 'Programada').length;
  }

  /** Entregas do dia com alguma receita da casa sem estoque suficiente. */
  get estoqueInsuficienteCount(): number {
    return this.lista.filter((e) => prontidaoEntrega(e.operacional).casaFalta > 0).length;
  }

  /**
   * Planejar a rota do dia selecionado (qualquer data, hoje ou futura).
   * Visual/preparatório: o backend de Rotas ainda não existe.
   */
  planejarRota(): void {
    if (!this.diaSelecionado) {
      this.snack.open('Selecione um dia no calendário para planejar a rota.', 'OK', { duration: 3000 });
      return;
    }
    const total = this.lista.length;
    const fora = this.foraDaRotaCount;
    const [y, m, d] = this.diaSelecionado.split('-').map(Number);
    const dataBr = `${String(d).padStart(2, '0')}/${String(m).padStart(2, '0')}/${y}`;
    this.snack.open(
      `Planejamento de rota para ${dataBr}: ${total} entrega(s)` + (fora ? `, ${fora} fora da rota.` : '.') +
        ' (Módulo de Rotas em breve)',
      'OK',
      { duration: 4000 },
    );
  }

  private erro(msg: string): void {
    this.snack.open(msg, 'Fechar', { duration: 4000 });
  }
}
