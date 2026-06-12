import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { MatTableModule } from '@angular/material/table';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { FrequenciaEntrega, SalvarFrequenciaRequest } from './frequencias.model';
import { FrequenciasService } from './frequencias.service';
import { FrequenciaDialogComponent } from './frequencia-dialog.component';

@Component({
  selector: 'app-frequencias',
  standalone: true,
  imports: [
    FormsModule,
    MatTableModule,
    MatFormFieldModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatTooltipModule,
    MatProgressBarModule,
  ],
  templateUrl: './frequencias.component.html',
  styleUrl: './frequencias.component.scss',
})
export class FrequenciasComponent implements OnInit {
  private readonly service = inject(FrequenciasService);
  private readonly dialog = inject(MatDialog);
  private readonly snack = inject(MatSnackBar);

  readonly displayedColumns = ['nome', 'ciclo', 'descricao', 'ativo', 'acoes'];
  todas: FrequenciaEntrega[] = [];
  carregando = false;
  filtroStatus = '';

  ngOnInit(): void {
    this.carregar();
  }

  get lista(): FrequenciaEntrega[] {
    if (!this.filtroStatus) {
      return this.todas;
    }
    return this.todas.filter((f) => (this.filtroStatus === 'ativo' ? f.ativo : !f.ativo));
  }

  carregar(): void {
    this.carregando = true;
    this.service.listar().subscribe({
      next: (fs) => {
        this.todas = fs;
        this.carregando = false;
      },
      error: () => {
        this.carregando = false;
        this.erro('Falha ao carregar frequências.');
      },
    });
  }

  novo(): void {
    this.abrir(null);
  }

  editar(f: FrequenciaEntrega): void {
    this.abrir(f);
  }

  alternarStatus(f: FrequenciaEntrega, ev: Event): void {
    ev.stopPropagation();
    this.service.alternarStatus(f.id, !f.ativo).subscribe({
      next: () => {
        this.snack.open(f.ativo ? 'Frequência inativada.' : 'Frequência ativada.', 'OK', { duration: 2500 });
        this.carregar();
      },
      error: (e: HttpErrorResponse) => this.erro(this.mensagemErro(e)),
    });
  }

  private abrir(f: FrequenciaEntrega | null): void {
    const ref = this.dialog.open(FrequenciaDialogComponent, {
      data: { frequencia: f },
      width: '480px',
      maxWidth: '95vw',
      autoFocus: false,
    });
    ref.afterClosed().subscribe((req: SalvarFrequenciaRequest | undefined) => {
      if (!req) {
        return;
      }
      const obs = f ? this.service.atualizar(f.id, req) : this.service.criar(req);
      obs.subscribe({
        next: () => {
          this.snack.open(f ? 'Frequência atualizada.' : 'Frequência criada.', 'OK', { duration: 2500 });
          this.carregar();
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
    return e.error?.detail ?? 'Não foi possível salvar a frequência.';
  }

  private erro(msg: string): void {
    this.snack.open(msg, 'Fechar', { duration: 4000 });
  }
}
