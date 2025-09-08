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
import { MatTooltipModule } from '@angular/material/tooltip';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api.service';

@Component({
  selector: 'app-relatorios-categorias',
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
    MatTooltipModule,
    FormsModule
  ],
  templateUrl: './relatorios-categorias.html',
  styleUrls: ['./relatorios-categorias.scss']
})
export class RelatoriosCategoriasComponent implements OnInit {
  loading = false;
  categorias: any[] = [];
  relatorioData: any[] = [];
  filtros = {
    dataInicio: new Date(new Date().getFullYear(), new Date().getMonth(), 1),
    dataFim: new Date(),
    contaId: null
  };
  contas: any[] = [];

  constructor(private apiService: ApiService) {}

  ngOnInit() {
    this.carregarContas();
    this.carregarCategorias();
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

  carregarCategorias() {
    this.apiService.getCategorias().subscribe({
      next: (categorias) => {
        this.categorias = categorias;
      },
      error: (error) => {
        console.error('Erro ao carregar categorias:', error);
      }
    });
  }

  gerarRelatorio() {
    this.loading = true;
    
    // Simulação de dados do relatório por categoria
    // Em uma implementação real, isso viria da API
    setTimeout(() => {
      this.relatorioData = this.categorias.map(categoria => ({
        categoria: categoria.nome,
        valorPrevisto: categoria.valorPrevisto || 0,
        valorRealizado: Math.random() * (categoria.valorPrevisto || 1000),
        percentual: 0,
        transacoes: Math.floor(Math.random() * 20) + 1
      }));

      // Calcular percentuais
      const totalRealizado = this.relatorioData.reduce((sum, item) => sum + item.valorRealizado, 0);
      this.relatorioData.forEach(item => {
        item.percentual = totalRealizado > 0 ? (item.valorRealizado / totalRealizado) * 100 : 0;
      });

      // Ordenar por valor realizado (maior para menor)
      this.relatorioData.sort((a, b) => b.valorRealizado - a.valorRealizado);

      this.loading = false;
    }, 1000);
  }

  getTotalPrevisto(): number {
    return this.relatorioData.reduce((sum, item) => sum + item.valorPrevisto, 0);
  }

  getTotalRealizado(): number {
    return this.relatorioData.reduce((sum, item) => sum + item.valorRealizado, 0);
  }

  getVariacao(): number {
    const previsto = this.getTotalPrevisto();
    const realizado = this.getTotalRealizado();
    return previsto > 0 ? ((realizado - previsto) / previsto) * 100 : 0;
  }

  exportarRelatorio() {
    // Implementar exportação para CSV/PDF
    console.log('Exportar relatório por categoria');
  }
}
