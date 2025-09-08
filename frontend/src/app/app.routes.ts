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
import { AuthGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'auth/callback', component: AuthCallbackComponent },
  { path: 'dashboard', component: DashboardComponent, canActivate: [AuthGuard] },
  { path: 'contas', component: ContasComponent, canActivate: [AuthGuard] },
  { path: 'categorias', component: CategoriasComponent, canActivate: [AuthGuard] },
  { path: 'lancamentos', component: LancamentosComponent, canActivate: [AuthGuard] },
  { path: 'folha-mensal', component: FolhaMensalComponent, canActivate: [AuthGuard] },
  { path: 'provisionamento', component: ProvisionamentoComponent, canActivate: [AuthGuard] },
  { path: 'contas-conjuntas', component: ContasConjuntasComponent, canActivate: [AuthGuard] },
  { path: 'relatorios/categorias', component: RelatoriosCategoriasComponent, canActivate: [AuthGuard] },
  { path: 'relatorios/evolucao', component: RelatoriosEvolucaoComponent, canActivate: [AuthGuard] },
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: '**', redirectTo: '/login' } // Redireciona para o login se a rota não for encontrada
];

