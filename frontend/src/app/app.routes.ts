import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';
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

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  {
    path: '',
    component: MainLayoutComponent,
    canActivate: [authGuard],
    children: [
      { path: 'central', component: CentralOperacionalComponent },
      { path: 'clientes', component: ClientesComponent },
      { path: 'entregas', component: EntregasComponent },
      { path: 'estoque/itens', component: ItensEstoqueComponent },
      { path: 'estoque/compras', component: ComprasComponent },
      { path: 'estoque/movimentacoes', component: MovimentacoesComponent },
      { path: 'estoque/fornecedores', component: FornecedoresComponent },
      { path: 'cadastros/categorias', component: CategoriasComponent },
      { path: 'ingredientes', component: IngredientesComponent },
      { path: 'tabela-consumo', component: ConsumoComponent },
      { path: 'receitas', component: ReceitasComponent },
      { path: 'tamanhos-pacote', component: TamanhosPacoteComponent },
      { path: 'frequencias-entrega', component: FrequenciasComponent },
      { path: '', pathMatch: 'full', redirectTo: 'central' },
    ],
  },
  { path: '**', redirectTo: '' },
];
