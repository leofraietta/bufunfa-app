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
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { ApiService } from '../services/api';

interface FolhaMensal {
  id: number;
  ano: number;
  mes: number;
  contaId: number;
  conta?: any;
  saldoInicialReal: number;
  saldoInicialProvisionado: number;
  saldoFinalReal: number;
  saldoFinalProvisionado: number;
  totalReceitasReais: number;
  totalReceitasProvisionadas: number;
  totalDespesasReais: number;
  totalDespesasProvisionadas: number;
  lancamentosFolha: LancamentoFolha[];
}

interface LancamentoFolha {
  id: number;
  descricao: string;
  valorProvisionado: number;
  valorReal?: number;
  dataPrevista: Date | string;
  dataRealizacao?: Date | string;
  tipo: number;
  tipoRecorrencia: number;
  parcelaAtual?: number;
  totalParcelas?: number;
  categoriaId?: number;
  categoria?: any;
  realizado: boolean;
  valorEfetivo: number;
  emAtraso: boolean;
  statusDescricao: string;
}

interface Conta {
  id: number;
  nome: string;
  tipo: number;
  saldoInicial: number;
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
    MatDialogModule
  ],
  templateUrl: './folha-mensal.html',
  styleUrls: ['./folha-mensal.css']
})
export class FolhaMensalComponent implements OnInit {
  anoAtual: number = new Date().getFullYear();
  mesAtual: number = new Date().getMonth() + 1;
  contaSelecionada: number = 0;
  
  folhaAtual: FolhaMensal | null = null;
  contas: Conta[] = [];
  lancamentosFiltrados: LancamentoFolha[] = [];
  filtroTipo: string = 'todos';
  
  isLoading = false;

  private readonly mesesNomes = [
    'Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho',
    'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro'
  ];

