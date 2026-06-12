import { Component, Inject, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { TamanhoPacote } from '../../tamanhos-pacote/tamanhos-pacote.model';
import { TamanhosPacoteService } from '../../tamanhos-pacote/tamanhos-pacote.service';
import { Pet } from './pet.model';
import {
  custoIngrediente,
  fmtMoeda,
  fmtPeso,
  gramasPacotes,
  IngredienteMock,
  ItemCasa,
  ItemPersonalizado,
  MOCK_FREQUENCIAS,
  MOCK_INGREDIENTES,
  MOCK_RECEITAS_CASA,
  ReceitaPersonalizada,
  sugerirPacotes,
  TipoAlimentacao,
} from './plano.model';

export interface PlanoDialogData {
  pet: Pet;
}

@Component({
  selector: 'app-plano-dialog',
  standalone: true,
  imports: [
    FormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatButtonToggleModule,
    MatTooltipModule,
    MatProgressBarModule,
  ],
  templateUrl: './plano-dialog.component.html',
  styleUrl: './plano-dialog.component.scss',
})
export class PlanoDialogComponent {
  private readonly tamanhosService = inject(TamanhosPacoteService);

  readonly frequencias = MOCK_FREQUENCIAS;
  readonly receitasCasa = MOCK_RECEITAS_CASA;
  readonly ingredientes = MOCK_INGREDIENTES;
  readonly fmtMoeda = fmtMoeda;
  readonly fmtPeso = fmtPeso;

  readonly pet: Pet;

  // Tamanhos de pacote ATIVOS, vindos do cadastro real.
  tamanhos: TamanhoPacote[] = [];
  carregandoTamanhos = true;

  // ----- Estado do plano (mock, não persistido) -----
  frequenciaId: number | null = 2; // quinzenal por padrão
  primeiraEntrega: string | null = null;
  gramasDiaAjustadas: number | null = null;
  tipo: TipoAlimentacao = 'Casa';
  itensCasa: ItemCasa[] = [];
  receitasPers: ReceitaPersonalizada[] = [];
  private uidSeq = 1;

  constructor(
    private readonly ref: MatDialogRef<PlanoDialogComponent>,
    @Inject(MAT_DIALOG_DATA) data: PlanoDialogData,
  ) {
    this.pet = data.pet;
    this.gramasDiaAjustadas = data.pet.gramasDiaAjustadas ?? data.pet.gramasDiaSugeridas ?? null;
    this.itensCasa.push({ receitaId: null, pacotes: {} });

    this.tamanhosService.listar().subscribe({
      next: (lista) => {
        this.tamanhos = lista.filter((t) => t.ativo).sort((a, b) => a.pesoGramas - b.pesoGramas);
        this.carregandoTamanhos = false;
        this.recomputeCasa();
      },
      error: () => {
        this.carregandoTamanhos = false;
      },
    });
  }

  get semTamanhos(): boolean {
    return !this.carregandoTamanhos && this.tamanhos.length === 0;
  }

  // ===== Ciclo =====
  get gramasDiaSugeridas(): number | null {
    return this.pet.gramasDiaSugeridas;
  }

  get gramasDia(): number {
    return this.gramasDiaAjustadas ?? this.gramasDiaSugeridas ?? 0;
  }

  get diasCiclo(): number {
    return this.frequencias.find((f) => f.id === this.frequenciaId)?.diasCiclo ?? 0;
  }

  get totalCiclo(): number {
    return this.gramasDia * this.diasCiclo;
  }

  // ===== Receita da Casa =====
  onConfigChange(): void {
    this.recomputeCasa();
  }

  adicionarReceitaCasa(): void {
    this.itensCasa.push({ receitaId: null, pacotes: {} });
    this.recomputeCasa();
  }

  removerReceitaCasa(i: number): void {
    this.itensCasa.splice(i, 1);
    this.recomputeCasa();
  }

  /** Gramas que cabem a cada receita (divisão igual do total do ciclo). */
  get gramasPorReceita(): number {
    return this.itensCasa.length ? this.totalCiclo / this.itensCasa.length : 0;
  }

  /** (Re)aplica a sugestão de pacotes para a parte de cada receita. */
  recomputeCasa(): void {
    const share = this.gramasPorReceita;
    for (const item of this.itensCasa) {
      item.pacotes = sugerirPacotes(share, this.tamanhos);
    }
  }

  enviadoItemCasa(item: ItemCasa): number {
    return gramasPacotes(item.pacotes, this.tamanhos);
  }

  get totalEnviadoCasa(): number {
    return this.itensCasa.reduce((s, i) => s + this.enviadoItemCasa(i), 0);
  }

  get diferencaCasa(): number {
    return this.totalEnviadoCasa - this.totalCiclo;
  }

  get statusCasa(): 'vazio' | 'faltando' | 'atendido' {
    if (this.totalCiclo <= 0) {
      return 'vazio';
    }
    return this.totalEnviadoCasa < this.totalCiclo ? 'faltando' : 'atendido';
  }

  // ===== Receita Personalizada =====
  adicionarReceitaPers(): void {
    const n = String(this.receitasPers.length + 1).padStart(3, '0');
    const r: ReceitaPersonalizada = {
      uid: this.uidSeq++,
      codigo: `VET-${n} ${this.pet.nome}`,
      observacoesPreparo: '',
      itens: [{ ingredienteId: null, gramasCozidas: 0 }],
      quantidadePacotes: 1,
    };
    this.receitasPers.push(r);
  }

  removerReceitaPers(uid: number): void {
    this.receitasPers = this.receitasPers.filter((r) => r.uid !== uid);
  }

  adicionarItemPers(r: ReceitaPersonalizada): void {
    r.itens.push({ ingredienteId: null, gramasCozidas: 0 });
  }

  removerItemPers(r: ReceitaPersonalizada, idx: number): void {
    r.itens.splice(idx, 1);
  }

  ingrediente(id: number | null): IngredienteMock | undefined {
    return this.ingredientes.find((x) => x.id === id);
  }

  custoItemPers(it: ItemPersonalizado): number {
    return custoIngrediente(it.gramasCozidas || 0, this.ingrediente(it.ingredienteId));
  }

  /** Tamanho do pacote da receita = soma dos ingredientes cozidos. */
  tamanhoPacotePers(r: ReceitaPersonalizada): number {
    return r.itens.reduce((s, it) => s + (it.gramasCozidas || 0), 0);
  }

  /** Total da receita no ciclo = tamanho do pacote × quantidade de pacotes. */
  totalCicloPers(r: ReceitaPersonalizada): number {
    return this.tamanhoPacotePers(r) * (r.quantidadePacotes || 0);
  }

  custoReceitaPers(r: ReceitaPersonalizada): number {
    return r.itens.reduce((s, it) => s + this.custoItemPers(it), 0);
  }

  custoPorKgPers(r: ReceitaPersonalizada): number {
    const g = this.tamanhoPacotePers(r);
    return g > 0 ? this.custoReceitaPers(r) / (g / 1000) : 0;
  }

  /** Custo da receita no ciclo = custo por pacote × quantidade de pacotes. */
  custoCicloPers(r: ReceitaPersonalizada): number {
    return this.custoReceitaPers(r) * (r.quantidadePacotes || 0);
  }

  get totalInformadoPers(): number {
    return this.receitasPers.reduce((s, r) => s + this.totalCicloPers(r), 0);
  }

  get totalPacotesPers(): number {
    return this.receitasPers.reduce((s, r) => s + (r.quantidadePacotes || 0), 0);
  }

  fechar(): void {
    this.ref.close();
  }
}
