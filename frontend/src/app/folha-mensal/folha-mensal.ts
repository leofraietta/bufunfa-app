import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatTableModule } from '@angular/material/table';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ApiService, FolhaMensal, Lancamento, Conta, Categoria } from '../services/api.service';

interface CategoriaAgrupada {
  id?: number;
  nome: string;
  valorProvisionado: number;
  valorReal: number;
  diferenca: number;
  lancamentos: Lancamento[];
}

@Component({
  selector: 'app-folha-mensal',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatSelectModule,
    MatFormFieldModule,
    MatButtonToggleModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    MatTableModule,
    MatSnackBarModule
  ],
  templateUrl: './folha-mensal.html',
  styleUrls: ['./folha-mensal.css']
})
export class FolhaMensalComponent implements OnInit {
  mesAnoSelecionado: string = '';
  contaSelecionada: number = 0;
  
  folhaAtual: FolhaMensal | null = null;
  contas: Conta[] = [];
  lancamentos: Lancamento[] = [];
  categoriasAgrupadas: CategoriaAgrupada[] = [];
  filtroTipo: string = 'todos';
  mesesDisponiveis: { value: string, label: string }[] = [];
  
  isLoading = false;
  error: string | null = null;

  constructor(
    private apiService: ApiService,
    private snackBar: MatSnackBar
  ) {
    this.mesAnoSelecionado = this.apiService.getCurrentMesAno();
    this.mesesDisponiveis = this.apiService.getMesesDisponiveis();
  }

  ngOnInit() {
    this.carregarContas();
  }

  carregarContas() {
    this.isLoading = true;
    this.apiService.getContas().subscribe({
      next: (contas) => {
        this.contas = contas;
        if (contas.length > 0) {
          this.contaSelecionada = contas[0].id;
          this.carregarFolhaMensal();
        }
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Erro ao carregar contas:', error);
        this.isLoading = false;
        this.error = 'Erro ao carregar contas';
        this.criarDadosMockados();
      }
    });
  }

  carregarFolhaMensal() {
    if (this.contaSelecionada === 0) return;

    this.isLoading = true;
    this.apiService.getFolhaMensal(this.contaSelecionada, this.mesAnoSelecionado).subscribe({
      next: (folha) => {
        this.folhaAtual = folha;
        this.lancamentos = folha.lancamentos || [];
        this.agruparPorCategorias();
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Erro ao carregar folha mensal:', error);
        this.isLoading = false;
        this.error = 'Erro ao carregar dados da folha mensal';
        this.criarDadosMockados();
      }
    });
  }

  private criarDadosMockados() {
    this.contas = [
      { id: 1, nome: 'Conta Corrente', tipo: 1, saldoInicial: 1000.00, ativa: true },
      { id: 2, nome: 'Cartão Visa', tipo: 2, saldoInicial: 0.00, ativa: true }
    ];
    this.contaSelecionada = this.contas[0].id;

    this.folhaAtual = {
      id: 1,
      contaId: this.contaSelecionada,
      mesAno: this.mesAnoSelecionado,
      saldoInicial: 1000.00,
      saldoFinal: 850.00,
      totalReceitas: 3000.00,
      totalDespesas: 2150.00,
      totalReceitasProvisionadas: 3000.00,
      totalDespesasProvisionadas: 2200.00,
      status: 1,
      lancamentos: [
        {
          id: 1,
          descricao: 'Salário',
          valor: 3000.00,
          valorProvisionado: 3000.00,
          dataInicial: new Date(),
          tipo: 1,
          tipoRecorrencia: 2,
          contaId: this.contaSelecionada,
          realizado: true,
          cancelado: false,
          quitado: false
        },
        {
          id: 2,
          descricao: 'Aluguel',
          valor: 800.00,
          valorProvisionado: 800.00,
          dataInicial: new Date(),
          tipo: 2,
          tipoRecorrencia: 2,
          contaId: this.contaSelecionada,
          realizado: true,
          cancelado: false,
          quitado: false
        }
      ]
    };

    this.lancamentos = this.folhaAtual.lancamentos;
    this.agruparPorCategorias();
  }

  private agruparPorCategorias() {
    const categorias = new Map<string, CategoriaAgrupada>();

    this.lancamentos.forEach(lancamento => {
      const nomeCategoria = 'Sem Categoria'; // Por enquanto, sem categorias
      
      if (!categorias.has(nomeCategoria)) {
        categorias.set(nomeCategoria, {
          nome: nomeCategoria,
          valorProvisionado: 0,
          valorReal: 0,
          diferenca: 0,
          lancamentos: []
        });
      }

      const categoria = categorias.get(nomeCategoria)!;
      categoria.lancamentos.push(lancamento);
      categoria.valorProvisionado += lancamento.valorProvisionado || 0;
      categoria.valorReal += lancamento.realizado ? lancamento.valor : 0;
    });

    this.categoriasAgrupadas = Array.from(categorias.values()).map(categoria => ({
      ...categoria,
      diferenca: categoria.valorReal - categoria.valorProvisionado
    }));
  }

  onMesAnoChange() {
    this.carregarFolhaMensal();
  }

  onContaChange() {
    this.carregarFolhaMensal();
  }

  realizarLancamento(lancamento: Lancamento) {
    const valorReal = prompt(`Valor real para "${lancamento.descricao}":`, (lancamento.valorProvisionado || lancamento.valor).toString());
    
    if (valorReal !== null) {
      const valor = parseFloat(valorReal);
      if (!isNaN(valor)) {
        this.apiService.realizarLancamento(lancamento.id, valor).subscribe({
          next: () => {
            this.snackBar.open('Lançamento realizado com sucesso', 'Fechar', { duration: 3000 });
            this.carregarFolhaMensal();
          },
          error: (error: any) => {
            console.error('Erro ao realizar lançamento:', error);
            this.snackBar.open('Erro ao realizar lançamento', 'Fechar', { duration: 3000 });
          }
        });
      }
    }
  }

  editarLancamento(lancamento: Lancamento) {
    console.log('Editar lançamento:', lancamento);
    // TODO: Implementar dialog de edição
  }

  getTipoLancamentoText(tipo: number): string {
    return this.apiService.getTipoLancamentoText(tipo);
  }

  getTipoRecorrenciaText(tipo: number): string {
    return this.apiService.getTipoRecorrenciaText(tipo);
  }

  formatMesAno(mesAno: string): string {
    return this.apiService.formatMesAno(mesAno);
  }

  get saldoInicial(): number {
    return this.folhaAtual?.saldoInicial || 0;
  }

  get saldoFinal(): number {
    return this.folhaAtual?.saldoFinal || 0;
  }

  get totalReceitas(): number {
    return this.folhaAtual?.totalReceitas || 0;
  }

  get totalDespesas(): number {
    return this.folhaAtual?.totalDespesas || 0;
  }

  get totalReceitasProvisionadas(): number {
    return this.folhaAtual?.totalReceitasProvisionadas || 0;
  }

  get totalDespesasProvisionadas(): number {
    return this.folhaAtual?.totalDespesasProvisionadas || 0;
  }
}

