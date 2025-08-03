import { Component, OnInit } from '@angular/core';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MatToolbarModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatMenuModule
  ],
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.css']
})
export class DashboardComponent implements OnInit {
  userName: string = 'Usuário';
  totalBalance: number = 0;
  monthlyIncome: number = 0;
  monthlyExpense: number = 0;

  ngOnInit() {
    this.loadUserData();
    this.loadFinancialSummary();
  }

  loadUserData() {
    // Aqui você carregaria os dados do usuário do localStorage ou de um serviço
    const userData = localStorage.getItem('user');
    if (userData) {
      const user = JSON.parse(userData);
      this.userName = user.nome || 'Usuário';
    }
  }

  loadFinancialSummary() {
    // Aqui você faria chamadas para a API para carregar os dados financeiros
    // Por enquanto, vamos usar dados mockados
    this.totalBalance = 5000.00;
    this.monthlyIncome = 3000.00;
    this.monthlyExpense = 2500.00;
  }

  logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    window.location.href = '/login';
  }
}

