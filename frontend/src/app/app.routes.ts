import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';
import { papelGuard } from './core/auth/papel.guard';
import { ADMIN_OPERADOR, SO_ADMIN, TODOS_PERFIS } from './core/auth/perfis';
import { LoginComponent } from './features/login/login.component';
import { MainLayoutComponent } from './layout/main-layout.component';
import { CentralOperacionalComponent } from './features/central/central-operacional.component';
import { IngredientesComponent } from './features/ingredientes/ingredientes.component';
import { ConsumoComponent } from './features/consumo/consumo.component';
import { ReceitasComponent } from './features/receitas/receitas.component';
import { FrequenciasComponent } from './features/frequencias/frequencias.component';
import { ClientesComponent } from './features/clientes/clientes.component';
import { TamanhosPacoteComponent } from './features/tamanhos-pacote/tamanhos-pacote.component';
import { EntregasComponent } from './features/entregas/entregas.component';
import { ItensEstoqueComponent } from './features/estoque/itens-estoque.component';
import { FornecedoresComponent } from './features/estoque/fornecedores.component';
import { MovimentacoesComponent } from './features/estoque/movimentacoes.component';
import { ComprasComponent } from './features/estoque/compras.component';
import { CategoriasComponent } from './features/categorias/categorias.component';
import { OrigensVendaComponent } from './features/origens-venda/origens-venda.component';
import { ProducaoPlanejarComponent } from './features/producao/planejar.component';
import { ProducaoDiaComponent } from './features/producao/producao-dia.component';
import { ProducaoCozinhaComponent } from './features/producao/cozinha.component';
import { ProducaoImpressaoComponent } from './features/producao/impressao.component';
import { ProducaoRendimentosComponent } from './features/producao/rendimentos.component';
import { UsuariosComponent } from './features/usuarios/usuarios.component';
import { ClientesPjComponent } from './features/clientes-pj/clientes-pj.component';
import { PetsComponent } from './features/pets/pets.component';
import { PlanejarRotasComponent } from './features/rotas/planejar-rotas.component';
import { RotaImpressaoComponent } from './features/rotas/rota-impressao.component';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  // Pré-visualização de impressão (tela cheia, sem layout) — protótipo
  { path: 'producao/impressao/:tipo', component: ProducaoImpressaoComponent, canActivate: [authGuard, papelGuard], data: { papeis: TODOS_PERFIS } },
  { path: 'rotas/:id/impressao', component: RotaImpressaoComponent, canActivate: [authGuard, papelGuard], data: { papeis: ADMIN_OPERADOR } },
  {
    path: '',
    component: MainLayoutComponent,
    canActivate: [authGuard],
    canActivateChild: [papelGuard],
    children: [
      { path: 'central', component: CentralOperacionalComponent, data: { papeis: ADMIN_OPERADOR } },
      { path: 'clientes', component: ClientesComponent, data: { papeis: ADMIN_OPERADOR } },
      { path: 'clientes-pj', component: ClientesPjComponent, data: { papeis: ADMIN_OPERADOR } },
      { path: 'pets', component: PetsComponent, data: { papeis: ADMIN_OPERADOR } },
      { path: 'rotas', component: PlanejarRotasComponent, data: { papeis: ADMIN_OPERADOR } },
      { path: 'entregas', component: EntregasComponent, data: { papeis: ADMIN_OPERADOR } },
      { path: 'estoque/itens', component: ItensEstoqueComponent, data: { papeis: ADMIN_OPERADOR } },
      { path: 'estoque/compras', component: ComprasComponent, data: { papeis: ADMIN_OPERADOR } },
      { path: 'estoque/movimentacoes', component: MovimentacoesComponent, data: { papeis: ADMIN_OPERADOR } },
      { path: 'estoque/fornecedores', component: FornecedoresComponent, data: { papeis: ADMIN_OPERADOR } },
      { path: 'cadastros/categorias', component: CategoriasComponent, data: { papeis: ADMIN_OPERADOR } },
      { path: 'cadastros/origens-venda', component: OrigensVendaComponent, data: { papeis: ADMIN_OPERADOR } },
      { path: 'cadastros/usuarios', component: UsuariosComponent, data: { papeis: SO_ADMIN } },
      { path: 'producao/planejar', component: ProducaoPlanejarComponent, data: { papeis: ADMIN_OPERADOR } },
      { path: 'producao/dia', component: ProducaoDiaComponent, data: { papeis: TODOS_PERFIS } },
      { path: 'producao/cozinha', component: ProducaoCozinhaComponent, data: { papeis: TODOS_PERFIS } },
      { path: 'producao/rendimentos', component: ProducaoRendimentosComponent, data: { papeis: ADMIN_OPERADOR } },
      { path: 'ingredientes', component: IngredientesComponent, data: { papeis: ADMIN_OPERADOR } },
      { path: 'tabela-consumo', component: ConsumoComponent, data: { papeis: ADMIN_OPERADOR } },
      { path: 'receitas', component: ReceitasComponent, data: { papeis: ADMIN_OPERADOR } },
      { path: 'tamanhos-pacote', component: TamanhosPacoteComponent, data: { papeis: ADMIN_OPERADOR } },
      { path: 'frequencias-entrega', component: FrequenciasComponent, data: { papeis: ADMIN_OPERADOR } },
      { path: '', pathMatch: 'full', redirectTo: 'central' },
    ],
  },
  { path: '**', redirectTo: '' },
];
