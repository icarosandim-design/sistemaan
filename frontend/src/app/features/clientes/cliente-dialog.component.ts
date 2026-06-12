import { DatePipe } from '@angular/common';
import { Component, Inject, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialog, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatTabsModule } from '@angular/material/tabs';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar } from '@angular/material/snack-bar';
import { HttpErrorResponse } from '@angular/common/http';
import { forkJoin } from 'rxjs';
import { FrequenciaEntrega } from '../frequencias/frequencias.model';
import { FrequenciasService } from '../frequencias/frequencias.service';
import { labelSexo, Pet, SalvarPetRequest } from './pets/pet.model';
import { PetDialogComponent } from './pets/pet-dialog.component';
import { PlanoDialogComponent } from './pets/plano-dialog.component';
import { PetService } from './pets/pet.service';
import { PlanoService } from './pets/plano.service';
import { fmtPeso, PlanoAlimentar } from './pets/plano.model';
import {
  Cliente,
  FormaPagamento,
  FORMAS_PAGAMENTO,
  ORIGENS_VENDA,
  SalvarClienteRequest,
  STATUS_FINANCEIRO,
  StatusFinanceiro,
  TipoCliente,
  TIPOS_CLIENTE,
  UFS,
} from './clientes.model';

export interface ClienteDialogData {
  cliente: Cliente | null;
}

@Component({
  selector: 'app-cliente-dialog',
  standalone: true,
  imports: [
    DatePipe,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatTooltipModule,
    MatTabsModule,
    MatProgressBarModule,
  ],
  templateUrl: './cliente-dialog.component.html',
  styleUrl: './cliente-dialog.component.scss',
})
export class ClienteDialogComponent {
  private readonly fb = inject(FormBuilder);
  private readonly petDialog = inject(MatDialog);
  private readonly petService = inject(PetService);
  private readonly planoService = inject(PlanoService);
  private readonly frequenciasService = inject(FrequenciasService);
  private readonly snack = inject(MatSnackBar);

  // Pets (persistidos via API — apenas para clientes já salvos)
  readonly pets = signal<Pet[]>([]);
  readonly planos = signal<Record<number, PlanoAlimentar>>({});
  readonly carregandoPets = signal(false);
  readonly clienteId: number | null;
  readonly labelSexo = labelSexo;
  readonly fmtPeso = fmtPeso;

  readonly frequencias = signal<FrequenciaEntrega[]>([]);

  readonly tipos = TIPOS_CLIENTE;
  readonly formas = FORMAS_PAGAMENTO;
  readonly statusFin = STATUS_FINANCEIRO;
  readonly origens = ORIGENS_VENDA;
  readonly ufs = UFS;
  readonly edicao: boolean;

  readonly form = this.fb.nonNullable.group({
    nome: ['', [Validators.required]],
    cpf: [''],
    telefone: [''],
    email: ['', [Validators.email]],
    origemVenda: ['' as string | null],
    rua: [''],
    numero: [''],
    complemento: [''],
    cep: [''],
    bairro: [''],
    cidade: [''],
    estado: ['' as string | null],
    frequenciaEntregaId: [null as number | null],
    primeiraEntrega: ['' as string | null],
    observacoes: [''],
    tipoCliente: ['Assinante' as TipoCliente],
    formaPagamento: ['Pix' as FormaPagamento | null],
    diaCobranca: [null as number | null, [Validators.min(1), Validators.max(31)]],
    valorRecorrenteMensal: [0, [Validators.min(0)]],
    statusFinanceiro: ['EmDia' as StatusFinanceiro],
    observacoesFinanceiras: [''],
  });

  constructor(
    private readonly ref: MatDialogRef<ClienteDialogComponent, SalvarClienteRequest>,
    @Inject(MAT_DIALOG_DATA) readonly data: ClienteDialogData,
  ) {
    this.edicao = !!data.cliente;
    this.clienteId = data.cliente?.id ?? null;

    this.frequenciasService.listar().subscribe({
      next: (fs) => this.frequencias.set(fs.filter((f) => f.ativo)),
      error: () => undefined,
    });

    if (data.cliente) {
      const c = data.cliente;
      this.carregarPets();
      this.form.patchValue({
        nome: c.nome,
        cpf: c.cpf ?? '',
        telefone: c.telefone ?? '',
        email: c.email ?? '',
        origemVenda: c.origemVenda ?? null,
        rua: c.rua ?? '',
        numero: c.numero ?? '',
        complemento: c.complemento ?? '',
        cep: c.cep ?? '',
        bairro: c.bairro ?? '',
        cidade: c.cidade ?? '',
        estado: c.estado ?? null,
        frequenciaEntregaId: c.frequenciaEntregaId ?? null,
        primeiraEntrega: c.primeiraEntrega ?? null,
        observacoes: c.observacoes ?? '',
        tipoCliente: c.tipoCliente,
        formaPagamento: c.formaPagamento ?? null,
        diaCobranca: c.diaCobranca ?? null,
        valorRecorrenteMensal: c.valorRecorrenteMensal,
        statusFinanceiro: c.statusFinanceiro,
        observacoesFinanceiras: c.observacoesFinanceiras ?? '',
      });
    }
  }

