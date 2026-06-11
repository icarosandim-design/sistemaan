import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';
import { LoginComponent } from './features/login/login.component';
import { MainLayoutComponent } from './layout/main-layout.component';
import { CentralOperacionalComponent } from './features/central/central-operacional.component';
import { IngredientesComponent } from './features/ingredientes/ingredientes.component';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  {
    path: '',
    component: MainLayoutComponent,
    canActivate: [authGuard],
    children: [
      { path: 'central', component: CentralOperacionalComponent },
      { path: 'ingredientes', component: IngredientesComponent },
      { path: '', pathMatch: 'full', redirectTo: 'central' },
    ],
  },
  { path: '**', redirectTo: '' },
];
