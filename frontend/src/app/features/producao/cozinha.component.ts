import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { FichaProducao, StatusFicha, fmtPeso } from './producao.model';
import { ProducaoStore } from './producao.store';
import { ConcluirFichaDialogComponent, ConcluirFichaResult } from './concluir-ficha-dialog.component';
import { AuthService } from '../../core/auth/auth.service';
import { PERFIL } from '../../core/auth/perfis';

interface Coluna {
  key: string;
  titulo: string;
  statuses: StatusFicha[];
}

const AVANCO: StatusFicha[] = ['Pendente', 'EmPreparo', 'Produzida', 'Envasada'];

@Component({
  selector: 'app-producao-cozinha',
  standalone: true,
  imports: [RouterLink, MatButtonModule, MatIconModule, MatTooltipModule, MatProgressSpinnerModule],
  templateUrl: './cozinha.component.html',
  styleUrl: './cozinha.component.scss',
})
export class ProducaoCozinhaComponent implements OnInit {
  readonly store = inject(ProducaoStore);
  private readonly dialog = inject(MatDialog);
  private readonly snack = inject(MatSnackBar);
  private readonly auth = inject(AuthService);
  readonly fmtPeso = fmtPeso;

  /** Operador apenas visualiza; Administrador e Cozinha operam. */
  readonly podeOperar = computed(() => this.auth.temPapel(PERFIL.ADMIN, PERFIL.COZINHA));

  readonly mostrarConcluidas = signal(false);

  readonly colunas: Coluna[] = [
    { key: 'afazer', titulo: 'A fazer', statuses: ['Pendente'] },
    { key: 'producao', titulo: 'Em produção', statuses: ['EmPreparo', 'Produzida'] },
    { key: 'envasando', titulo: 'Envasando', statuses: ['Envasada'] },
  ];

  ngOnInit(): void {
    this.store.garantirOrdemDia();
  }

  fmtData(iso: string): string {
    if (!iso) return '—';
    const [y, m, d] = iso.split('-');
    return `${d}/${m}/${y}`;
  }

  readonly fichas = computed(() => this.store.ordem()?.fichas ?? []);
  readonly concluidas = computed(() => this.fichas().filter((f) => f.status === 'Conferida'));
  readonly naoFeitas = computed(() => this.fichas().filter((f) => f.status === 'NaoFeita'));

  porColuna(col: Coluna): FichaProducao[] {
    return this.fichas().filter((f) => col.statuses.includes(f.status));
  }

  ehEnvasada(f: FichaProducao): boolean {
    return f.status === 'Envasada';
  }

  async avancar(f: FichaProducao): Promise<void> {
    const i = AVANCO.indexOf(f.status);
    if (i >= 0 && i < AVANCO.length - 1) {
      try {
        await this.store.mudarStatusFicha(f.id, AVANCO[i + 1]);
      } catch {
        this.snack.open('Não foi possível avançar a ficha.', 'OK', { duration: 3000 });
      }
    }
  }

  concluir(f: FichaProducao): void {
    this.abrirConclusao(f, false);
  }

  naoFeita(f: FichaProducao): void {
    this.abrirConclusao(f, true);
  }

  private abrirConclusao(f: FichaProducao, naoFeita: boolean): void {
    const ref = this.dialog.open(ConcluirFichaDialogComponent, { data: { ficha: f, naoFeita }, autoFocus: false });
    ref.afterClosed().subscribe(async (res: ConcluirFichaResult | undefined) => {
      if (!res) return;
      try {
        if (res.naoFeita) {
          await this.store.marcarNaoFeita(f.id, res.motivo ?? '');
          this.snack.open(`${f.petNome || f.receitaNome} marcada como não feita.`, 'OK', { duration: 3000 });
        } else {
          await this.store.concluirFicha(f.id, {
            pacotesReais: res.pacotesReais ?? f.quantidadePacotes,
            pesoEnvasadoGramas: res.pesoEnvasadoGramas,
            observacoes: res.observacoes,
          });
          if (f.tipo === 'Casa') {
            this.snack.open(`${f.receitaNome} conferida (${res.pacotesReais ?? f.quantidadePacotes} pacotes).`, 'OK', { duration: 3000 });
          } else {
            this.snack.open(`${f.petNome} conferida.`, 'OK', { duration: 2500 });
          }
        }
      } catch {
        this.snack.open('Não foi possível registrar a ficha.', 'OK', { duration: 3000 });
      }
    });
  }

  async reabrir(f: FichaProducao): Promise<void> {
    try {
      await this.store.mudarStatusFicha(f.id, 'Pendente');
    } catch {
      this.snack.open('Não foi possível reabrir.', 'OK', { duration: 3000 });
    }
  }
}
