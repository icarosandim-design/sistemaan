import { Component, Inject, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatTabsModule } from '@angular/material/tabs';
import {
  Cliente,
  FORMAS_PAGAMENTO,
  SalvarClienteRequest,
  STATUS_FINANCEIRO,
  TIPOS_CLIENTE,
} from './clientes.model';

export interface ClienteDialogData {
  cliente: Cliente | null;
}

@Component({
  selector: 'app-cliente-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatSlideToggleModule,
    MatTabsModule,
  ],
  templateUrl: './cliente-dialog.component.html',
  styleUrl: './cliente-dialog.component.scss',
})
export class ClienteDialogComponent {
  private readonly fb = inject(FormBuilder);

  readonly tipos = TIPOS_CLIENTE;
  readonly formas = FORMAS_PAGAMENTO;
  readonly statusFin = STATUS_FINANCEIRO;
  readonly edicao: boolean;

  readonly form = this.fb.nonNullable.group({
    nome: ['', [Validators.required]],
    telefone: [''],
    email: ['', [Validators.email]],
    endereco: [''],
    bairro: [''],
    cidade: [''],
    observacoes: [''],
    ativo: [true],
    tipoCliente: ['Assinante' as const],
    formaPagamento: ['Pix' as string | null],
    diaCobranca: [null as number | null, [Validators.min(1), Validators.max(31)]],
    valorRecorrenteMensal: [0, [Validators.min(0)]],
    statusFinanceiro: ['EmDia' as const],
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
        telefone: c.telefone ?? '',
        email: c.email ?? '',
        endereco: c.endereco ?? '',
        bairro: c.bairro ?? '',
        cidade: c.cidade ?? '',
        observacoes: c.observacoes ?? '',
        ativo: c.ativo,
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
    const req: SalvarClienteRequest = {
      nome: v.nome.trim(),
      telefone: v.telefone.trim() || null,
      email: v.email.trim() || null,
      endereco: v.endereco.trim() || null,
      bairro: v.bairro.trim() || null,
      cidade: v.cidade.trim() || null,
      observacoes: v.observacoes.trim() || null,
      ativo: v.ativo,
      tipoCliente: v.tipoCliente,
      formaPagamento: v.formaPagamento || null,
      diaCobranca: v.diaCobranca ?? null,
      valorRecorrenteMensal: Number(v.valorRecorrenteMensal) || 0,
      statusFinanceiro: v.statusFinanceiro,
      observacoesFinanceiras: v.observacoesFinanceiras.trim() || null,
    };
    this.ref.close(req);
  }

  cancelar(): void {
    this.ref.close();
  }
}
