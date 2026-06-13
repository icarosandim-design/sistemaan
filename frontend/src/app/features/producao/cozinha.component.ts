import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { fmtPeso, FichaMock, StatusFichaMock } from './producao.mock';
import { ProducaoMockService } from './producao-mock.service';
import { ConcluirFichaDialogComponent, ConcluirFichaResult } from './concluir-ficha-dialog.component';

@Component({
  selector: 'app-producao-cozinha',
  standalone: true,
  imports: [RouterLink, MatButtonModule, MatIconModule, MatTooltipModule],
  templateUrl: './cozinha.component.html',
  styleUrl: './cozinha.component.scss',
})
export class ProducaoCozinhaComponent {
  readonly mock = inject(ProducaoMockService);
  private readonly dialog = inject(MatDialog);
  private readonly snack = inject(MatSnackBar);
  readonly fmtPeso = fmtPeso;

  readonly mostrarConcluidas = signal(false);

  readonly colunas: { status: StatusFichaMock; titulo: string }[] = [
    { status: 'Pendente', titulo: 'A fazer' },
    { status: 'EmProducao', titulo: 'Em produção' },
    { status: 'Envasando', titulo: 'Envasando' },
  ];

  porStatus = (s: StatusFichaMock): FichaMock[] => this.mock.fichas().filter((f) => f.status === s);

  readonly concluidas = computed(() => this.mock.fichasConcluidas());
  readonly naoFeitas = computed(() => this.mock.fichasNaoFeitas());

  avancar(f: FichaMock): void {
    this.mock.avancarFicha(f.id);
  }

  concluir(f: FichaMock): void {
    this.abrirConclusao(f, 'Concluida');
  }

  naoFeita(f: FichaMock): void {
    this.abrirConclusao(f, 'NaoFeita');
  }

  private abrirConclusao(f: FichaMock, modoInicial: StatusFichaMock): void {
    const ref = this.dialog.open(ConcluirFichaDialogComponent, { data: { ficha: f, modoInicial }, autoFocus: false });
    ref.afterClosed().subscribe((res: ConcluirFichaResult | undefined) => {
      if (!res) {
        return;
      }
      this.mock.concluirFicha(f.id, res);
      if (res.status === 'NaoFeita') {
        this.snack.open(`${f.pet || f.receitaNome} marcada como não feita.`, 'OK', { duration: 3000 });
      } else if (f.tipo === 'Casa') {
        this.snack.open(`Estoque: +${res.pacotesFeitos ?? f.pacotes} pacote(s) de ${f.receitaNome} (mock).`, 'OK', { duration: 3500 });
      } else {
        this.snack.open(`${f.pet} concluída.`, 'OK', { duration: 2500 });
      }
    });
  }

  reabrir(f: FichaMock): void {
    this.mock.mudarStatusFicha(f.id, 'Pendente');
  }
}
