import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { fmtPeso, FichaMock, StatusFichaMock } from './producao.mock';
import { ProducaoMockService } from './producao-mock.service';

@Component({
  selector: 'app-producao-cozinha',
  standalone: true,
  imports: [RouterLink, MatButtonModule, MatIconModule, MatTooltipModule],
  templateUrl: './cozinha.component.html',
  styleUrl: './cozinha.component.scss',
})
export class ProducaoCozinhaComponent {
  readonly mock = inject(ProducaoMockService);
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

  naoFeita(f: FichaMock): void {
    this.mock.mudarStatusFicha(f.id, 'NaoFeita');
  }

  reabrir(f: FichaMock): void {
    this.mock.mudarStatusFicha(f.id, 'Pendente');
  }
}
