import { Component, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { map } from 'rxjs';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { AuthService } from '../core/auth/auth.service';

interface NavItem {
  label: string;
  icone: string;
  rota?: string | null;
  filhos?: NavItem[];
}

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    MatToolbarModule,
    MatSidenavModule,
    MatListModule,
    MatIconModule,
    MatButtonModule,
    MatMenuModule,
    MatTooltipModule,
  ],
  templateUrl: './main-layout.component.html',
  styleUrl: './main-layout.component.scss',
})
export class MainLayoutComponent {
  readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly breakpoint = inject(BreakpointObserver);

  readonly isMobile = toSignal(
    this.breakpoint.observe([Breakpoints.Handset, Breakpoints.TabletPortrait]).pipe(map((r) => r.matches)),
    { initialValue: false },
  );

  readonly menu: NavItem[] = [
    { label: 'Central Operacional', icone: 'space_dashboard', rota: '/central' },
    {
      label: 'Clientes',
      icone: 'group',
      filhos: [
        { label: 'Clientes', icone: 'badge', rota: null },
        { label: 'Pets', icone: 'pets', rota: null },
      ],
    },
    { label: 'Produção', icone: 'factory', rota: null },
    { label: 'Estoque', icone: 'inventory_2', rota: null },
    { label: 'Entregas', icone: 'local_shipping', rota: null },
    {
      label: 'Cadastros',
      icone: 'tune',
      filhos: [
        { label: 'Receitas', icone: 'menu_book', rota: '/receitas' },
        { label: 'Ingredientes', icone: 'eco', rota: '/ingredientes' },
        { label: 'Tabela de Consumo', icone: 'monitor_weight', rota: '/tabela-consumo' },
      ],
    },
  ];

  private readonly abertos = signal<Set<string>>(this.gruposIniciais());

  isAberto(label: string): boolean {
    return this.abertos().has(label);
  }

  alternarGrupo(label: string): void {
    const set = new Set(this.abertos());
    set.has(label) ? set.delete(label) : set.add(label);
    this.abertos.set(set);
  }

  sair(): void {
    this.auth.logout();
    this.router.navigateByUrl('/login');
  }

  private gruposIniciais(): Set<string> {
    const url = this.router.url;
    const grupoAtivo = this.menu.find((m) => m.filhos?.some((f) => f.rota && url.startsWith(f.rota)));
    return new Set([grupoAtivo?.label ?? 'Cadastros']);
  }
}
