import { Component, Inject, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar } from '@angular/material/snack-bar';
import { forkJoin, map, switchMap, tap } from 'rxjs';
import { FrequenciaEntrega } from '../../frequencias/frequencias.model';
import { FrequenciasService } from '../../frequencias/frequencias.service';
import { Ingrediente } from '../../ingredientes/ingredientes.model';
import { IngredientesService } from '../../ingredientes/ingredientes.service';
import { ReceitaCasa } from '../../receitas/receitas.model';
import { ReceitasService } from '../../receitas/receitas.service';
import { TamanhoPacote } from '../../tamanhos-pacote/tamanhos-pacote.model';
import { TamanhosPacoteService } from '../../tamanhos-pacote/tamanhos-pacote.service';
import { Pet } from './pet.model';
import { PlanoService } from './plano.service';
import { ReceitaPersonalizadaService } from './receita-personalizada.service';
import {
  custoIngrediente,
  fmtMoeda,
  fmtPeso,
  gramasPacotes,
  ItemCasa,
  ItemPersonalizado,
  Pacotes,
  PlanoItemPacoteApi,
  ReceitaPersEdit,
  SalvarPlanoItemRequest,
  SalvarPlanoRequest,
  SalvarReceitaPersonalizadaRequest,
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
  private readonly planoService = inject(PlanoService);
  private readonly receitaPersService = inject(ReceitaPersonalizadaService);
  private readonly frequenciasService = inject(FrequenciasService);
  private readonly receitasService = inject(ReceitasService);
  private readonly ingredientesService = inject(IngredientesService);
  private readonly tamanhosService = inject(TamanhosPacoteService);
  private readonly snack = inject(MatSnackBar);

  readonly fmtMoeda = fmtMoeda;
  readonly fmtPeso = fmtPeso;
  readonly pet: Pet;

  // Listas reais (apenas ativos).
  frequencias: FrequenciaEntrega[] = [];
  receitasCasa: ReceitaCasa[] = [];
  ingredientes: Ingrediente[] = [];
  tamanhos: TamanhoPacote[] = [];

  carregando = true;
  salvando = false;

  // Estado do plano.
  frequenciaId: number | null = null;
  primeiraEntrega: string | null = null;
  gramasDiaAjustadas: number | null = null;
  tipo: TipoAlimentacao = 'Casa';
  itensCasa: ItemCasa[] = [];
  receitasPers: ReceitaPersEdit[] = [];
  private uidSeq = 1;

  constructor(
    private readonly ref: MatDialogRef<PlanoDialogComponent, boolean>,
    @Inject(MAT_DIALOG_DATA) data: PlanoDialogData,
  ) {
    this.pet = data.pet;
    this.gramasDiaAjustadas = data.pet.gramasDiaAjustadas ?? data.pet.gramasDiaSugeridas ?? null;

    forkJoin({
      freq: this.frequenciasService.listar(),
      rec: this.receitasService.listar(),
      ing: this.ingredientesService.listar(),
      tam: this.tamanhosService.listar(),
      plano: this.planoService.obterPorPet(this.pet.id),
      pers: this.receitaPersService.listarPorPet(this.pet.id),
    }).subscribe({
      next: ({ freq, rec, ing, tam, plano, pers }) => {
        this.frequencias = freq.filter((f) => f.ativo);
        this.receitasCasa = rec.filter((r) => r.ativo);
        this.ingredientes = ing.filter((i) => i.ativo);
        this.tamanhos = tam.filter((t) => t.ativo).sort((a, b) => a.pesoGramas - b.pesoGramas);

        if (plano) {
          this.frequenciaId = plano.frequenciaEntregaId;
          this.primeiraEntrega = plano.primeiraEntrega;
          this.gramasDiaAjustadas = plano.gramasDiaAjustadas ?? this.gramasDiaAjustadas;
          this.tipo = plano.tipo;

          if (plano.tipo === 'Casa') {
            this.itensCasa = plano.itens.map((i) => ({
              receitaId: i.receitaId,
              pacotes: this.paraRecord(i.pacotes),
            }));
            if (this.itensCasa.length === 0) {
              this.adicionarReceitaCasa();
            }
          } else {
            this.receitasPers = plano.itens.map((i) => {
              const rp = pers.find((p) => p.id === i.receitaId);
              return {
                uid: this.uidSeq++,
                id: i.receitaId,
                codigo: rp?.codigo ?? '',
                observacoesPreparo: rp?.observacoes ?? '',
                itens: (rp?.itens ?? []).map((it) => ({ ingredienteId: it.ingredienteId, gramasCozidas: it.gramas })),
                quantidadePacotes: i.quantidadePacotes ?? 1,
              };
            });
          }
        } else {
          this.frequenciaId = this.frequencias[0]?.id ?? null;
          this.adicionarReceitaCasa();
        }

        this.carregando = false;
      },
      error: () => {
        this.carregando = false;
        this.erro('Falha ao carregar os dados do plano.');
      },
    });
  }

  get semTamanhos(): boolean {
    return !this.carregando && this.tamanhos.length === 0;
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

  get gramasPorReceita(): number {
    return this.itensCasa.length ? this.totalCiclo / this.itensCasa.length : 0;
  }

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
    this.receitasPers.push({
      uid: this.uidSeq++,
      id: null,
      codigo: `VET-${n} ${this.pet.nome}`,
      observacoesPreparo: '',
      itens: [{ ingredienteId: null, gramasCozidas: 0 }],
      quantidadePacotes: 1,
    });
  }

  removerReceitaPers(uid: number): void {
    this.receitasPers = this.receitasPers.filter((r) => r.uid !== uid);
  }

  adicionarItemPers(r: ReceitaPersEdit): void {
    r.itens.push({ ingredienteId: null, gramasCozidas: 0 });
  }

  removerItemPers(r: ReceitaPersEdit, idx: number): void {
    r.itens.splice(idx, 1);
  }

  ingrediente(id: number | null): Ingrediente | undefined {
    return this.ingredientes.find((x) => x.id === id);
  }

  custoItemPers(it: ItemPersonalizado): number {
    return custoIngrediente(it.gramasCozidas || 0, this.ingrediente(it.ingredienteId));
  }

  tamanhoPacotePers(r: ReceitaPersEdit): number {
    return r.itens.reduce((s, it) => s + (it.gramasCozidas || 0), 0);
  }

  totalCicloPers(r: ReceitaPersEdit): number {
    return this.tamanhoPacotePers(r) * (r.quantidadePacotes || 0);
  }

  custoReceitaPers(r: ReceitaPersEdit): number {
    return r.itens.reduce((s, it) => s + this.custoItemPers(it), 0);
  }

  custoPorKgPers(r: ReceitaPersEdit): number {
    const g = this.tamanhoPacotePers(r);
    return g > 0 ? this.custoReceitaPers(r) / (g / 1000) : 0;
  }

  custoCicloPers(r: ReceitaPersEdit): number {
    return this.custoReceitaPers(r) * (r.quantidadePacotes || 0);
  }

  get totalInformadoPers(): number {
    return this.receitasPers.reduce((s, r) => s + this.totalCicloPers(r), 0);
  }

  get totalPacotesPers(): number {
    return this.receitasPers.reduce((s, r) => s + (r.quantidadePacotes || 0), 0);
  }

  // ===== Salvar =====
  salvar(): void {
    if (!this.frequenciaId) {
      this.erro('Selecione a frequência de entrega.');
      return;
    }
    if (!this.primeiraEntrega) {
      this.erro('Informe a primeira data de entrega.');
      return;
    }

    if (this.tipo === 'Casa') {
      this.salvarCasa();
    } else {
      this.salvarPersonalizada();
    }
  }

  private salvarCasa(): void {
    const itens: SalvarPlanoItemRequest[] = this.itensCasa
      .filter((i) => i.receitaId)
      .map((i) => ({
        receitaId: i.receitaId!,
        quantidadeCicloGramas: Math.round(this.gramasPorReceita),
        pacotes: this.paraArray(i.pacotes),
      }));

    if (itens.length === 0) {
      this.erro('Inclua ao menos uma receita da casa.');
      return;
    }

    this.persistirPlano({ ...this.basePlano(), tipo: 'Casa', itens });
  }

  private salvarPersonalizada(): void {
    if (this.receitasPers.length === 0) {
      this.erro('Inclua ao menos uma receita personalizada.');
      return;
    }

    const saves = this.receitasPers.map((r) => {
      const req: SalvarReceitaPersonalizadaRequest = {
        codigo: r.codigo.trim(),
        nome: r.codigo.trim() || 'Receita personalizada',
        ativo: true,
        observacoes: r.observacoesPreparo,
        itens: r.itens
          .filter((it) => it.ingredienteId)
          .map((it) => ({ ingredienteId: it.ingredienteId!, gramas: it.gramasCozidas })),
      };
      const obs = r.id
        ? this.receitaPersService.atualizar(r.id, req)
        : this.receitaPersService.criar(this.pet.id, req);
      return obs.pipe(map((dto) => ({ uid: r.uid, id: dto.id, qtd: r.quantidadePacotes })));
    });

    this.salvando = true;
    forkJoin(saves)
      .pipe(
        tap((res) => res.forEach((x) => {
          const r = this.receitasPers.find((rr) => rr.uid === x.uid);
          if (r) {
            r.id = x.id; // evita recriar em caso de retry
          }
        })),
        switchMap((res) => {
          const itens: SalvarPlanoItemRequest[] = res.map((x) => ({ receitaId: x.id, quantidadePacotes: x.qtd }));
          return this.planoService.salvar(this.pet.id, { ...this.basePlano(), tipo: 'Personalizada', itens });
        }),
      )
      .subscribe({
        next: () => this.sucesso(),
        error: (e: HttpErrorResponse) => {
          this.salvando = false;
          this.erro(this.mensagemErro(e));
        },
      });
  }

  private persistirPlano(req: SalvarPlanoRequest): void {
    this.salvando = true;
    this.planoService.salvar(this.pet.id, req).subscribe({
      next: () => this.sucesso(),
      error: (e: HttpErrorResponse) => {
        this.salvando = false;
        this.erro(this.mensagemErro(e));
      },
    });
  }

  private basePlano(): Omit<SalvarPlanoRequest, 'tipo' | 'itens'> {
    return {
      frequenciaEntregaId: this.frequenciaId!,
      primeiraEntrega: this.primeiraEntrega!,
      gramasDiaSugeridas: this.gramasDiaSugeridas,
      gramasDiaAjustadas: this.gramasDiaAjustadas,
    };
  }

  private sucesso(): void {
    this.salvando = false;
    this.snack.open('Plano alimentar salvo.', 'OK', { duration: 2500 });
    this.ref.close(true);
  }

  fechar(): void {
    this.ref.close();
  }

  // ===== Mapeamentos de pacotes (Casa) =====
  private paraRecord(pacotes: PlanoItemPacoteApi[]): Pacotes {
    const rec: Pacotes = {};
    for (const t of this.tamanhos) {
      rec[t.id] = 0;
    }
    for (const p of pacotes) {
      rec[p.tamanhoPacoteId] = p.quantidade;
    }
    return rec;
  }

  private paraArray(pacotes: Pacotes): PlanoItemPacoteApi[] {
    return this.tamanhos
      .map((t) => ({ tamanhoPacoteId: t.id, quantidade: pacotes[t.id] || 0 }))
      .filter((p) => p.quantidade > 0);
  }

  private mensagemErro(e: HttpErrorResponse): string {
    const errors = e.error?.errors as Record<string, string[]> | undefined;
    if (errors) {
      const primeira = Object.values(errors)[0]?.[0];
      if (primeira) {
        return primeira;
      }
    }
    return e.error?.detail ?? 'Não foi possível salvar o plano.';
  }

  private erro(msg: string): void {
    this.snack.open(msg, 'Fechar', { duration: 4000 });
  }
}