  constructor(
    private apiService: ApiService,
    private dialog: MatDialog
  ) {}

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
        // Fallback para dados mockados
        this.contas = [
          { id: 1, nome: 'Conta Corrente', tipo: 1, saldoInicial: 1000.00 },
          { id: 2, nome: 'Cartão Visa', tipo: 2, saldoInicial: 0.00 }
        ];
        this.contaSelecionada = this.contas[0].id;
        this.carregarFolhaMensal();
      }
    });
  }

  carregarFolhaMensal() {
    if (this.contaSelecionada === 0) return;

    this.isLoading = true;
    this.apiService.getFolhaMensal(this.contaSelecionada, this.anoAtual, this.mesAtual).subscribe({
      next: (folha) => {
        this.folhaAtual = this.processarDatasLancamentos(folha);
        this.aplicarFiltros();
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Erro ao carregar folha mensal:', error);
        this.isLoading = false;
        // Fallback para dados mockados
        this.criarFolhaMockada();
      }
    });
  }

  private criarFolhaMockada() {
    this.folhaAtual = {
      id: 1,
      ano: this.anoAtual,
      mes: this.mesAtual,
      contaId: this.contaSelecionada,
      saldoInicialReal: 1000.00,
      saldoInicialProvisionado: 1000.00,
      saldoFinalReal: 850.00,
      saldoFinalProvisionado: 800.00,
      totalReceitasReais: 3000.00,
      totalReceitasProvisionadas: 3000.00,
      totalDespesasReais: 2150.00,
      totalDespesasProvisionadas: 2200.00,
      lancamentosFolha: [
        {
          id: 1,
          descricao: 'Salário',
          valorProvisionado: 3000.00,
          valorReal: 3000.00,
          dataPrevista: new Date(this.anoAtual, this.mesAtual - 1, 5),
          dataRealizacao: new Date(this.anoAtual, this.mesAtual - 1, 5),
          tipo: 1,
          tipoRecorrencia: 2,
          realizado: true,
          valorEfetivo: 3000.00,
          emAtraso: false,
          statusDescricao: 'Realizado'
        },
        {
          id: 2,
          descricao: 'Aluguel',
          valorProvisionado: 800.00,
          valorReal: 800.00,
          dataPrevista: new Date(this.anoAtual, this.mesAtual - 1, 10),
          dataRealizacao: new Date(this.anoAtual, this.mesAtual - 1, 10),
          tipo: 2,
          tipoRecorrencia: 2,
          realizado: true,
          valorEfetivo: 800.00,
          emAtraso: false,
          statusDescricao: 'Realizado'
        },
        {
          id: 3,
          descricao: 'Supermercado',
          valorProvisionado: 400.00,
          valorReal: 350.00,
          dataPrevista: new Date(this.anoAtual, this.mesAtual - 1, 15),
          dataRealizacao: new Date(this.anoAtual, this.mesAtual - 1, 15),
          tipo: 2,
          tipoRecorrencia: 2,
          realizado: true,
          valorEfetivo: 350.00,
          emAtraso: false,
          statusDescricao: 'Realizado'
        },
        {
          id: 4,
          descricao: 'Internet',
          valorProvisionado: 100.00,
          dataPrevista: new Date(this.anoAtual, this.mesAtual - 1, 25),
          tipo: 2,
          tipoRecorrencia: 2,
          realizado: false,
          valorEfetivo: 100.00,
          emAtraso: new Date() > new Date(this.anoAtual, this.mesAtual - 1, 25),
          statusDescricao: 'Pendente'
        }
      ]
    };
    this.aplicarFiltros();
  }

  navegarMes(direcao: number) {
    if (direcao > 0) {
      if (this.mesAtual === 12) {
        this.mesAtual = 1;
        this.anoAtual++;
      } else {
        this.mesAtual++;
      }
    } else {
      if (this.mesAtual === 1) {
        this.mesAtual = 12;
        this.anoAtual--;
      } else {
        this.mesAtual--;
      }
    }
    this.carregarFolhaMensal();
  }

  onContaChange() {
    this.carregarFolhaMensal();
  }

  aplicarFiltros() {
    if (!this.folhaAtual) {
      this.lancamentosFiltrados = [];
      return;
    }

    let lancamentos = this.folhaAtual.lancamentosFolha || [];

    switch (this.filtroTipo) {
      case 'receitas':
        lancamentos = lancamentos.filter(l => l.tipo === 1);
        break;
      case 'despesas':
        lancamentos = lancamentos.filter(l => l.tipo === 2);
        break;
      default:
        // 'todos' - não filtra
        break;
    }

    // Ordenar por data prevista
    this.lancamentosFiltrados = lancamentos.sort((a, b) => 
      new Date(a.dataPrevista).getTime() - new Date(b.dataPrevista).getTime()
    );
  }

  realizarLancamento(lancamento: LancamentoFolha) {
    // TODO: Implementar dialog para realização de lançamento
    const valorReal = prompt(`Valor real para "${lancamento.descricao}":`, lancamento.valorProvisionado.toString());
    
    if (valorReal !== null) {
      const valor = parseFloat(valorReal);
      if (!isNaN(valor)) {
        this.apiService.realizarLancamentoFolha(lancamento.id, valor).subscribe({
          next: () => {
            console.log('Lançamento realizado com sucesso');
            this.carregarFolhaMensal(); // Recarregar dados
          },
          error: (error) => {
            console.error('Erro ao realizar lançamento:', error);
            // Fallback para atualização local
            lancamento.valorReal = valor;
            lancamento.realizado = true;
            lancamento.dataRealizacao = new Date();
            lancamento.statusDescricao = 'Realizado';
            this.aplicarFiltros();
          }
        });
      }
    }
  }

  editarLancamento(lancamento: LancamentoFolha) {
    // TODO: Implementar edição de lançamento
    console.log('Editar lançamento:', lancamento);
  }

  abrirSeletorMes() {
    // TODO: Implementar dialog para seleção de mês/ano
    const novoMes = prompt(`Mês (1-12):`, this.mesAtual.toString());
    const novoAno = prompt(`Ano:`, this.anoAtual.toString());
    
    if (novoMes && novoAno) {
      const mes = parseInt(novoMes);
      const ano = parseInt(novoAno);
      
      if (mes >= 1 && mes <= 12 && ano >= 2020 && ano <= 2030) {
        this.mesAtual = mes;
        this.anoAtual = ano;
        this.carregarFolhaMensal();
      }
    }
  }

  getNomeMes(mes: number): string {
    return this.mesesNomes[mes - 1] || 'Mês';
  }

  private processarDatasLancamentos(folha: FolhaMensal): FolhaMensal {
    if (folha.lancamentosFolha) {
      folha.lancamentosFolha = folha.lancamentosFolha.map(lancamento => ({
        ...lancamento,
        dataPrevista: this.converterParaDate(lancamento.dataPrevista),
        dataRealizacao: lancamento.dataRealizacao ? this.converterParaDate(lancamento.dataRealizacao) : undefined
      }));
    }
    return folha;
  }

  private converterParaDate(data: Date | string): Date {
    if (data instanceof Date) {
      return data;
    }
    
    if (typeof data === 'string') {
      // Tenta diferentes formatos de data
      const date = new Date(data);
      if (!isNaN(date.getTime())) {
        return date;
      }
      
      // Se o formato padrão não funcionar, tenta formato brasileiro dd/MM/yyyy
      const partes = data.match(/(\d{2})\/(\d{2})\/(\d{4})/);
      if (partes) {
        const [, dia, mes, ano] = partes;
        return new Date(parseInt(ano), parseInt(mes) - 1, parseInt(dia));
      }
    }
    
    // Fallback para data atual se não conseguir converter
    return new Date();
  }
}

