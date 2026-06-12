import { Component, Inject, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { CATEGORIAS_FORNECEDOR, Fornecedor, rotulo, SalvarFornecedorRequest } from './estoque.model';

export interface FornecedorDialogData {
  fornecedor: Fornecedor | null;
}

@Component({
  selector: 'app-fornecedor-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatSlideToggleModule,
  ],
  templateUrl: './fornecedor-dialog.component.html',
  styleUrl: './fornecedor-dialog.component.scss',
})
export class FornecedorDialogComponent {
  private readonly fb = inject(FormBuilder);

  readonly edicao: boolean;
  readonly categorias = CATEGORIAS_FORNECEDOR;
  readonly rotulo = rotulo;

  readonly form = this.fb.nonNullable.group({
    nome: ['', [Validators.required]],
    nomeFantasia: [''],
    documento: [''],
    telefone: [''],
    whatsApp: [''],
    email: ['', [Validators.email]],
    pessoaContato: [''],
    endereco: [''],
    cidade: [''],
    estado: [''],
    categoria: [''],
    observacoes: [''],
    prazoPagamentoDias: [null as number | null],
    formaPagamentoPreferida: [''],
    chavePix: [''],
    dadosBancarios: [''],
    ativo: [true],
  });

  constructor(
    private readonly ref: MatDialogRef<FornecedorDialogComponent, SalvarFornecedorRequest>,
    @Inject(MAT_DIALOG_DATA) readonly data: FornecedorDialogData,
  ) {
    this.edicao = !!data.fornecedor;
    if (data.fornecedor) {
      const f = data.fornecedor;
      this.form.patchValue({
        nome: f.nome,
        nomeFantasia: f.nomeFantasia ?? '',
        documento: f.documento ?? '',
        telefone: f.telefone ?? '',
        whatsApp: f.whatsApp ?? '',
        email: f.email ?? '',
        pessoaContato: f.pessoaContato ?? '',
        endereco: f.endereco ?? '',
        cidade: f.cidade ?? '',
        estado: f.estado ?? '',
        categoria: f.categoria ?? '',
        observacoes: f.observacoes ?? '',
        prazoPagamentoDias: f.prazoPagamentoDias,
        formaPagamentoPreferida: f.formaPagamentoPreferida ?? '',
        chavePix: f.chavePix ?? '',
        dadosBancarios: f.dadosBancarios ?? '',
        ativo: f.ativo,
      });
    }
  }

  salvar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const v = this.form.getRawValue();
    const t = (s: string) => (s.trim() ? s.trim() : null);
    const req: SalvarFornecedorRequest = {
      nome: v.nome.trim(),
      nomeFantasia: t(v.nomeFantasia),
      documento: t(v.documento),
      telefone: t(v.telefone),
      whatsApp: t(v.whatsApp),
      email: t(v.email),
      pessoaContato: t(v.pessoaContato),
      endereco: t(v.endereco),
      cidade: t(v.cidade),
      estado: t(v.estado)?.toUpperCase() ?? null,
      categoria: v.categoria || null,
      observacoes: t(v.observacoes),
      prazoPagamentoDias: v.prazoPagamentoDias != null ? Number(v.prazoPagamentoDias) : null,
      formaPagamentoPreferida: t(v.formaPagamentoPreferida),
      chavePix: t(v.chavePix),
      dadosBancarios: t(v.dadosBancarios),
      ativo: v.ativo,
    };
    this.ref.close(req);
  }

  cancelar(): void {
    this.ref.close();
  }
}
