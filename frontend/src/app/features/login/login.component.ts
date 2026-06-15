import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '../../core/auth/auth.service';
import { rotaInicial } from '../../core/auth/perfis';
import { TokenStorageService } from '../../core/auth/token-storage.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatCheckboxModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly storage = inject(TokenStorageService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly carregando = signal(false);
  readonly erro = signal<string | null>(null);
  readonly mostrarSenha = signal(false);

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    senha: ['', [Validators.required]],
    lembrar: [false],
  });

  constructor() {
    if (this.auth.estaAutenticado()) {
      this.router.navigateByUrl(rotaInicial(this.auth.papeis()));
      return;
    }

    const lembrado = this.storage.emailLembrado;
    if (lembrado) {
      this.form.patchValue({ email: lembrado, lembrar: true });
    }
  }

  alternarSenha(): void {
    this.mostrarSenha.update((v) => !v);
  }

  enviar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.carregando.set(true);
    this.erro.set(null);

    const { email, senha, lembrar } = this.form.getRawValue();

    this.auth.login({ email, senha }).subscribe({
      next: () => {
        if (lembrar) {
          this.storage.lembrarEmail(email);
        } else {
          this.storage.esquecerEmail();
        }

        const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl') ?? rotaInicial(this.auth.papeis());
        this.router.navigateByUrl(returnUrl);
      },
      error: (e: HttpErrorResponse) => {
        this.carregando.set(false);
        this.erro.set(this.mensagemErro(e));
      },
    });
  }

  private mensagemErro(e: HttpErrorResponse): string {
    switch (e.status) {
      case 0:
        return 'Não foi possível conectar ao servidor. Verifique sua conexão.';
      case 401:
        return 'E-mail ou senha inválidos.';
      case 403:
        return 'Usuário inativo. Contate o administrador.';
      default:
        return 'Ocorreu um erro inesperado. Tente novamente.';
    }
  }
}
