import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { fmtPeso, rotuloProntidao } from './producao.mock';
import { ProducaoMockService } from './producao-mock.service';

@Component({
  selector: 'app-producao-planejar',
  standalone: true,
  imports: [FormsModule, RouterLink, MatButtonModule, MatIconModule, MatCheckboxModule, MatInputModule, MatFormFieldModule],
  templateUrl: './planejar.component.html',
  styleUrl: './planejar.component.scss',
})
export class ProducaoPlanejarComponent {
  readonly mock = inject(ProducaoMockService);
  readonly fmtPeso = fmtPeso;
  readonly rotuloProntidao = rotuloProntidao;

  readonly janelas = ['Hoje', 'Amanhã', 'Próximos 3 dias', 'Próximos 7 dias', 'Próxima semana'];
  janela = 'Próximos 7 dias';

  falta(necessario: number, estoque: number): number {
    return Math.max(0, necessario - estoque);
  }
}
