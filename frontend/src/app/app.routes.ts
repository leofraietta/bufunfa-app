import { Routes } from '@angular/router';
import { LoginComponent } from './login/login';
import { DashboardComponent } from './dashboard/dashboard';
import { ContasComponent } from './contas/contas';
import { LancamentosComponent } from './lancamentos/lancamentos';
import { ContasConjuntasComponent } from './contas-conjuntas/contas-conjuntas';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'dashboard', component: DashboardComponent },
  { path: 'contas', component: ContasComponent },
  { path: 'lancamentos', component: LancamentosComponent },
  { path: 'contas-conjuntas', component: ContasConjuntasComponent },
  { path: '', redirectTo: '/dashboard', pathMatch: 'full' },
  { path: '**', redirectTo: '/dashboard' } // Redireciona para o dashboard se a rota não for encontrada
];

