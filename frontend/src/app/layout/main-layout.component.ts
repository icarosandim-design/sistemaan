import { Component, computed, inject, signal } from '@angular/core';
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
import { ADMIN_OPERADOR, SO_ADMIN, TODOS_PERFIS } from '../core/auth/perfis';

interface NavItem {
  label: string;
  icone: string;
  rota?: string | null;
  filhos?: NavItem[];
  /** Perfis que enxergam o item. Ausente = todos os autenticados. */
  papeis?: string[];
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
    { label: 'Central Operacional', icone: 'space_dashboard', rota: '/central', papeis: ADMIN_OPERADOR },
    {
      label: 'Clientes',
      icone: 'group',
      papeis: ADMIN_OPERADOR,
      filhos: [
        { label: 'Clientes', icone: 'badge', rota: '/clientes', papeis: ADMIN_OPERADOR },
        { label: 'Clientes PJ', icone: 'store', rota: '/clientes-pj', papeis: ADMIN_OPERADOR },
        { label: 'Pets', icone: 'pets', rota: '/pets', papeis: ADMIN_OPERADOR },
      ],
    },
    {
      label: 'Produção',
      icone: 'factory',
      papeis: TODOS_PERFIS,
      filhos: [
        { label: 'Planejar produção', icone: 'event_note', rota: '/producao/planejar', papeis: ADMIN_OPERADOR },
        { label: 'Produção do dia', icone: 'soup_kitchen', rota: '/producao/dia', papeis: TODOS_PERFIS },
        { label: 'Cozinha', icone: 'tv', rota: '/producao/cozinha', papeis: TODOS_PERFIS },
        { label: 'Rendimentos e perdas', icone: 'insights', rota: '/producao/rendimentos', papeis: ADMIN_OPERADOR },
      ],
    },
    {
      label: 'Estoque',
      icone: 'inventory_2',
      papeis: ADMIN_OPERADOR,
      filhos: [
        { label: 'Itens de Estoque', icone: 'inventory', rota: '/estoque/itens', papeis: ADMIN_OPERADOR },
        { label: 'Compras / Entradas', icone: 'shopping_cart', rota: '/estoque/compras', papeis: ADMIN_OPERADOR },
        { label: 'Movimentações', icone: 'sync_alt', rota: '/estoque/movimentacoes', papeis: ADMIN_OPERADOR },
        { label: 'Fornecedores', icone: 'local_shipping', rota: '/estoque/fornecedores', papeis: ADMIN_OPERADOR },
      ],
    },
    { label: 'Entregas', icone: 'local_shipping', rota: '/entregas', papeis: ADMIN_OPERADOR },
    {
      label: 'Cadastros',
      icone: 'tune',
      papeis: ADMIN_OPERADOR,
      filhos: [
        { label: 'Receitas da Casa', icone: 'menu_book', rota: '/receitas', papeis: ADMIN_OPERADOR },
        { label: 'Tamanhos de Pacote', icone: 'inventory_2', rota: '/tamanhos-pacote', papeis: ADMIN_OPERADOR },
        { label: 'Ingredientes', icone: 'eco', rota: '/ingredientes', papeis: ADMIN_OPERADOR },
        { label: 'Categorias', icone: 'category', rota: '/cadastros/categorias', papeis: ADMIN_OPERADOR },
        { label: 'Tabela de Consumo', icone: 'monitor_weight', rota: '/tabela-consumo', papeis: ADMIN_OPERADOR },
        { label: 'Frequências de Entrega', icone: 'event_repeat', rota: '/frequencias-entrega', papeis: ADMIN_OPERADOR },
        { label: 'Usuários', icone: 'manage_accounts', rota: '/cadastros/usuarios', papeis: SO_ADMIN },
      ],
    },
  ];

  /** Menu filtrado pelos papéis do usuário atual (esconde grupos vazios). */
  readonly menuVisivel = computed<NavItem[]>(() => {
    const meus = this.auth.papeis();
    const pode = (it: NavItem) => !it.papeis || it.papeis.some((p) => meus.includes(p));
    return this.menu
      .map((item) => {
        if (item.filhos) {
          const filhos = item.filhos.filter(pode);
          return filhos.length ? { ...item, filhos } : null;
        }
        return pode(item) ? item : null;
      })
      .filter((x): x is NavItem => x !== null);
  });

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
