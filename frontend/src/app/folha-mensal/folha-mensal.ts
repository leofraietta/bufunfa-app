import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
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
  expanded?: boolean;
}

@Component({
  selector: 'app-folha-mensal',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatSelectModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonToggleModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    MatTableModule,
    MatSnackBarModule
  ],
  templateUrl: './folha-mensal.html',
  styleUrls: ['./folha-mensal.scss']
})
export class FolhaMensalComponent implements OnInit {
  mesAnoSelecionado: string = '';
  contaSelecionada: number = 0;
  
  folhaAtual: FolhaMensal | null = null;
  contas: Conta[] = [];
  lancamentos: Lancamento[] = [];
  categoriasAgrupadas: CategoriaAgrupada[] = [];
  lancamentosEsporadicos: Lancamento[] = [];
  
  // Modal para realizar lançamento
  showRealizarModal: boolean = false;
  lancamentoSelecionado: Lancamento | null = null;
  valorRealInput: string = '';
  valorRealNumerico: number | null = null;
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
    // Força criação de dados mock para teste
    this.criarDadosMockados();
    this.agruparPorCategorias();
  }

  carregarContas() {
    console.log('🔄 Iniciando carregamento de contas...');
    this.isLoading = true;
    this.apiService.getContas().subscribe({
      next: (contas) => {
        console.log('✅ Contas carregadas da API:', contas);
        console.log('📊 Número de contas:', contas.length);
        console.log('📋 Detalhes das contas:', JSON.stringify(contas, null, 2));
        
        this.contas = contas;
        if (contas.length > 0) {
          this.contaSelecionada = contas[0].id;
          console.log('🎯 Conta selecionada:', this.contaSelecionada, '- Nome:', contas[0].nome);
          this.carregarFolhaMensal();
        }
        this.isLoading = false;
      },
      error: (error) => {
        console.error('❌ Erro ao carregar contas da API:', error);
        console.log('🔧 Usando dados mockados para desenvolvimento');
        this.isLoading = false;
        this.criarDadosMockados();
      }
    });
  }

  carregarFolhaMensal() {
    if (this.contaSelecionada === 0) return;

    console.log('🔄 Carregando folha mensal para conta:', this.contaSelecionada, 'mês:', this.mesAnoSelecionado);
    this.isLoading = true;
    this.apiService.getFolhaMensal(this.contaSelecionada, this.mesAnoSelecionado).subscribe({
      next: (folha) => {
        console.log('✅ Folha mensal carregada da API:', folha);
        console.log('📊 Lançamentos na folha:', (folha as any).lancamentosFolha?.length || 0);
        console.log('📋 Detalhes da folha:', JSON.stringify(folha, null, 2));
        
        this.folhaAtual = folha;
        // Usar lancamentosFolha em vez de lancamentos
        this.lancamentos = (folha as any).lancamentosFolha || [];
        console.log('💾 Lançamentos armazenados no componente:', this.lancamentos);
        this.agruparPorCategorias();
        this.isLoading = false;
      },
      error: (error) => {
        console.error('❌ Erro ao carregar folha mensal da API:', error);
        console.log('🔧 Usando dados mockados para desenvolvimento');
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
    console.log('🔄 Iniciando agrupamento por categorias...');
    console.log('📊 Lançamentos para agrupar:', this.lancamentos.length);
    
    const categoriasMap = new Map<string, CategoriaAgrupada>();
    
    this.lancamentos.forEach(lancamento => {
      console.log('🏷️ Processando lançamento:', lancamento.descricao, 'Categoria ID:', lancamento.categoriaId);
      
      // Usar o nome da categoria da API ou fallback
      const lancamentoAPI = lancamento as any;
      const nomeCategoria = lancamentoAPI.categoria?.nome || this.getNomeCategoria(lancamento.categoriaId || 0);
      
      if (!categoriasMap.has(nomeCategoria)) {
        categoriasMap.set(nomeCategoria, {
          nome: nomeCategoria,
          valorProvisionado: 0,
          valorReal: 0,
          diferenca: 0,
          lancamentos: [],
          expanded: true
        });
        console.log('➕ Nova categoria criada:', nomeCategoria);
      }
      
      const categoria = categoriasMap.get(nomeCategoria)!;
      categoria.lancamentos.push(lancamento);
      categoria.valorProvisionado += lancamento.valorProvisionado || lancamentoAPI.valorEfetivo || 0;
      
      if (lancamento.realizado) {
        categoria.valorReal += lancamentoAPI.valorEfetivo || 0;
      }
    });
    
    // Calcular diferenças
    categoriasMap.forEach(categoria => {
      categoria.diferenca = categoria.valorReal - categoria.valorProvisionado;
    });
    
    this.categoriasAgrupadas = Array.from(categoriasMap.values());
    console.log('✅ Categorias agrupadas:', this.categoriasAgrupadas.length);
    console.log('📋 Detalhes das categorias:', JSON.stringify(this.categoriasAgrupadas, null, 2));
    
    // Se não há dados reais, criar mock
    if (this.categoriasAgrupadas.length === 0) {
      console.log('⚠️ Nenhuma categoria encontrada, criando dados mock...');
      this.criarCategoriasMock();
    }
  }

  private getNomeCategoria(categoriaId: number): string {
    // Implementar busca real por categoria
    const nomes: { [key: number]: string } = {
      1: 'Alimentação',
      2: 'Transporte', 
      3: 'Moradia',
      4: 'Saúde',
      5: 'Educação'
    };
    return nomes[categoriaId] || `Categoria ${categoriaId}`;
  }

  private criarCategoriasMock() {
    console.log('Criando categorias mock com lançamentos:', this.lancamentos);
    this.categoriasAgrupadas = [
      {
        nome: 'Alimentação',
        valorProvisionado: 500.00,
        valorReal: 450.00,
        diferenca: -50.00,
        lancamentos: this.lancamentos.filter(l => l.tipo === 2).slice(0, 2),
        expanded: true
      },
      {
        nome: 'Moradia',
        valorProvisionado: 800.00,
        valorReal: 800.00,
        diferenca: 0.00,
        lancamentos: this.lancamentos.filter(l => l.tipo === 2).slice(2),
        expanded: true
      }
    ];
    console.log('Categorias criadas:', this.categoriasAgrupadas);
  }

  onMesAnoChange() {
    this.carregarFolhaMensal();
  }

  onContaChange() {
    this.carregarFolhaMensal();
  }

  // Métodos para o modal de realizar lançamento
  abrirModalRealizarLancamento(lancamento: Lancamento) {
    this.lancamentoSelecionado = lancamento;
    this.valorRealInput = (lancamento.valorProvisionado || lancamento.valor || 0).toFixed(2).replace('.', ',');
    this.valorRealNumerico = lancamento.valorProvisionado || lancamento.valor || 0;
    this.showRealizarModal = true;
  }

  fecharModalRealizar() {
    this.showRealizarModal = false;
    this.lancamentoSelecionado = null;
    this.valorRealInput = '';
    this.valorRealNumerico = null;
  }

  formatarValorReal(event: any) {
    let value = event.target.value;
    
    // Remove tudo que não é número, vírgula ou ponto
    value = value.replace(/[^\d,\.]/g, '');
    
    // Substitui ponto por vírgula
    value = value.replace('.', ',');
    
    // Permite apenas uma vírgula
    const parts = value.split(',');
    if (parts.length > 2) {
      value = parts[0] + ',' + parts.slice(1).join('');
    }
    
    // Limita casas decimais a 2
    if (parts[1] && parts[1].length > 2) {
      value = parts[0] + ',' + parts[1].substring(0, 2);
    }
    
    this.valorRealInput = value;
    
    // Converte para número
    if (value) {
      const numericValue = parseFloat(value.replace(',', '.'));
      this.valorRealNumerico = isNaN(numericValue) ? null : numericValue;
    } else {
      this.valorRealNumerico = null;
    }
  }

  getDiferenca(): number {
    if (!this.lancamentoSelecionado || this.valorRealNumerico === null) return 0;
    const valorProvisionado = this.lancamentoSelecionado.valorProvisionado || this.lancamentoSelecionado.valor || 0;
    return this.valorRealNumerico - valorProvisionado;
  }

  confirmarRealizacao() {
    if (!this.lancamentoSelecionado || this.valorRealNumerico === null) return;
    
    this.apiService.realizarLancamento(this.lancamentoSelecionado.id, this.valorRealNumerico).subscribe({
      next: () => {
        this.snackBar.open('Lançamento realizado com sucesso', 'Fechar', { duration: 3000 });
        this.fecharModalRealizar();
        this.carregarFolhaMensal();
      },
      error: (error: any) => {
        console.error('Erro ao realizar lançamento:', error);
        this.snackBar.open('Erro ao realizar lançamento', 'Fechar', { duration: 3000 });
      }
    });
  }

  // Métodos auxiliares para o template
  toggleCategoria(categoria: CategoriaAgrupada) {
    categoria.expanded = !categoria.expanded;
  }

  getPendentesCategoria(categoria: CategoriaAgrupada): number {
    return categoria.lancamentos.filter(l => !l.realizado && !l.cancelado).length;
  }

  getTotalLancamentosCategorias(): number {
    return this.categoriasAgrupadas.reduce((total, cat) => total + cat.lancamentos.length, 0);
  }

  getStatusText(lancamento: Lancamento): string {
    if (lancamento.realizado) return 'Realizado';
    if (lancamento.cancelado) return 'Cancelado';
    return 'Pendente';
  }

  getFormattedDate(dateValue: any): string {
    if (!dateValue) return '';
    
    try {
      let date: Date;
      
      if (typeof dateValue === 'string') {
        // Handle Brazilian date format
        const brazilianDateRegex = /^(\d{2})\/(\d{2})\/(\d{4})\s+(\d{2}):(\d{2}):(\d{2})$/;
        const match = dateValue.match(brazilianDateRegex);
        
        if (match) {
          const [, day, month, year, hour, minute, second] = match;
          const isoString = `${year}-${month}-${day}T${hour}:${minute}:${second}`;
          date = new Date(isoString);
        } else {
          date = new Date(dateValue);
        }
      } else if (dateValue instanceof Date) {
        date = dateValue;
      } else {
        return '';
      }
      
      if (isNaN(date.getTime())) {
        return '';
      }
      
      const day = date.getDate().toString().padStart(2, '0');
      const month = (date.getMonth() + 1).toString().padStart(2, '0');
      const year = date.getFullYear();
      return `${day}/${month}/${year}`;
    } catch (error) {
      console.warn('Erro ao formatar data:', error);
      return '';
    }
  }

  trackByCategoria(index: number, categoria: CategoriaAgrupada): any {
    return categoria.id || categoria.nome;
  }

  trackByLancamento(index: number, lancamento: Lancamento): any {
    return lancamento.id;
  }

  editarLancamento(lancamento: Lancamento) {
    console.log('Editar lançamento:', lancamento);
    // TODO: Implementar dialog de edição
  }

  cancelarLancamento(lancamento: Lancamento) {
    if (confirm(`Tem certeza que deseja cancelar o lançamento "${lancamento.descricao}"?`)) {
      this.apiService.cancelarLancamento(lancamento.id).subscribe({
        next: () => {
          this.snackBar.open('Lançamento cancelado com sucesso', 'Fechar', { duration: 3000 });
          this.carregarFolhaMensal();
        },
        error: (error: any) => {
          console.error('Erro ao cancelar lançamento:', error);
          this.snackBar.open('Erro ao cancelar lançamento', 'Fechar', { duration: 3000 });
        }
      });
    }
  }

  criarNovoLancamento() {
    console.log('Criar novo lançamento');
    // TODO: Implementar navegação para criação de lançamento
  }

  gerenciarCategorias() {
    console.log('Gerenciar categorias');
    // TODO: Implementar navegação para gestão de categorias
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

