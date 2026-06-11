import { Component, inject } from '@angular/core';
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
  rota: string | null;
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

  /** Navegação preparada para as próximas telas (itens sem rota ficam "Em breve"). */
  readonly navItems: NavItem[] = [
    { label: 'Central Operacional', icone: 'space_dashboard', rota: '/central' },
    { label: 'Clientes', icone: 'group', rota: null },
    { label: 'Pets', icone: 'pets', rota: null },
    { label: 'Receitas', icone: 'menu_book', rota: null },
    { label: 'Ingredientes', icone: 'eco', rota: '/ingredientes' },
    { label: 'Tabela de Consumo', icone: 'monitor_weight', rota: '/tabela-consumo' },
    { label: 'Produção', icone: 'factory', rota: null },
    { label: 'Estoque', icone: 'inventory_2', rota: null },
    { label: 'Entregas', icone: 'local_shipping', rota: null },
  ];

  sair(): void {
    this.auth.logout();
    this.router.navigateByUrl('/login');
  }
}
