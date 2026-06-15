import { Component, Inject, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { PerfilOpcao, Usuario } from './usuarios.model';
import { UsuariosService } from './usuarios.service';

function msgErro(e: unknown): string {
  const err = (e as { error?: { errors?: Record<string, string[]>; detail?: string } })?.error;
  const first = err?.errors ? Object.values(err.errors)[0]?.[0] : undefined;
  return first ?? err?.detail ?? 'Não foi possível salvar.';
}

// ===================== Criar / Editar usuário =====================
@Component({
  selector: 'app-usuario-dialog',
  standalone: true,
  imports: [FormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatButtonModule, MatIconModule],
  template: `
    <h2 mat-dialog-title>{{ edicao ? 'Editar usuário' : 'Novo usuário' }}</h2>
    <mat-dialog-content>
      <mat-form-field appearance="outline" class="full">
        <mat-label>Nome</mat-label>
        <input matInput [(ngModel)]="nome" />
      </mat-form-field>
      <mat-form-field appearance="outline" class="full">
        <mat-label>E-mail (login)</mat-label>
        <input matInput type="email" [(ngModel)]="email" />
      </mat-form-field>
      <mat-form-field appearance="outline" class="full">
        <mat-label>Confirmar e-mail</mat-label>
        <input matInput type="email" [(ngModel)]="confirmarEmail" (paste)="$event.preventDefault()" />
      </mat-form-field>
      @if (!edicao) {
        <mat-form-field appearance="outline" class="full">
          <mat-label>Senha</mat-label>
          <input matInput type="password" [(ngModel)]="senha" />
          <mat-hint>Mínimo 6 caracteres</mat-hint>
        </mat-form-field>
        <mat-form-field appearance="outline" class="full">
          <mat-label>Confirmar senha</mat-label>
          <input matInput type="password" [(ngModel)]="confirmarSenha" (paste)="$event.preventDefault()" />
        </mat-form-field>
      }
      <mat-form-field appearance="outline" class="full">
        <mat-label>Perfil</mat-label>
        <mat-select [(ngModel)]="perfil">
          @for (p of data.perfis; track p.nome) {
            <mat-option [value]="p.nome">{{ p.nome }} — {{ p.descricao }}</mat-option>
          }
        </mat-select>
      </mat-form-field>
      <mat-form-field appearance="outline" class="full">
        <mat-label>Telefone (opcional)</mat-label>
        <input matInput [(ngModel)]="telefone" />
      </mat-form-field>
      <mat-form-field appearance="outline" class="full">
        <mat-label>Observações (opcional)</mat-label>
        <textarea matInput rows="2" [(ngModel)]="observacoes"></textarea>
      </mat-form-field>
      @if (erro()) { <p class="erro"><mat-icon>error</mat-icon> {{ erro() }}</p> }
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="ref.close()">Cancelar</button>
      <button mat-flat-button class="btn-cta" [disabled]="salvando()" (click)="salvar()"><mat-icon>check</mat-icon> Salvar</button>
    </mat-dialog-actions>
  `,
  styles: [`
    .full { width: 100%; }
    .erro { display: flex; align-items: center; gap: 0.4rem; color: #b3261e; font-size: 0.85rem; margin: 0; }
    .erro .mat-icon { font-size: 1.05rem; width: 1.05rem; height: 1.05rem; }
    .btn-cta { background: var(--an-cta); color: #fff; }
    mat-dialog-content { min-width: 420px; display: flex; flex-direction: column; }
    @media (max-width: 460px) { mat-dialog-content { min-width: auto; } }
  `],
})
export class UsuarioDialogComponent {
  private readonly service = inject(UsuariosService);
  readonly edicao: boolean;
  nome: string;
  email: string;
  confirmarEmail: string;
  senha = '';
  confirmarSenha = '';
  perfil: string;
  telefone: string;
  observacoes: string;

  readonly salvando = signal(false);
  readonly erro = signal<string | null>(null);

  constructor(
    readonly ref: MatDialogRef<UsuarioDialogComponent, Usuario>,
    @Inject(MAT_DIALOG_DATA) readonly data: { usuario?: Usuario; perfis: PerfilOpcao[] },
  ) {
    const u = data.usuario;
    this.edicao = !!u;
    this.nome = u?.nome ?? '';
    this.email = u?.email ?? '';
    this.confirmarEmail = u?.email ?? '';
    this.perfil = u?.perfil ?? data.perfis[0]?.nome ?? 'Operador';
    this.telefone = u?.telefone ?? '';
    this.observacoes = u?.observacoes ?? '';
  }

  salvar(): void {
    this.erro.set(null);
    if (this.email.trim().toLowerCase() !== this.confirmarEmail.trim().toLowerCase()) {
      this.erro.set('Os e-mails não coincidem.');
      return;
    }
    if (!this.edicao && this.senha !== this.confirmarSenha) {
      this.erro.set('As senhas não coincidem.');
      return;
    }
    this.salvando.set(true);
    const base = {
      nome: this.nome.trim(),
      email: this.email.trim(),
      perfil: this.perfil,
      telefone: this.telefone.trim() || null,
      observacoes: this.observacoes.trim() || null,
    };
    const obs = this.edicao
      ? this.service.atualizar(this.data.usuario!.id, base)
      : this.service.criar({ ...base, senha: this.senha });
    obs.subscribe({
      next: (u) => this.ref.close(u),
      error: (e) => {
        this.salvando.set(false);
        this.erro.set(msgErro(e));
      },
    });
  }
}

// ===================== Redefinir senha =====================
@Component({
  selector: 'app-redefinir-senha-dialog',
  standalone: true,
  imports: [FormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatIconModule],
  template: `
    <h2 mat-dialog-title>Redefinir senha</h2>
    <mat-dialog-content>
      <p class="quem">{{ data.usuario.nome }} <span>{{ data.usuario.email }}</span></p>
      <mat-form-field appearance="outline" class="full">
        <mat-label>Nova senha</mat-label>
        <input matInput type="password" [(ngModel)]="senha" />
        <mat-hint>Mínimo 6 caracteres</mat-hint>
      </mat-form-field>
      <mat-form-field appearance="outline" class="full">
        <mat-label>Confirmar nova senha</mat-label>
        <input matInput type="password" [(ngModel)]="confirmarSenha" (paste)="$event.preventDefault()" />
      </mat-form-field>
      @if (erro()) { <p class="erro"><mat-icon>error</mat-icon> {{ erro() }}</p> }
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="ref.close()">Cancelar</button>
      <button mat-flat-button class="btn-cta" [disabled]="salvando()" (click)="salvar()"><mat-icon>key</mat-icon> Redefinir</button>
    </mat-dialog-actions>
  `,
  styles: [`
    .quem { margin: 0 0 0.75rem; font-weight: 600; color: var(--an-texto-titulo); span { display: block; font-weight: 400; font-size: 0.85rem; color: var(--an-texto-secundario); } }
    .full { width: 100%; }
    .erro { display: flex; align-items: center; gap: 0.4rem; color: #b3261e; font-size: 0.85rem; margin: 0; }
    .erro .mat-icon { font-size: 1.05rem; width: 1.05rem; height: 1.05rem; }
    .btn-cta { background: var(--an-cta); color: #fff; }
    mat-dialog-content { min-width: 360px; }
    @media (max-width: 400px) { mat-dialog-content { min-width: auto; } }
  `],
})
export class RedefinirSenhaDialogComponent {
  private readonly service = inject(UsuariosService);
  senha = '';
  confirmarSenha = '';
  readonly salvando = signal(false);
  readonly erro = signal<string | null>(null);

  constructor(
    readonly ref: MatDialogRef<RedefinirSenhaDialogComponent, boolean>,
    @Inject(MAT_DIALOG_DATA) readonly data: { usuario: Usuario },
  ) {}

  salvar(): void {
    this.erro.set(null);
    if (this.senha !== this.confirmarSenha) {
      this.erro.set('As senhas não coincidem.');
      return;
    }
    this.salvando.set(true);
    this.service.redefinirSenha(this.data.usuario.id, this.senha).subscribe({
      next: () => this.ref.close(true),
      error: (e) => {
        this.salvando.set(false);
        this.erro.set(msgErro(e));
      },
    });
  }
}
