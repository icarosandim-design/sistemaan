import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { classeStatus, EntregaResumo, labelStatus, STATUS_ENTREGA } from './entregas.model';
import { EntregasService } from './entregas.service';
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
    MatTableModule,
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
  private readonly dialog = inject(MatDialog);
  private readonly snack = inject(MatSnackBar);

  readonly statusOpcoes = STATUS_ENTREGA;
  readonly labelStatus = labelStatus;
  readonly classeStatus = classeStatus;
  readonly displayedColumns = ['cliente', 'pets', 'bairro', 'cidade', 'status', 'acoes'];
  readonly diasSemana = ['Dom', 'Seg', 'Ter', 'Qua', 'Qui', 'Sex', 'Sáb'];

  readonly todas = signal<EntregaResumo[]>([]);
  readonly carregando = signal(false);
  readonly gerando = signal(false);

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
        this.todas.set(lista);
        this.carregando.set(false);
        if (inicial || !this.diaSelecionado) {
          this.selecionarDiaInicial();
        }
      },
      error: () => {
        this.carregando.set(false);
        this.erro('Falha ao carregar entregas.');
      },
    });
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
    }
  }

  hojeIso = EntregasComponent.iso(new Date());

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

  private erro(msg: string): void {
    this.snack.open(msg, 'Fechar', { duration: 4000 });
  }
}
