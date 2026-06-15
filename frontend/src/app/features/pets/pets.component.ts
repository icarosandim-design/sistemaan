import { Component, Inject, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MAT_DIALOG_DATA, MatDialog, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { PetResumo, SalvarPetRequest, labelSexo } from '../clientes/pets/pet.model';
import { PetService } from '../clientes/pets/pet.service';
import { PetDialogComponent } from '../clientes/pets/pet-dialog.component';
import { ClientesService } from '../clientes/clientes.service';
import { Cliente } from '../clientes/clientes.model';

// ---- Diálogo simples para escolher o tutor (Cliente PF) ao criar um pet ----
@Component({
  selector: 'app-selecionar-tutor-dialog',
  standalone: true,
  imports: [FormsModule, MatDialogModule, MatFormFieldModule, MatSelectModule, MatButtonModule, MatIconModule],
  template: `
    <h2 mat-dialog-title>Novo pet — escolher tutor</h2>
    <mat-dialog-content>
      <mat-form-field appearance="outline" class="full">
        <mat-label>Cliente (tutor)</mat-label>
        <mat-select [(ngModel)]="clienteId">
          @for (c of clientes; track c.id) { <mat-option [value]="c.id">{{ c.nome }}</mat-option> }
        </mat-select>
      </mat-form-field>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="ref.close()">Cancelar</button>
      <button mat-flat-button class="btn-cta" [disabled]="!clienteId" (click)="ref.close(clienteId)"><mat-icon>arrow_forward</mat-icon> Continuar</button>
    </mat-dialog-actions>
  `,
  styles: [`.full { width: 100%; min-width: 360px; } .btn-cta { background: var(--an-cta); color: #fff; }`],
})
export class SelecionarTutorDialogComponent {
  clienteId: number | null = null;
  constructor(
    readonly ref: MatDialogRef<SelecionarTutorDialogComponent, number | null>,
    @Inject(MAT_DIALOG_DATA) readonly clientes: Cliente[],
  ) {}
}

@Component({
  selector: 'app-pets',
  standalone: true,
  imports: [
    FormsModule, MatButtonModule, MatIconModule, MatTooltipModule,
    MatFormFieldModule, MatInputModule, MatSelectModule, MatProgressSpinnerModule,
  ],
  templateUrl: './pets.component.html',
  styleUrl: './pets.component.scss',
})
export class PetsComponent implements OnInit {
  private readonly petService = inject(PetService);
  private readonly clientesService = inject(ClientesService);
  private readonly dialog = inject(MatDialog);
  private readonly snack = inject(MatSnackBar);
  readonly labelSexo = labelSexo;

  readonly pets = signal<PetResumo[]>([]);
  readonly carregando = signal(false);

  busca = '';
  filtroAtivo = 'true';
  filtroTipo = '';

  ngOnInit(): void {
    this.carregar();
  }

  carregar(): void {
    this.carregando.set(true);
    const ativo = this.filtroAtivo === '' ? null : this.filtroAtivo === 'true';
    this.petService.listarTodos(this.busca.trim() || undefined, ativo, this.filtroTipo || undefined).subscribe({
      next: (ps) => { this.pets.set(ps); this.carregando.set(false); },
      error: () => this.carregando.set(false),
    });
  }

  fmtData(iso: string | null): string {
    if (!iso) return '—';
    const [y, m, d] = iso.split('-');
    return `${d}/${m}/${y}`;
  }

  editar(p: PetResumo): void {
    this.petService.obter(p.id).subscribe((pet) => {
      const ref = this.dialog.open(PetDialogComponent, { data: { pet }, width: '560px', maxWidth: '94vw', autoFocus: false });
      ref.afterClosed().subscribe((req: SalvarPetRequest | undefined) => {
        if (!req) return;
        this.petService.atualizar(p.id, req).subscribe({
          next: () => { this.snack.open('Pet atualizado.', 'OK', { duration: 2500 }); this.carregar(); },
          error: () => this.snack.open('Não foi possível salvar o pet.', 'OK', { duration: 3000 }),
        });
      });
    });
  }

  novo(): void {
    this.clientesService.listar().subscribe((clientes) => {
      const sel = this.dialog.open(SelecionarTutorDialogComponent, { data: clientes, autoFocus: false });
      sel.afterClosed().subscribe((clienteId) => {
        if (!clienteId) return;
        const ref = this.dialog.open(PetDialogComponent, { data: { pet: null }, width: '560px', maxWidth: '94vw', autoFocus: false });
        ref.afterClosed().subscribe((req: SalvarPetRequest | undefined) => {
          if (!req) return;
          this.petService.criar(clienteId, req).subscribe({
            next: () => { this.snack.open('Pet criado.', 'OK', { duration: 2500 }); this.carregar(); },
            error: () => this.snack.open('Não foi possível criar o pet.', 'OK', { duration: 3000 }),
          });
        });
      });
    });
  }

  alternarStatus(p: PetResumo): void {
    const op = p.ativo ? this.petService.inativar(p.id) : this.petService.reativar(p.id);
    op.subscribe({
      next: () => { this.snack.open(p.ativo ? 'Pet inativado.' : 'Pet ativado.', 'OK', { duration: 2500 }); this.carregar(); },
      error: () => this.snack.open('Não foi possível alterar o status.', 'OK', { duration: 3000 }),
    });
  }
}
