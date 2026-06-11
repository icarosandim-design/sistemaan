import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly carregando = signal(false);
  readonly erro = signal<string | null>(null);
  readonly mostrarSenha = signal(false);

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    senha: ['', [Validators.required]],
  });

  constructor() {
    // Já autenticado? Vai direto para a área interna.
    if (this.auth.estaAutenticado()) {
      this.router.navigateByUrl('/inicio');
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

    this.auth.login(this.form.getRawValue()).subscribe({
      next: () => {
        const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl') ?? '/inicio';
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
