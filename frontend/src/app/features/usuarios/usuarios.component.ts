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
import { PerfilOpcao, Usuario } from './usuarios.model';
import { UsuariosService } from './usuarios.service';
import { RedefinirSenhaDialogComponent, UsuarioDialogComponent } from './usuario-dialogs.component';

@Component({
  selector: 'app-usuarios',
  standalone: true,
  imports: [
    FormsModule, MatButtonModule, MatIconModule, MatTooltipModule,
    MatFormFieldModule, MatInputModule, MatSelectModule, MatProgressSpinnerModule,
  ],
  templateUrl: './usuarios.component.html',
  styleUrl: './usuarios.component.scss',
})
export class UsuariosComponent implements OnInit {
  private readonly service = inject(UsuariosService);
  private readonly dialog = inject(MatDialog);
  private readonly snack = inject(MatSnackBar);

  readonly usuarios = signal<Usuario[]>([]);
  readonly perfis = signal<PerfilOpcao[]>([]);
  readonly carregando = signal(false);

  busca = '';
  filtroPerfil = '';
  filtroAtivo = '';

  ngOnInit(): void {
    this.service.perfis().subscribe((p) => this.perfis.set(p));
    this.carregar();
  }

  carregar(): void {
    this.carregando.set(true);
    const ativo = this.filtroAtivo === '' ? null : this.filtroAtivo === 'true';
    this.service.listar(this.busca.trim() || undefined, this.filtroPerfil || undefined, ativo).subscribe({
      next: (us) => {
        this.usuarios.set(us);
        this.carregando.set(false);
      },
      error: () => this.carregando.set(false),
    });
  }

  fmtData(iso: string | null): string {
    if (!iso) return '—';
    const d = new Date(iso);
    return d.toLocaleDateString('pt-BR');
  }

  novo(): void {
    const ref = this.dialog.open(UsuarioDialogComponent, { data: { perfis: this.perfis() }, autoFocus: false });
    ref.afterClosed().subscribe((u) => { if (u) { this.snack.open('Usuário criado.', 'OK', { duration: 2500 }); this.carregar(); } });
  }

  editar(usuario: Usuario): void {
    const ref = this.dialog.open(UsuarioDialogComponent, { data: { usuario, perfis: this.perfis() }, autoFocus: false });
    ref.afterClosed().subscribe((u) => { if (u) { this.snack.open('Usuário atualizado.', 'OK', { duration: 2500 }); this.carregar(); } });
  }

  alternarStatus(usuario: Usuario): void {
    this.service.alternarStatus(usuario.id, !usuario.ativo).subscribe({
      next: () => { this.snack.open(usuario.ativo ? 'Usuário inativado.' : 'Usuário ativado.', 'OK', { duration: 2500 }); this.carregar(); },
      error: (e) => this.snack.open(this.erro(e), 'OK', { duration: 4000 }),
    });
  }

  redefinirSenha(usuario: Usuario): void {
    const ref = this.dialog.open(RedefinirSenhaDialogComponent, { data: { usuario }, autoFocus: false });
    ref.afterClosed().subscribe((ok) => { if (ok) this.snack.open('Senha redefinida.', 'OK', { duration: 2500 }); });
  }

  private erro(e: unknown): string {
    const err = (e as { error?: { errors?: Record<string, string[]>; detail?: string } })?.error;
    const first = err?.errors ? Object.values(err.errors)[0]?.[0] : undefined;
    return first ?? err?.detail ?? 'Operação não permitida.';
  }
}
