import { Routes } from '@angular/router';
import { LoginComponent } from './login/login';
import { DashboardComponent } from './dashboard/dashboard';
import { ContasComponent } from './contas/contas';
import { CategoriasComponent } from './categorias/categorias';
import { LancamentosComponent } from './lancamentos/lancamentos';
import { ContasConjuntasComponent } from './contas-conjuntas/contas-conjuntas';
import { FolhaMensalComponent } from './folha-mensal/folha-mensal';
import { ProvisionamentoComponent } from './provisionamento/provisionamento';
import { AuthCallbackComponent } from './auth/auth-callback';
import { RelatoriosCategoriasComponent } from './relatorios/categorias/relatorios-categorias';
import { RelatoriosEvolucaoComponent } from './relatorios/evolucao/relatorios-evolucao';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'auth/callback', component: AuthCallbackComponent },
  { path: 'dashboard', component: DashboardComponent },
  { path: 'contas', component: ContasComponent },
  { path: 'categorias', component: CategoriasComponent },
  { path: 'lancamentos', component: LancamentosComponent },
  { path: 'folha-mensal', component: FolhaMensalComponent },
  { path: 'provisionamento', component: ProvisionamentoComponent },
  { path: 'contas-conjuntas', component: ContasConjuntasComponent },
  { path: 'relatorios/categorias', component: RelatoriosCategoriasComponent },
  { path: 'relatorios/evolucao', component: RelatoriosEvolucaoComponent },
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: '**', redirectTo: '/login' } // Redireciona para o login se a rota não for encontrada
];

