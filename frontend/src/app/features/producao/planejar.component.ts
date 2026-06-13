import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { fmtPeso, rotuloProntidao } from './producao.mock';
import { ProducaoMockService } from './producao-mock.service';

// Fator MOCK para estimar cru a partir do cozido (no backend real virá do coeficiente por ingrediente).
const FATOR_CRU_MOCK = 1.8;

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

  busca = '';
  apenasNaoProntas = false;

  get personalizadasFiltradas() {
    const t = this.busca.trim().toLowerCase();
    return this.mock.personalizadas().filter((p) => {
      if (this.apenasNaoProntas && p.prontidao !== 'NaoPronta') {
        return false;
      }
      if (t && !p.pet.toLowerCase().includes(t) && !p.cliente.toLowerCase().includes(t)) {
        return false;
      }
      return true;
    });
  }

  get personalizadasSelecionadas(): number {
    return this.mock.personalizadas().filter((p) => p.selecionada).length;
  }

  get casaSelecionadas(): number {
    return this.mock.casa().filter((c) => c.incluir).length;
  }

  // ----- Resumo das escolhas -----
  get totalCozidoSelecionado(): number {
    const pers = this.mock.personalizadas().filter((p) => p.selecionada).reduce((s, p) => s + p.pacotes * p.pesoPacoteGramas, 0);
    const casa = this.mock.casa().filter((c) => c.incluir).reduce((s, c) => s + c.qtdProduzir * this.tamanhoGramas(c.tamanho), 0);
    return pers + casa;
  }

  get totalCruSelecionado(): number {
    return Math.round(this.totalCozidoSelecionado * FATOR_CRU_MOCK);
  }

  private tamanhoGramas(tamanho: string): number {
    const n = parseInt(tamanho, 10);
    return Number.isFinite(n) ? n : 0;
  }

  falta(necessario: number, estoque: number): number {
    return Math.max(0, necessario - estoque);
  }

  /** Data real da entrega (protótipo: hoje + dias). */
  dataDe(dias: number): string {
    const d = new Date();
    d.setDate(d.getDate() + dias);
    return d.toLocaleDateString('pt-BR');
  }
}
