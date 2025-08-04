import { Routes } from '@angular/router';
import { LoginComponent } from './login/login';
import { DashboardComponent } from './dashboard/dashboard';
import { ContasComponent } from './contas/contas';
import { LancamentosComponent } from './lancamentos/lancamentos';
import { ContasConjuntasComponent } from './contas-conjuntas/contas-conjuntas';
import { AuthCallbackComponent } from './auth/auth-callback';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'auth/callback', component: AuthCallbackComponent },
  { path: 'dashboard', component: DashboardComponent },
  { path: 'contas', component: ContasComponent },
  { path: 'lancamentos', component: LancamentosComponent },
  { path: 'contas-conjuntas', component: ContasConjuntasComponent },
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: '**', redirectTo: '/login' } // Redireciona para o login se a rota não for encontrada
];

