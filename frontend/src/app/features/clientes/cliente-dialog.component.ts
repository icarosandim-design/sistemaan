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
import { labelSexo, MOCK_PETS, Pet, sugestaoGramasDia } from './pets/pet.model';
import { PetDialogComponent } from './pets/pet-dialog.component';
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
  ],
  templateUrl: './cliente-dialog.component.html',
  styleUrl: './cliente-dialog.component.scss',
})
export class ClienteDialogComponent {
  private readonly fb = inject(FormBuilder);
  private readonly petDialog = inject(MatDialog);

  // Pets (mock nesta fase — não persistidos)
  readonly pets = signal<Pet[]>(MOCK_PETS.map((p) => ({ ...p })));
  readonly labelSexo = labelSexo;
  readonly sugestaoGramasDia = sugestaoGramasDia;

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
    if (data.cliente) {
      const c = data.cliente;
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

  // ===== Pets (mock) =====
  adicionarPet(): void {
    this.abrirPet(null);
  }

  editarPet(p: Pet): void {
    this.abrirPet(p);
  }

  alternarPet(p: Pet, ev: Event): void {
    ev.stopPropagation();
    this.pets.update((lista) => lista.map((x) => (x.id === p.id ? { ...x, ativo: !x.ativo } : x)));
  }

  private abrirPet(p: Pet | null): void {
    const ref = this.petDialog.open(PetDialogComponent, {
      data: { pet: p },
      width: '560px',
      maxWidth: '95vw',
      autoFocus: false,
    });
    ref.afterClosed().subscribe((res: Pet | undefined) => {
      if (!res) {
        return;
      }
      this.pets.update((lista) => {
        const idx = lista.findIndex((x) => x.id === res.id);
        if (idx >= 0) {
          const copia = [...lista];
          copia[idx] = res;
          return copia;
        }
        return [...lista, res];
      });
    });
  }
}