  salvar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const v = this.form.getRawValue();
    const txt = (s: string) => (s.trim() ? s.trim() : null);
    const req: SalvarClienteRequest = {
      nome: v.nome.trim(),
      cpf: txt(v.cpf),
      telefone: txt(v.telefone),
      email: txt(v.email),
      origemVenda: v.origemVenda || null,
      observacoes: txt(v.observacoes),
      rua: txt(v.rua),
      numero: txt(v.numero),
      complemento: txt(v.complemento),
      cep: txt(v.cep),
      bairro: txt(v.bairro),
      cidade: txt(v.cidade),
      estado: v.estado || null,
      frequenciaEntregaId: v.frequenciaEntregaId ?? null,
      primeiraEntrega: v.primeiraEntrega || null,
      tipoCliente: v.tipoCliente,
      formaPagamento: v.formaPagamento || null,
      diaCobranca: v.diaCobranca ?? null,
      valorRecorrenteMensal: Number(v.valorRecorrenteMensal) || 0,
      statusFinanceiro: v.statusFinanceiro,
      observacoesFinanceiras: txt(v.observacoesFinanceiras),
    };
    this.ref.close(req);
  }

  cancelar(): void {
    this.ref.close();
  }

  // ===== Pets (API) =====
  private carregarPets(): void {
    if (this.clienteId === null) {
      return;
    }
    this.carregandoPets.set(true);
    this.petService.listarPorCliente(this.clienteId).subscribe({
      next: (lista) => {
        this.pets.set(lista);
        this.carregandoPets.set(false);
        this.carregarPlanos(lista);
      },
      error: () => {
        this.carregandoPets.set(false);
        this.erro('Falha ao carregar os pets.');
      },
    });
  }

  private carregarPlanos(lista: Pet[]): void {
    if (lista.length === 0) {
      this.planos.set({});
      return;
    }
    forkJoin(lista.map((p) => this.planoService.obterPorPet(p.id))).subscribe({
      next: (resultados) => {
        const mapa: Record<number, PlanoAlimentar> = {};
        resultados.forEach((plano, i) => {
          if (plano) {
            mapa[lista[i].id] = plano;
          }
        });
        this.planos.set(mapa);
      },
      error: () => undefined,
    });
  }

  // ===== Resumo do Plano no card do pet =====
  planoDoPet(petId: number): PlanoAlimentar | undefined {
    return this.planos()[petId];
  }

  private get diasCicloCliente(): number {
    const id = this.form.controls.frequenciaEntregaId.value;
    return this.frequencias().find((f) => f.id === id)?.diasCiclo ?? 0;
  }

  nomeFrequenciaCliente(): string {
    const id = this.form.controls.frequenciaEntregaId.value;
    return this.frequencias().find((f) => f.id === id)?.nome ?? '—';
  }

  totalCicloPet(p: Pet): string {
    const plano = this.planoDoPet(p.id);
    const dias = this.diasCicloCliente;
    if (!plano || dias <= 0) {
      return '—';
    }
    const gramasDia = plano.gramasDiaAjustadas ?? plano.gramasDiaSugeridas ?? p.gramasDiaAjustadas ?? p.gramasDiaSugeridas ?? 0;
    return gramasDia > 0 ? this.fmtPeso(gramasDia * dias) : '—';
  }

  adicionarPet(): void {
    this.abrirPet(null);
  }

  abrirPlano(p: Pet, ev: Event): void {
    ev.stopPropagation();
    const ref = this.petDialog.open(PlanoDialogComponent, {
      data: {
        pet: p,
        frequenciaEntregaId: this.form.controls.frequenciaEntregaId.value,
        primeiraEntrega: this.form.controls.primeiraEntrega.value,
      },
      width: '920px',
      maxWidth: '96vw',
      autoFocus: false,
    });
    ref.afterClosed().subscribe((salvou) => {
      if (salvou) {
        this.carregarPlanos(this.pets());
      }
    });
  }

  editarPet(p: Pet): void {
    this.abrirPet(p);
  }

  alternarPet(p: Pet, ev: Event): void {
    ev.stopPropagation();
    const acao = p.ativo ? this.petService.inativar(p.id) : this.petService.reativar(p.id);
    acao.subscribe({
      next: () => {
        this.snack.open(p.ativo ? 'Pet inativado.' : 'Pet reativado.', 'OK', { duration: 2000 });
        this.carregarPets();
      },
      error: () => this.erro('Não foi possível alterar a situação do pet.'),
    });
  }

  private abrirPet(p: Pet | null): void {
    if (this.clienteId === null) {
      return;
    }
    const clienteId = this.clienteId;
    const ref = this.petDialog.open(PetDialogComponent, {
      data: { pet: p },
      width: '560px',
      maxWidth: '95vw',
      autoFocus: false,
    });
    ref.afterClosed().subscribe((req: SalvarPetRequest | undefined) => {
      if (!req) {
        return;
      }
      const obs = p ? this.petService.atualizar(p.id, req) : this.petService.criar(clienteId, req);
      obs.subscribe({
        next: () => {
          this.snack.open(p ? 'Pet atualizado.' : 'Pet cadastrado.', 'OK', { duration: 2000 });
          this.carregarPets();
        },
        error: (e: HttpErrorResponse) => this.erro(this.mensagemErro(e)),
      });
    });
  }

  private mensagemErro(e: HttpErrorResponse): string {
    const errors = e.error?.errors as Record<string, string[]> | undefined;
    if (errors) {
      const primeira = Object.values(errors)[0]?.[0];
      if (primeira) {
        return primeira;
      }
    }
    return e.error?.detail ?? 'Não foi possível concluir a operação.';
  }

  private erro(msg: string): void {
    this.snack.open(msg, 'Fechar', { duration: 4000 });
  }
}
