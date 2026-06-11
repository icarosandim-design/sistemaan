import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';
import { LoginComponent } from './features/login/login.component';
import { MainLayoutComponent } from './layout/main-layout.component';
import { CentralOperacionalComponent } from './features/central/central-operacional.component';
import { IngredientesComponent } from './features/ingredientes/ingredientes.component';
import { ConsumoComponent } from './features/consumo/consumo.component';
import { ReceitasComponent } from './features/receitas/receitas.component';
import { FrequenciasComponent } from './features/frequencias/frequencias.component';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  {
    path: '',
    component: MainLayoutComponent,
    canActivate: [authGuard],
    children: [
      { path: 'central', component: CentralOperacionalComponent },
      { path: 'ingredientes', component: IngredientesComponent },
      { path: 'tabela-consumo', component: ConsumoComponent },
      { path: 'receitas', component: ReceitasComponent },
      { path: 'frequencias-entrega', component: FrequenciasComponent },
      { path: '', pathMatch: 'full', redirectTo: 'central' },
    ],
  },
  { path: '**', redirectTo: '' },
];
