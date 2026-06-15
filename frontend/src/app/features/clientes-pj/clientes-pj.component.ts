import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ClientePjResumo } from './clientes-pj.model';
import { ClientesPjService } from './clientes-pj.service';
import { ClientePjDialogComponent } from './cliente-pj-dialog.component';
import { PedidosClienteDialogComponent } from './pedidos-cliente-dialog.component';

@Component({
  selector: 'app-clientes-pj',
  standalone: true,
  imports: [
    FormsModule, MatButtonModule, MatIconModule, MatTooltipModule,
    MatFormFieldModule, MatInputModule, MatSelectModule, MatProgressSpinnerModule,
  ],
  templateUrl: './clientes-pj.component.html',
  styleUrl: './clientes-pj.component.scss',
})
export class ClientesPjComponent implements OnInit {
  private readonly service = inject(ClientesPjService);
  private readonly dialog = inject(MatDialog);
  private readonly snack = inject(MatSnackBar);

  readonly clientes = signal<ClientePjResumo[]>([]);
  readonly carregando = signal(false);

  busca = '';
  filtroAtivo = '';

  ngOnInit(): void {
    this.carregar();
  }

  carregar(): void {
    this.carregando.set(true);
    const ativo = this.filtroAtivo === '' ? null : this.filtroAtivo === 'true';
    this.service.listar(this.busca.trim() || undefined, ativo).subscribe({
      next: (cs) => { this.clientes.set(cs); this.carregando.set(false); },
      error: () => this.carregando.set(false),
    });
  }

  rotuloTipo(t: string): string {
    switch (t) {
      case 'PetShop': return 'Pet shop';
      case 'ClinicaVeterinaria': return 'Clínica veterinária';
      default: return t;
    }
  }

  novo(): void {
    const ref = this.dialog.open(ClientePjDialogComponent, { data: {}, autoFocus: false, maxWidth: '94vw' });
    ref.afterClosed().subscribe((c) => { if (c) { this.snack.open('Cliente PJ criado.', 'OK', { duration: 2500 }); this.carregar(); } });
  }

  editar(c: ClientePjResumo): void {
    this.service.obter(c.id).subscribe((cliente) => {
      const ref = this.dialog.open(ClientePjDialogComponent, { data: { cliente }, autoFocus: false, maxWidth: '94vw' });
      ref.afterClosed().subscribe((res) => { if (res) { this.snack.open('Cliente PJ atualizado.', 'OK', { duration: 2500 }); this.carregar(); } });
    });
  }

  pedidos(c: ClientePjResumo): void {
    this.dialog.open(PedidosClienteDialogComponent, { data: { clienteId: c.id, nomeFantasia: c.nomeFantasia }, autoFocus: false, maxWidth: '94vw' });
  }

  alternarStatus(c: ClientePjResumo): void {
    this.service.alternarStatus(c.id, !c.ativo).subscribe({
      next: () => { this.snack.open(c.ativo ? 'Cliente inativado.' : 'Cliente ativado.', 'OK', { duration: 2500 }); this.carregar(); },
      error: () => this.snack.open('Não foi possível alterar o status.', 'OK', { duration: 3000 }),
    });
  }
}
