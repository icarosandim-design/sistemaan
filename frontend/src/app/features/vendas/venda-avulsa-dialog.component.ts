import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatDialog, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';
import { EstoqueService } from '../estoque/estoque.service';
import { OpcaoSimples } from '../estoque/estoque.model';
import { ClientesService } from '../clientes/clientes.service';
import { Cliente, FORMAS_PAGAMENTO, SalvarClienteRequest } from '../clientes/clientes.model';
import { ClienteDialogComponent } from '../clientes/cliente-dialog.component';
import { PetService } from '../clientes/pets/pet.service';
import { Pet, SalvarPetRequest } from '../clientes/pets/pet.model';
import { PetDialogComponent } from '../clientes/pets/pet-dialog.component';
import { VendaAvulsaService } from './venda-avulsa.service';
import { SalvarVendaAvulsaRequest, VendaAvulsaResultado } from './venda-avulsa.model';

function msgErro(e: unknown): string {
  const err = (e as { error?: { errors?: Record<string, string[]>; detail?: string } })?.error;
  const first = err?.errors ? Object.values(err.errors)[0]?.[0] : undefined;
  return first ?? err?.detail ?? 'Não foi possível registrar a venda.';
}

interface LinhaItem {
  receitaId: number | null;
  tamanhoPacoteId: number | null;
  quantidade: number | null;
  observacao: string | null;
}

