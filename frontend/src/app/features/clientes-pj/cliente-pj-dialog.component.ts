import { Component, Inject, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { ClientePj, SalvarClientePjRequest, TipoPjOpcao } from './clientes-pj.model';
import { ClientesPjService } from './clientes-pj.service';
import { PREFERENCIAS_HORARIO } from '../clientes/clientes.model';

function msgErro(e: unknown): string {
  const err = (e as { error?: { errors?: Record<string, string[]>; detail?: string } })?.error;
  const first = err?.errors ? Object.values(err.errors)[0]?.[0] : undefined;
  return first ?? err?.detail ?? 'Não foi possível salvar.';
}

@Component({
  selector: 'app-cliente-pj-dialog',
  standalone: true,
  imports: [FormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatButtonModule, MatIconModule],
  templateUrl: './cliente-pj-dialog.component.html',
  styleUrl: './cliente-pj-dialog.component.scss',
})
export class ClientePjDialogComponent implements OnInit {
  private readonly service = inject(ClientesPjService);
  readonly edicao: boolean;
  readonly tipos = signal<TipoPjOpcao[]>([]);
  readonly preferencias = PREFERENCIAS_HORARIO;
  readonly salvando = signal(false);
  readonly erro = signal<string | null>(null);

  readonly f: SalvarClientePjRequest = {
    razaoSocial: '', nomeFantasia: '', cnpj: '', inscricaoEstadual: null,
    telefone: null, whatsapp: null, email: null, pessoaContato: null, cargoContato: null,
    rua: null, numero: null, complemento: null, bairro: null, cidade: null, estado: null, cep: null,
    entregaRua: null, entregaNumero: null, entregaComplemento: null, entregaBairro: null,
    entregaCidade: null, entregaEstado: null, entregaCep: null,
    tipoPJ: 'Mercado', condicaoComercial: null, prazoPagamento: null, diaEntregaPreferencial: null,
    frequenciaCompra: null, observacoes: null, observacoesComerciais: null,
    preferenciaHorario: 'HorarioComercial',
  };

  constructor(
    readonly ref: MatDialogRef<ClientePjDialogComponent, ClientePj>,
    @Inject(MAT_DIALOG_DATA) readonly data: { cliente?: ClientePj },
  ) {
    this.edicao = !!data.cliente;
    if (data.cliente) {
      Object.assign(this.f, data.cliente);
    }
  }

  ngOnInit(): void {
    this.service.tipos().subscribe((t) => this.tipos.set(t));
  }

  copiarEnderecoComercial(): void {
    this.f.entregaRua = this.f.rua;
    this.f.entregaNumero = this.f.numero;
    this.f.entregaComplemento = this.f.complemento;
    this.f.entregaBairro = this.f.bairro;
    this.f.entregaCidade = this.f.cidade;
    this.f.entregaEstado = this.f.estado;
    this.f.entregaCep = this.f.cep;
  }

  salvar(): void {
    this.salvando.set(true);
    this.erro.set(null);
    const obs = this.edicao ? this.service.atualizar(this.data.cliente!.id, this.f) : this.service.criar(this.f);
    obs.subscribe({
      next: (c) => this.ref.close(c),
      error: (e) => { this.salvando.set(false); this.erro.set(msgErro(e)); },
    });
  }
}
