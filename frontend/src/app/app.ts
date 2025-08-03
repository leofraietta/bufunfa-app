import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { LoginComponent } from './login/login';
import { DashboardComponent } from './dashboard/dashboard';
import { ContasComponent } from './contas/contas';
import { LancamentosComponent } from './lancamentos/lancamentos';
import { ContasConjuntasComponent } from './contas-conjuntas/contas-conjuntas';
import { CommonModule } from '@angular/common';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    RouterOutlet,
    CommonModule,
    LoginComponent,
    DashboardComponent,
    ContasComponent,
    LancamentosComponent,
    ContasConjuntasComponent,
    MatToolbarModule,
    MatButtonModule,
    MatIconModule
  ],
  templateUrl: './app.html',
  styleUrls: ['./app.css']
})
export class AppComponent {
  title = 'Bufunfa - Controle Financeiro Pessoal';
  currentView = 'login'; // Controla qual componente mostrar

  constructor() {
    // Verifica se o usuário já está logado
    const token = localStorage.getItem('token');
    if (token) {
      this.currentView = 'dashboard';
    }
  }

  showLogin() {
    this.currentView = 'login';
  }

  showDashboard() {
    this.currentView = 'dashboard';
  }

  showContas() {
    this.currentView = 'contas';
  }

  showLancamentos() {
    this.currentView = 'lancamentos';
  }

  showContasConjuntas() {
    this.currentView = 'contas-conjuntas';
  }

  logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    this.currentView = 'login';
  }
}

