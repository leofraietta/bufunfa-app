import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog } from '@angular/material/dialog';
import { ApiService } from '../services/api';

interface ResumoProvisionamento {
  id: number;
  contaNome: string;
  categoriaNome: string;
  ano: number;
  mes: number;
  valorProvisionado: number;
  valorGastoReal: number;
  diferenca: number;
  percentualUtilizado: number;
  quantidadeGastos: number;
  gastosReais: GastoReal[];
}

interface GastoReal {
  id: number;
  descricao: string;
  valor: number;
  data: Date;
}

interface ResumoGeral {
  totalProvisionado: number;
  totalGasto: number;
  diferenca: number;
  percentualUtilizado: number;
}

@Component({
  selector: 'app-provisionamento',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatSelectModule,
    MatProgressBarModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './provisionamento.html',
  styleUrls: ['./provisionamento.css']
})
export class ProvisionamentoComponent implements OnInit {
  provisionamentos: ResumoProvisionamento[] = [];
  resumoGeral: ResumoGeral | null = null;
  isLoading = false;
  isUsingMockData = true;
  mesAnoSelecionado: string;
  mesesDisponiveis: { value: string; label: string }[] = [];

  constructor(
    private apiService: ApiService,
    private dialog: MatDialog
  ) {
    this.mesAnoSelecionado = this.getCurrentMonthYear();
    this.generateAvailableMonths();
  }

  ngOnInit() {
    this.loadProvisionamentos();
  }

  getCurrentMonthYear(): string {
    const now = new Date();
    return `${now.getFullYear()}-${(now.getMonth() + 1).toString().padStart(2, '0')}`;
  }

  generateAvailableMonths() {
    const months = [];
    const now = new Date();
    
    // Gerar últimos 6 meses e próximos 6 meses
    for (let i = -6; i <= 6; i++) {
      const date = new Date(now.getFullYear(), now.getMonth() + i, 1);
      const value = `${date.getFullYear()}-${(date.getMonth() + 1).toString().padStart(2, '0')}`;
      const label = date.toLocaleDateString('pt-BR', { month: 'long', year: 'numeric' });
      months.push({ value, label });
    }
    
    this.mesesDisponiveis = months;
  }

  onMesAnoChange() {
    this.loadProvisionamentos();
  }

  loadProvisionamentos() {
    this.isLoading = true;
    const [ano, mes] = this.mesAnoSelecionado.split('-').map(Number);

    // Simular dados enquanto a API não está disponível
    setTimeout(() => {
      this.provisionamentos = this.getMockProvisionamentos();
      this.calculateResumoGeral();
      this.isLoading = false;
      this.isUsingMockData = this.checkIfMockData();
    }, 1000);

    // TODO: Implementar chamada real para API quando estiver disponível
    /*
    this.apiService.getProvisionamentos(ano, mes).subscribe({
      next: (data) => {
        this.provisionamentos = data;
        this.calculateResumoGeral();
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Erro ao carregar provisionamentos:', error);
        this.provisionamentos = this.getMockProvisionamentos();
        this.calculateResumoGeral();
        this.isLoading = false;
      }
    });
    */
  }

  getMockProvisionamentos(): ResumoProvisionamento[] {
    return [
      {
        id: 1,
        contaNome: 'Conta Corrente',
        categoriaNome: 'Supermercado',
        ano: 2025,
        mes: 8,
        valorProvisionado: 800.00,
        valorGastoReal: 650.50,
        diferenca: 149.50,
        percentualUtilizado: 81.3,
        quantidadeGastos: 12,
        gastosReais: [
          { id: 1, descricao: 'Compras Extra', valor: 125.30, data: new Date('2025-08-15') },
          { id: 2, descricao: 'Feira livre', valor: 85.20, data: new Date('2025-08-12') },
          { id: 3, descricao: 'Padaria', valor: 45.00, data: new Date('2025-08-10') }
        ]
      },
      {
        id: 2,
        contaNome: 'Cartão Visa',
        categoriaNome: 'Farmácia',
        ano: 2025,
        mes: 8,
        valorProvisionado: 200.00,
        valorGastoReal: 180.75,
        diferenca: 19.25,
        percentualUtilizado: 90.4,
        quantidadeGastos: 5,
        gastosReais: [
          { id: 4, descricao: 'Remédios', valor: 95.50, data: new Date('2025-08-14') },
          { id: 5, descricao: 'Vitaminas', valor: 85.25, data: new Date('2025-08-08') }
        ]
      },
      {
        id: 3,
        contaNome: 'Conta Corrente',
        categoriaNome: 'Combustível',
        ano: 2025,
        mes: 8,
        valorProvisionado: 400.00,
        valorGastoReal: 420.80,
        diferenca: -20.80,
        percentualUtilizado: 105.2,
        quantidadeGastos: 8,
        gastosReais: [
          { id: 6, descricao: 'Posto Shell', valor: 180.00, data: new Date('2025-08-16') },
          { id: 7, descricao: 'Posto BR', valor: 165.40, data: new Date('2025-08-09') },
          { id: 8, descricao: 'Posto Ipiranga', valor: 75.40, data: new Date('2025-08-03') }
        ]
      }
    ];
  }

  calculateResumoGeral() {
    if (this.provisionamentos.length === 0) {
      this.resumoGeral = null;
      return;
    }

    const totalProvisionado = this.provisionamentos.reduce((sum, p) => sum + p.valorProvisionado, 0);
    const totalGasto = this.provisionamentos.reduce((sum, p) => sum + p.valorGastoReal, 0);
    const diferenca = totalProvisionado - totalGasto;
    const percentualUtilizado = totalProvisionado > 0 ? (totalGasto / totalProvisionado) * 100 : 0;

    this.resumoGeral = {
      totalProvisionado,
      totalGasto,
      diferenca,
      percentualUtilizado
    };
  }

  getProgressBarClass(percentual: number): string {
    if (percentual <= 70) return 'normal';
    if (percentual <= 90) return 'warning';
    return 'danger';
  }

  openProvisionamentoDialog() {
    // TODO: Implementar dialog para criar/editar provisionamento
    console.log('Abrir dialog de provisionamento');
  }

  editarProvisionamento(provisionamento: ResumoProvisionamento) {
    // TODO: Implementar edição de provisionamento
    console.log('Editar provisionamento:', provisionamento);
  }

  adicionarGasto(provisionamentoId: number) {
    // TODO: Implementar dialog para adicionar gasto real
    console.log('Adicionar gasto para provisionamento:', provisionamentoId);
  }

  verTodosGastos(provisionamento: ResumoProvisionamento) {
    // TODO: Implementar dialog para ver todos os gastos
    console.log('Ver todos os gastos:', provisionamento);
  }

  checkIfMockData(): boolean {
    // Verifica se os dados correspondem aos dados mock
    // Compara com alguns valores específicos dos dados mock
    const mockData = this.getMockProvisionamentos();
    
    if (this.provisionamentos.length !== mockData.length) {
      return false;
    }
    
    // Verifica se os primeiros valores coincidem com os dados mock
    for (let i = 0; i < Math.min(2, this.provisionamentos.length); i++) {
      const current = this.provisionamentos[i];
      const mock = mockData[i];
      
      if (current.categoriaNome === mock.categoriaNome && 
          current.valorProvisionado === mock.valorProvisionado &&
          current.valorGastoReal === mock.valorGastoReal) {
        continue;
      } else {
        return false;
      }
    }
    
    return true;
  }
}