@Component({
  selector: 'app-venda-avulsa-dialog',
  standalone: true,
  imports: [
    FormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
  ],
  template: `
    <h2 mat-dialog-title>Venda avulsa <span class="pf">PF</span></h2>
    <mat-dialog-content>
      <p class="dica">Venda única para cliente pessoa física, sem assinatura. Gera uma entrega.</p>

      <!-- Cliente -->
      <div class="linha-cliente">
        <mat-form-field appearance="outline" class="cresce">
          <mat-label>Cliente PF</mat-label>
          <mat-select [(ngModel)]="clienteId" (selectionChange)="aoTrocarCliente()">
            @for (c of clientes(); track c.id) {
              <mat-option [value]="c.id">{{ c.nome }}</mat-option>
            }
          </mat-select>
        </mat-form-field>
        <button mat-stroked-button type="button" (click)="novoCliente()">
          <mat-icon>person_add</mat-icon> Novo
        </button>
      </div>

      <!-- Pet -->
      <div class="linha-cliente">
        <mat-form-field appearance="outline" class="cresce">
          <mat-label>Pet</mat-label>
          <mat-select [(ngModel)]="petId" [disabled]="!clienteId">
            @for (p of pets(); track p.id) {
              <mat-option [value]="p.id">{{ p.nome }}</mat-option>
            }
          </mat-select>
          @if (clienteId && !pets().length) {
            <mat-hint>Este cliente ainda não tem pet. Cadastre um.</mat-hint>
          }
        </mat-form-field>
        <button mat-stroked-button type="button" [disabled]="!clienteId" (click)="novoPet()">
          <mat-icon>pets</mat-icon> Novo
        </button>
      </div>

      <!-- Itens -->
      <h3 class="sec">Itens (Receita da Casa)</h3>
      @for (l of itens; track $index) {
        <div class="linha-item">
          <mat-form-field appearance="outline" class="f-receita">
            <mat-label>Receita</mat-label>
            <mat-select [(ngModel)]="l.receitaId">
              @for (r of receitas(); track r.id) {
                <mat-option [value]="r.id">{{ r.nome }}</mat-option>
              }
            </mat-select>
          </mat-form-field>
          <mat-form-field appearance="outline" class="f-tam">
            <mat-label>Tamanho</mat-label>
            <mat-select [(ngModel)]="l.tamanhoPacoteId">
              @for (t of tamanhos(); track t.id) {
                <mat-option [value]="t.id">{{ t.nome }}</mat-option>
              }
            </mat-select>
          </mat-form-field>
          <mat-form-field appearance="outline" class="f-qtd">
            <mat-label>Qtd</mat-label>
            <input matInput type="number" min="1" [(ngModel)]="l.quantidade" />
          </mat-form-field>
          <button mat-icon-button type="button" (click)="removerLinha($index)" matTooltip="Remover">
            <mat-icon>delete</mat-icon>
          </button>
        </div>
      }
      <button mat-button type="button" class="add" (click)="adicionarLinha()">
        <mat-icon>add</mat-icon> Adicionar item
      </button>

      <!-- Entrega / pagamento -->
      <div class="linha-grid">
        <mat-form-field appearance="outline">
          <mat-label>Data de entrega</mat-label>
          <input matInput type="date" [(ngModel)]="dataEntrega" />
        </mat-form-field>
        <mat-form-field appearance="outline">
          <mat-label>Valor (R$)</mat-label>
          <input matInput type="number" min="0" step="0.01" [(ngModel)]="valor" />
        </mat-form-field>
        <mat-form-field appearance="outline">
          <mat-label>Forma de pagamento</mat-label>
          <mat-select [(ngModel)]="formaPagamento">
            <mat-option [value]="null">—</mat-option>
            @for (f of formas; track f.valor) {
              <mat-option [value]="f.valor">{{ f.label }}</mat-option>
            }
          </mat-select>
        </mat-form-field>
      </div>

      <mat-form-field appearance="outline" class="full">
        <mat-label>Observações</mat-label>
        <textarea matInput rows="2" [(ngModel)]="observacoes"></textarea>
      </mat-form-field>

      @if (erro()) {
        <p class="erro"><mat-icon>error</mat-icon> {{ erro() }}</p>
      }
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="ref.close()">Cancelar</button>
      <button mat-flat-button class="btn-cta" [disabled]="salvando()" (click)="salvar()">
        {{ salvando() ? 'Registrando...' : 'Registrar venda' }}
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    .pf { font-size: 0.7rem; font-weight: 700; padding: 0.1rem 0.45rem; border-radius: 999px; background: rgba(168,177,126,0.3); color: var(--an-primaria); vertical-align: middle; }
    .dica { margin: 0 0 1rem; font-size: 0.85rem; color: var(--an-texto-secundario); }
    .sec { margin: 1rem 0 0.5rem; font-size: 0.9rem; font-weight: 700; }
    .linha-cliente { display: flex; align-items: flex-start; gap: 0.5rem; }
    .linha-cliente .cresce { flex: 1; }
    .linha-cliente button { margin-top: 0.35rem; }
    .linha-item { display: flex; align-items: center; gap: 0.5rem; }
    .f-receita { flex: 2; min-width: 160px; }
    .f-tam { flex: 1; min-width: 90px; }
    .f-qtd { width: 80px; }
    .add { margin: 0 0 0.5rem; color: var(--an-cta-hover); font-weight: 600; }
    .linha-grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 0.5rem; }
    .full { width: 100%; }
    .btn-cta { background: var(--an-cta); color: #fff; }
    .erro { display: flex; align-items: center; gap: 0.4rem; color: #b3261e; font-size: 0.9rem; }
    mat-dialog-content { min-width: min(620px, 92vw); }
    @media (max-width: 600px) { .linha-grid { grid-template-columns: 1fr; } }
  `],
})
export class VendaAvulsaDialogComponent implements OnInit {
  private readonly clientesService = inject(ClientesService);
  private readonly petService = inject(PetService);
  private readonly estoque = inject(EstoqueService);
  private readonly service = inject(VendaAvulsaService);
  private readonly dialog = inject(MatDialog);
  private readonly snack = inject(MatSnackBar);
  readonly ref = inject(MatDialogRef<VendaAvulsaDialogComponent, VendaAvulsaResultado>);

  readonly clientes = signal<Cliente[]>([]);
  readonly pets = signal<Pet[]>([]);
  readonly receitas = signal<OpcaoSimples[]>([]);
  readonly tamanhos = signal<OpcaoSimples[]>([]);
  readonly salvando = signal(false);
  readonly erro = signal<string | null>(null);
  readonly formas = FORMAS_PAGAMENTO;

