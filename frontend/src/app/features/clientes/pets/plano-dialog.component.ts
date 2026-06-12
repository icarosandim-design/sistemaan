import { Component, Inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatTooltipModule } from '@angular/material/tooltip';
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
  ],
  templateUrl: './plano-dialog.component.html',
  styleUrl: './plano-dialog.component.scss',
})
export class PlanoDialogComponent {
  readonly frequencias = MOCK_FREQUENCIAS;
  readonly receitasCasa = MOCK_RECEITAS_CASA;
  readonly ingredientes = MOCK_INGREDIENTES;
  readonly fmtMoeda = fmtMoeda;
  readonly fmtPeso = fmtPeso;
  readonly gramasPacotes = gramasPacotes;

  readonly pet: Pet;

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
    this.adicionarReceitaCasa();
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
  /** Recalcula a divisão/pacotes quando muda frequência, consumo ou nº de receitas. */
  onConfigChange(): void {
    this.recomputeCasa();
  }

  adicionarReceitaCasa(): void {
    this.itensCasa.push({ receitaId: null, pacotes250: 0, pacotes500: 0 });
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
      const s = sugerirPacotes(share);
      item.pacotes250 = s.p250;
      item.pacotes500 = s.p500;
    }
  }

  enviadoItemCasa(item: ItemCasa): number {
    return gramasPacotes(item.pacotes250, item.pacotes500);
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
    this.receitasPers.push({
      uid: this.uidSeq++,
      codigo: `VET-${n} ${this.pet.nome}`,
      observacoesPreparo: '',
      itens: [{ ingredienteId: null, gramasCozidas: 0 }],
      pacotes250: 0,
      pacotes500: 0,
    });
  }

  removerReceitaPers(uid: number): void {
    this.receitasPers = this.receitasPers.filter((r) => r.uid !== uid);
  }

  adicionarItemPers(r: ReceitaPersonalizada): void {
    r.itens.push({ ingredienteId: null, gramasCozidas: 0 });
  }

  removerItemPers(r: ReceitaPersonalizada, idx: number): void {
    r.itens.splice(idx, 1);
    this.aoMudarReceitaPers(r);
  }

  ingrediente(id: number | null): IngredienteMock | undefined {
    return this.ingredientes.find((x) => x.id === id);
  }

  custoItemPers(it: ItemPersonalizado): number {
    return custoIngrediente(it.gramasCozidas || 0, this.ingrediente(it.ingredienteId));
  }

  totalGramasPers(r: ReceitaPersonalizada): number {
    return r.itens.reduce((s, it) => s + (it.gramasCozidas || 0), 0);
  }

  custoReceitaPers(r: ReceitaPersonalizada): number {
    return r.itens.reduce((s, it) => s + this.custoItemPers(it), 0);
  }

  custoPorKgPers(r: ReceitaPersonalizada): number {
    const g = this.totalGramasPers(r);
    return g > 0 ? this.custoReceitaPers(r) / (g / 1000) : 0;
  }

  /** Reaplica a sugestão de pacotes quando os ingredientes mudam. */
  aoMudarReceitaPers(r: ReceitaPersonalizada): void {
    const s = sugerirPacotes(this.totalGramasPers(r));
    r.pacotes250 = s.p250;
    r.pacotes500 = s.p500;
  }

  fechar(): void {
    this.ref.close();
  }
}
