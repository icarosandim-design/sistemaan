import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';

/**
 * Página interna TEMPORÁRIA, usada apenas para validar o login
 * (redirecionamento, persistência de sessão e logout).
 * Será substituída pelo Layout Principal + Dashboard nas próximas etapas.
 */
@Component({
  selector: 'app-inicio',
  standalone: true,
  template: `
    <div class="placeholder">
      <div class="card">
        <div class="check">✅</div>
        <h1>Autenticado com sucesso</h1>
        <p>Olá, <strong>{{ auth.usuario()?.nome }}</strong></p>
        <p class="muted">{{ auth.usuario()?.email }}</p>
        <p class="muted">Papéis: {{ auth.usuario()?.papeis?.join(', ') }}</p>
        <p class="nota">
          Tela temporária para validar o login. O Layout Principal e o Dashboard
          virão nas próximas etapas.
        </p>
        <button (click)="sair()">Sair</button>
      </div>
    </div>
  `,
  styles: [
    `
      .placeholder {
        min-height: 100vh;
        display: grid;
        place-items: center;
        background: var(--an-fundo, #f4efe5);
        font-family: system-ui, -apple-system, 'Segoe UI', Roboto, sans-serif;
        padding: 1rem;
      }
      .card {
        background: #fff;
        border: 1px solid #e5e7eb;
        border-radius: 16px;
        padding: 2.5rem;
        max-width: 460px;
        width: 100%;
        text-align: center;
        box-shadow: 0 10px 40px rgba(17, 24, 39, 0.08);
      }
      .check {
        font-size: 2.5rem;
      }
      h1 {
        font-size: 1.4rem;
        margin: 0.5rem 0 1rem;
      }
      .muted {
        color: #6b7280;
        margin: 0.25rem 0;
      }
      .nota {
        color: #9ca3af;
        font-size: 0.85rem;
        margin: 1.25rem 0 0;
      }
      button {
        margin-top: 1.5rem;
        padding: 0.6rem 1.6rem;
        border: 0;
        border-radius: 10px;
        background: var(--an-primaria, #3f4f2d);
        color: #fff;
        font-weight: 600;
        cursor: pointer;
      }
      button:hover {
        background: var(--an-primaria-hover, #2e3a21);
      }
    `,
  ],
})
export class InicioComponent {
  readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  sair(): void {
    this.auth.logout();
    this.router.navigateByUrl('/login');
  }
}
