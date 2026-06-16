import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog } from '@angular/material/dialog';
import { ActivatedRoute } from '@angular/router';
import { EntregaDisponivel, RotaResumo, rotuloPeriodo, rotuloStatusRota } from './rotas.model';
import { RotasService } from './rotas.service';
import { RotaDetalheDialogComponent, RotaFormDialogComponent } from './rota-dialogs.component';
import { rotuloPreferenciaHorario } from '../clientes/clientes.model';

@Component({
  selector: 'app-planejar-rotas',
  standalone: true,
  imports: [FormsModule, MatButtonModule, MatIconModule, MatTooltipModule, MatProgressSpinnerModule],
  templateUrl: './planejar-rotas.component.html',
  styleUrl: './planejar-rotas.component.scss',
})
export class PlanejarRotasComponent implements OnInit {
  private readonly service = inject(RotasService);
  private readonly dialog = inject(MatDialog);
  private readonly route = inject(ActivatedRoute);

  readonly rotas = signal<RotaResumo[]>([]);
  readonly disponiveis = signal<EntregaDisponivel[]>([]);
  readonly carregando = signal(false);

  readonly rotuloPeriodo = rotuloPeriodo;
  readonly rotuloStatusRota = rotuloStatusRota;
  readonly rotuloPreferencia = rotuloPreferenciaHorario;

  data = new Date().toISOString().slice(0, 10);

  ngOnInit(): void {
    const data = this.route.snapshot.queryParamMap.get('data');
    if (data) {
      this.data = data;
    }
    this.carregar();
  }

  carregar(): void {
    if (!this.data) return;
    this.carregando.set(true);
    this.service.listar(this.data).subscribe({
      next: (rs) => { this.rotas.set(rs); this.carregando.set(false); },
      error: () => this.carregando.set(false),
    });
    this.service.disponiveis(this.data).subscribe((d) => this.disponiveis.set(d));
  }

  fmtData(iso: string): string {
    const [y, m, d] = iso.split('-');
    return `${d}/${m}/${y}`;
  }

  novaSaida(): void {
    const ref = this.dialog.open(RotaFormDialogComponent, { data: { data: this.data }, autoFocus: false });
    ref.afterClosed().subscribe((r) => { if (r) { this.carregar(); this.abrir(r.id); } });
  }

  abrir(id: number): void {
    const ref = this.dialog.open(RotaDetalheDialogComponent, { data: { rotaId: id }, autoFocus: false, maxWidth: '96vw' });
    ref.afterClosed().subscribe(() => this.carregar());
  }
}