  clienteId: number | null = null;
  petId: number | null = null;
  dataEntrega = '';
  valor: number | null = null;
  formaPagamento: string | null = null;
  observacoes = '';
  itens: LinhaItem[] = [{ receitaId: null, tamanhoPacoteId: null, quantidade: null, observacao: null }];

  ngOnInit(): void {
    this.clientesService.listar().subscribe((cs) => this.clientes.set(cs.filter((c) => c.ativo)));
    this.estoque.listarReceitasCasa().subscribe((r) => this.receitas.set(r));
    this.estoque.listarTamanhos().subscribe((t) => this.tamanhos.set(t));
  }

  aoTrocarCliente(): void {
    this.petId = null;
    this.pets.set([]);
    if (this.clienteId) {
      this.petService.listarPorCliente(this.clienteId).subscribe((ps) => this.pets.set(ps.filter((p) => p.ativo)));
    }
  }

  novoCliente(): void {
    const ref = this.dialog.open(ClienteDialogComponent, { data: { cliente: null }, width: '680px', maxWidth: '96vw', autoFocus: false });
    ref.afterClosed().subscribe((req: SalvarClienteRequest | undefined) => {
      if (!req) {
        return;
      }
      this.clientesService.criar(req).subscribe({
        next: (c) => {
          this.clientes.update((xs) => [...xs, c].sort((a, b) => a.nome.localeCompare(b.nome)));
          this.clienteId = c.id;
          this.aoTrocarCliente();
          this.snack.open('Cliente criado.', 'OK', { duration: 2500 });
        },
        error: (e) => this.snack.open(msgErro(e), 'Fechar', { duration: 4000 }),
      });
    });
  }

  novoPet(): void {
    if (!this.clienteId) {
      return;
    }
    const clienteId = this.clienteId;
    const ref = this.dialog.open(PetDialogComponent, { data: { pet: null }, width: '560px', maxWidth: '95vw', autoFocus: false });
    ref.afterClosed().subscribe((req: SalvarPetRequest | undefined) => {
      if (!req) {
        return;
      }
      this.petService.criar(clienteId, req).subscribe({
        next: (p) => {
          this.pets.update((xs) => [...xs, p]);
          this.petId = p.id;
          this.snack.open('Pet cadastrado.', 'OK', { duration: 2500 });
        },
        error: (e) => this.snack.open(msgErro(e), 'Fechar', { duration: 4000 }),
      });
    });
  }

  adicionarLinha(): void {
    this.itens = [...this.itens, { receitaId: null, tamanhoPacoteId: null, quantidade: null, observacao: null }];
  }

  removerLinha(i: number): void {
    this.itens = this.itens.filter((_, idx) => idx !== i);
    if (!this.itens.length) {
      this.adicionarLinha();
    }
  }

  salvar(): void {
    this.erro.set(null);
    if (!this.clienteId) {
      this.erro.set('Selecione o cliente PF.');
      return;
    }
    if (!this.petId) {
      this.erro.set('Selecione (ou cadastre) o pet.');
      return;
    }
    if (!this.dataEntrega) {
      this.erro.set('Informe a data de entrega.');
      return;
    }
    const itens = this.itens
      .filter((l) => l.receitaId && l.tamanhoPacoteId && (l.quantidade ?? 0) > 0)
      .map((l) => ({ receitaId: l.receitaId!, tamanhoPacoteId: l.tamanhoPacoteId!, quantidade: l.quantidade!, observacao: l.observacao }));
    if (!itens.length) {
      this.erro.set('Inclua ao menos um item (receita + tamanho + quantidade).');
      return;
    }

    const req: SalvarVendaAvulsaRequest = {
      clienteId: this.clienteId,
      petId: this.petId,
      dataEntrega: this.dataEntrega,
      observacoes: this.observacoes.trim() || null,
      valor: this.valor ?? null,
      formaPagamento: this.formaPagamento,
      itens,
    };
    this.salvando.set(true);
    this.service.criar(req).subscribe({
      next: (r) => {
        this.snack.open('Venda avulsa registrada — entrega gerada.', 'OK', { duration: 3500 });
        this.ref.close(r);
      },
      error: (e) => {
        this.salvando.set(false);
        this.erro.set(msgErro(e));
      },
    });
  }
}
