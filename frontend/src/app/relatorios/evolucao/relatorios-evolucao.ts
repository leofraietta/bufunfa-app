import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatInputModule } from '@angular/material/input';
import { MatNativeDateModule } from '@angular/material/core';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatTooltipModule } from '@angular/material/tooltip';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api.service';

interface DadosEvolucao {
  periodo: string;
  receitas: number;
  despesas: number;
  saldo: number;
  saldoAcumulado: number;
}

@Component({
  selector: 'app-relatorios-evolucao',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    MatFormFieldModule,
    MatDatepickerModule,
    MatInputModule,
    MatNativeDateModule,
    MatButtonToggleModule,
    MatTooltipModule,
    FormsModule
  ],
  templateUrl: './relatorios-evolucao.html',
  styleUrls: ['./relatorios-evolucao.scss']
})
export class RelatoriosEvolucaoComponent implements OnInit {
  loading = false;
  dadosEvolucao: DadosEvolucao[] = [];
  filtros = {
    dataInicio: new Date(new Date().getFullYear(), 0, 1), // Janeiro do ano atual
    dataFim: new Date(),
    contaId: null,
    periodo: 'mensal' // mensal, trimestral, anual
  };
  contas: any[] = [];

  constructor(private apiService: ApiService) {}

  ngOnInit() {
    this.carregarContas();
    this.gerarRelatorio();
  }

  carregarContas() {
    this.apiService.getContas().subscribe({
      next: (contas) => {
        this.contas = contas;
      },
      error: (error) => {
        console.error('Erro ao carregar contas:', error);
      }
    });
  }

  gerarRelatorio() {
    this.loading = true;
    
    // Simulação de dados de evolução temporal
    // Em uma implementação real, isso viria da API
    setTimeout(() => {
      this.dadosEvolucao = this.gerarDadosSimulados();
      this.loading = false;
    }, 1000);
  }

  private gerarDadosSimulados(): DadosEvolucao[] {
    const dados: DadosEvolucao[] = [];
    const meses = ['Jan', 'Fev', 'Mar', 'Abr', 'Mai', 'Jun', 'Jul', 'Ago', 'Set', 'Out', 'Nov', 'Dez'];
    let saldoAcumulado = 0;

    for (let i = 0; i < 12; i++) {
      const receitas = Math.random() * 5000 + 3000; // Entre 3000 e 8000
      const despesas = Math.random() * 4000 + 2000; // Entre 2000 e 6000
      const saldo = receitas - despesas;
      saldoAcumulado += saldo;

      dados.push({
        periodo: meses[i],
        receitas,
        despesas,
        saldo,
        saldoAcumulado
      });
    }

    return dados;
  }

  getTotalReceitas(): number {
    return this.dadosEvolucao.reduce((sum, item) => sum + item.receitas, 0);
  }

  getTotalDespesas(): number {
    return this.dadosEvolucao.reduce((sum, item) => sum + item.despesas, 0);
  }

  getSaldoTotal(): number {
    return this.getTotalReceitas() - this.getTotalDespesas();
  }

  getSaldoFinal(): number {
    return this.dadosEvolucao.length > 0 ? 
      this.dadosEvolucao[this.dadosEvolucao.length - 1].saldoAcumulado : 0;
  }

  getMediaReceitas(): number {
    return this.dadosEvolucao.length > 0 ? this.getTotalReceitas() / this.dadosEvolucao.length : 0;
  }

  getMediaDespesas(): number {
    return this.dadosEvolucao.length > 0 ? this.getTotalDespesas() / this.dadosEvolucao.length : 0;
  }

  exportarRelatorio() {
    // Implementar exportação para CSV/PDF
    console.log('Exportar relatório de evolução temporal');
  }
}
