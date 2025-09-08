import { Component, OnInit } from '@angular/core';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ApiService, DashboardData, Lancamento } from '../services/api.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatToolbarModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatMenuModule,
    MatSnackBarModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.css']
})
export class DashboardComponent implements OnInit {
  user: any = {};
  dashboardData: DashboardData | null = null;
  isLoading = true;
  error: string | null = null;
  isUsingMockData = false;

  constructor(
    private apiService: ApiService,
    private snackBar: MatSnackBar
  ) { }

  ngOnInit(): void {
    this.loadUserData();
    this.loadDashboardData();
  }

  loadUserData() {
    const userData = localStorage.getItem('user');
    if (userData) {
      this.user = JSON.parse(userData);
    }
  }

  loadDashboardData() {
    this.isLoading = true;
    this.error = null;

    this.apiService.getDashboardData().subscribe({
      next: (data) => {
        this.dashboardData = data;
        this.isLoading = false;
        // Se os dados são exatamente iguais aos mock data, provavelmente são dados ilustrativos
        this.isUsingMockData = this.checkIfMockData(data);
      },
      error: (error) => {
        console.error('Erro ao carregar dados do dashboard:', error);
        this.isLoading = false;
        this.error = null; // Clear error to show fallback data
        this.isUsingMockData = true; // Fallback sempre usa dados mock
        
        // O fallback já é tratado no serviço, então não precisamos fazer nada aqui
        // A mensagem de aviso será mostrada apenas se necessário
      }
    });
  }

  get totalBalance(): number {
    return this.dashboardData?.saldoTotal || 0;
  }

  get monthlyIncome(): number {
    return this.dashboardData?.receitasMensais || 0;
  }

  get monthlyExpenses(): number {
    return this.dashboardData?.despesasMensais || 0;
  }

  get projectedBalance(): number {
    return this.dashboardData?.projecaoSaldo || 0;
  }

  get upcomingTransactions(): Lancamento[] {
    return this.dashboardData?.proximosVencimentos || [];
  }

  getTipoLancamentoText(tipo: number): string {
    return this.apiService.getTipoLancamentoText(tipo);
  }

  getTipoRecorrenciaText(tipo: number): string {
    return this.apiService.getTipoRecorrenciaText(tipo);
  }

  refreshData() {
    this.loadDashboardData();
  }

  getFormattedDate(dateValue: any): string {
    if (!dateValue) return '';
    
    try {
      let date: Date;
      
      if (typeof dateValue === 'string') {
        // Se for string, tentar converter para Date
        date = new Date(dateValue);
      } else if (dateValue instanceof Date) {
        date = dateValue;
      } else {
        return '';
      }
      
      // Verificar se a data é válida
      if (isNaN(date.getTime())) {
        return '';
      }
      
      // Formatar como dd/MM
      const day = date.getDate().toString().padStart(2, '0');
      const month = (date.getMonth() + 1).toString().padStart(2, '0');
      return `${day}/${month}`;
    } catch (error) {
      console.warn('Erro ao formatar data:', error);
      return '';
    }
  }

  checkIfMockData(data: DashboardData): boolean {
    // Verifica se os dados são exatamente iguais aos dados mock
    const mockData = this.apiService.getMockDashboardData();
    return data.saldoTotal === mockData.saldoTotal &&
           data.receitasMensais === mockData.receitasMensais &&
           data.despesasMensais === mockData.despesasMensais &&
           data.projecaoSaldo === mockData.projecaoSaldo;
  }

  logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    window.location.href = '/login';
  }
}

