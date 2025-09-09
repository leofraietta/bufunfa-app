import { Component, OnInit } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { CommonModule } from '@angular/common';
import { ApiService, Lancamento } from '../services/api.service';
import { LancamentoDialogComponent } from './lancamento-dialog';

@Component({
  selector: 'app-lancamentos',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatListModule,
    MatIconModule,
    MatDialogModule,
    MatTableModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatChipsModule,
    MatTooltipModule,
    MatFormFieldModule,
    MatSelectModule,
    MatInputModule
  ],
  templateUrl: './lancamentos.html',
  styleUrls: ['./lancamentos.scss']
})
export class LancamentosComponent implements OnInit {
  lancamentos: Lancamento[] = [];
  lancamentosFiltrados: Lancamento[] = [];
  lancamentosAgrupados: any[] = [];
  categorias: any[] = [];
  isLoading = false;
  error: string | null = null;
  displayedColumns: string[] = ['descricao', 'valor', 'data', 'tipo', 'acoes'];
  filtroTipo: string = 'todos';
  filtroRecorrencia: string = 'todos';

  constructor(
    private dialog: MatDialog,
    private apiService: ApiService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit() {
    this.loadLancamentos();
  }

  loadLancamentos() {
    this.isLoading = true;
    this.error = null;
    this.apiService.getLancamentos().subscribe({
      next: (lancamentos) => {
        this.lancamentos = lancamentos;
        this.loadCategorias();
      },
      error: (error) => {
        console.error('Erro ao carregar lançamentos:', error);
        this.error = 'Erro ao carregar lançamentos. Usando dados de exemplo.';
        this.isLoading = false;
        // Fallback para dados mockados em caso de erro
        this.lancamentos = [
          { 
            id: 1, 
            descricao: 'Salário', 
            valor: 3000.00,
            valorProvisionado: 3000.00,
            dataInicial: new Date(), 
            tipo: 1, // TipoLancamento.Receita
            tipoRecorrencia: 2, // TipoRecorrencia.Recorrente
            contaId: 1,
            categoriaId: 1, // Alimentação
            realizado: true,
            cancelado: false,
            quitado: false
          },
          { 
            id: 2, 
            descricao: 'Supermercado', 
            valor: 150.00,
            valorProvisionado: 200.00,
            dataInicial: new Date(), 
            tipo: 2, // TipoLancamento.Despesa
            tipoRecorrencia: 2, // TipoRecorrencia.Recorrente
            contaId: 1,
            categoriaId: 1, // Alimentação
            realizado: false,
            cancelado: false,
            quitado: false
          },
          { 
            id: 3, 
            descricao: 'Gasolina', 
            valor: 200.00,
            valorProvisionado: 180.00,
            dataInicial: new Date(), 
            tipo: 2, // TipoLancamento.Despesa
            tipoRecorrencia: 2, // TipoRecorrencia.Recorrente
            contaId: 1,
            categoriaId: 2, // Transporte
            realizado: false,
            cancelado: false,
            quitado: false
          },
          { 
            id: 4, 
            descricao: 'Aluguel', 
            valor: 1200.00,
            valorProvisionado: 1200.00,
            dataInicial: new Date(), 
            tipo: 2, // TipoLancamento.Despesa
            tipoRecorrencia: 2, // TipoRecorrencia.Recorrente
            contaId: 1,
            categoriaId: 3, // Moradia
            realizado: true,
            cancelado: false,
            quitado: false
          },
          { 
            id: 5, 
            descricao: 'Plano de Saúde', 
            valor: 350.00,
            valorProvisionado: 350.00,
            dataInicial: new Date(), 
            tipo: 2, // TipoLancamento.Despesa
            tipoRecorrencia: 2, // TipoRecorrencia.Recorrente
            contaId: 1,
            categoriaId: 4, // Saúde
            realizado: false,
            cancelado: false,
            quitado: false
          },
          { 
            id: 6, 
            descricao: 'Curso Online', 
            valor: 99.00,
            valorProvisionado: 99.00,
            dataInicial: new Date(), 
            tipo: 2, // TipoLancamento.Despesa
            tipoRecorrencia: 3, // TipoRecorrencia.Parcelado
            contaId: 1,
            categoriaId: 5, // Educação
            realizado: false,
            cancelado: false,
            quitado: false
          }
        ];
        this.aplicarFiltros();
      }
    });
  }

  openDialog() {
    const dialogRef = this.dialog.open(LancamentoDialogComponent, {
      width: '700px',
      maxHeight: '90vh',
      data: { isEdit: false }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.snackBar.open('Lançamento criado com sucesso', 'Fechar', { duration: 3000 });
        this.loadLancamentos();
      }
    });
  }

  editLancamento(lancamento: Lancamento) {
    const dialogRef = this.dialog.open(LancamentoDialogComponent, {
      width: '700px',
      maxHeight: '90vh',
      data: { lancamento, isEdit: true }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.snackBar.open('Lançamento atualizado com sucesso', 'Fechar', { duration: 3000 });
        this.loadLancamentos();
      }
    });
  }

  deleteLancamento(id: number) {
    if (confirm('Tem certeza que deseja excluir este lançamento?')) {
      this.apiService.deleteLancamento(id).subscribe({
        next: () => {
          this.snackBar.open('Lançamento excluído com sucesso', 'Fechar', { duration: 3000 });
          this.loadLancamentos();
        },
        error: (error) => {
          console.error('Erro ao deletar lançamento:', error);
          this.snackBar.open('Erro ao excluir lançamento', 'Fechar', { duration: 3000 });
          // Fallback para remoção local em caso de erro
          this.lancamentos = this.lancamentos.filter(l => l.id !== id);
          this.aplicarFiltros();
        }
      });
    }
  }

  aplicarFiltros() {
    let lancamentos = [...this.lancamentos];

    // Filtro por tipo
    if (this.filtroTipo !== 'todos') {
      const tipoFiltro = this.filtroTipo === 'receitas' ? 1 : 2;
      lancamentos = lancamentos.filter(l => l.tipo === tipoFiltro);
    }

    // Filtro por tipo de recorrência
    if (this.filtroRecorrencia !== 'todos') {
      switch (this.filtroRecorrencia) {
        case 'recorrente':
          lancamentos = lancamentos.filter(l => 
            l.tipoRecorrencia === 2 || 
            (typeof l.tipoRecorrencia === 'string' && (l.tipoRecorrencia as string).toLowerCase() === 'recorrente')
          );
          break;
        case 'parcelado':
          lancamentos = lancamentos.filter(l => 
            l.tipoRecorrencia === 3 || 
            (typeof l.tipoRecorrencia === 'string' && (l.tipoRecorrencia as string).toLowerCase() === 'parcelado')
          );
          break;
      }
    }

    // Ordenar por data
    this.lancamentosFiltrados = lancamentos.sort((a, b) => 
      new Date(b.dataInicial).getTime() - new Date(a.dataInicial).getTime()
    );
    this.groupLancamentosByCategory();
    console.log('Lançamentos filtrados:', this.lancamentosFiltrados);
  }

  onFiltroChange() {
    this.aplicarFiltros();
  }

  getTipoLancamentoText(tipo: any): string {
    // Se for string, converter para número
    if (typeof tipo === 'string') {
      switch (tipo.toLowerCase()) {
        case 'receita': return 'Receita';
        case 'despesa': return 'Despesa';
        default: return tipo;
      }
    }
    // Se for número, usar o serviço
    return this.apiService.getTipoLancamentoText(tipo);
  }

  getTipoRecorrenciaText(tipo: number | string): string {
    if (typeof tipo === 'string') {
      return tipo;
    }
    return this.apiService.getTipoRecorrenciaText(tipo);
  }

  getTipoRecorrenciaIcon(tipo: number | string): string {
    if (typeof tipo === 'string') {
      switch (tipo.toLowerCase()) {
        case 'recorrente': return 'repeat';
        case 'parcelado': return 'payment';
        default: return 'help_outline';
      }
    }
    
    switch (tipo) {
      case 2: return 'repeat'; // Recorrente
      case 3: return 'payment'; // Parcelado
      default: return 'help_outline';
    }
  }

  getCategoriaName(categoriaId: number | null): string {
    if (!categoriaId) return '';
    const categoria = this.categorias.find(c => c.id === categoriaId);
    return categoria ? categoria.nome : '';
  }

  getCategoriaColor(categoriaId: number | null): string {
    if (!categoriaId) return '#2196f3';
    
    const colors = [
      '#4caf50', // Verde
      '#ff9800', // Laranja  
      '#2196f3', // Azul
      '#f44336', // Vermelho
      '#9c27b0', // Roxo
      '#00bcd4', // Ciano
      '#ff5722', // Laranja escuro
      '#795548', // Marrom
      '#607d8b', // Azul acinzentado
      '#e91e63'  // Rosa
    ];
    
    const colorIndex = (categoriaId - 1) % colors.length;
    return colors[colorIndex];
  }

  getTipoLancamentoIcon(tipo: number): string {
    return tipo === 1 ? 'trending_up' : 'trending_down';
  }

  getTipoLancamentoColor(tipo: number): string {
    return tipo === 1 ? 'primary' : 'warn';
  }

  getStatusText(lancamento: Lancamento): string {
    // Verificar se tem campo status do backend
    if (lancamento.status) {
      if (typeof lancamento.status === 'string') {
        return lancamento.status;
      }
      // Mapear status numérico
      switch (lancamento.status) {
        case 1: return 'Provisional';
        case 2: return 'Realizado';
        case 3: return 'Cancelado';
        case 4: return 'Quitado';
        default: return 'Desconhecido';
      }
    }
    
    // Fallback para propriedades booleanas
    if (lancamento.cancelado) return 'Cancelado';
    if (lancamento.realizado) return 'Realizado';
    if (lancamento.quitado) return 'Quitado';
    return 'Pendente';
  }

  getStatusColor(lancamento: Lancamento): string {
    if (lancamento.cancelado) return 'warn';
    if (lancamento.realizado) return 'primary';
    if (lancamento.quitado) return 'accent';
    return '';
  }

  getLancamentosRealizados(): number {
    return this.lancamentos.filter(l => l.realizado).length;
  }

  getTotalReceitas(): number {
    return this.lancamentos
      .filter(l => l.tipo === 1 && l.realizado)
      .reduce((total, l) => total + l.valor, 0);
  }

  getTotalDespesas(): number {
    return this.lancamentos
      .filter(l => l.tipo === 2 && l.realizado)
      .reduce((total, l) => total + l.valor, 0);
  }

  loadCategorias() {
    this.apiService.getCategorias().subscribe({
      next: (categorias) => {
        this.categorias = categorias;
        this.aplicarFiltros();
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Erro ao carregar categorias:', error);
        // Fallback para dados mockados
        this.categorias = [
          { id: 1, nome: 'Alimentação' },
          { id: 2, nome: 'Transporte' },
          { id: 3, nome: 'Moradia' },
          { id: 4, nome: 'Saúde' },
          { id: 5, nome: 'Educação' }
        ];
        this.aplicarFiltros();
        this.isLoading = false;
      }
    });
  }

  groupLancamentosByCategory() {
    // Ordenar lançamentos por categoria e depois por descrição
    this.lancamentosFiltrados.sort((a, b) => {
      const categoriaA = this.getCategoriaName(a.categoriaId || null) || 'Sem categoria';
      const categoriaB = this.getCategoriaName(b.categoriaId || null) || 'Sem categoria';
      
      if (categoriaA !== categoriaB) {
        return categoriaA.localeCompare(categoriaB);
      }
      
      return a.descricao.localeCompare(b.descricao);
    });

    // Agrupar lançamentos por categoria para exibição com rowspan
    const grupos: { [key: string]: Lancamento[] } = {};
    
    this.lancamentosFiltrados.forEach(lancamento => {
      const categoriaNome = this.getCategoriaName(lancamento.categoriaId || null) || 'Sem categoria';
      if (!grupos[categoriaNome]) {
        grupos[categoriaNome] = [];
      }
      grupos[categoriaNome].push(lancamento);
    });

    // Converter para array com informações de rowspan
    this.lancamentosAgrupados = [];
    Object.keys(grupos).forEach(categoriaNome => {
      const lancamentosCategoria = grupos[categoriaNome];
      lancamentosCategoria.forEach((lancamento, index) => {
        this.lancamentosAgrupados.push({
          ...lancamento,
          categoriaNome,
          categoriaColor: this.getCategoriaColor(lancamento.categoriaId || null),
          isFirstInCategory: index === 0,
          categoryRowspan: index === 0 ? lancamentosCategoria.length : 0
        });
      });
    });
  }

  realizarLancamento(lancamento: Lancamento) {
    const valorReal = prompt(`Valor real para "${lancamento.descricao}":`, lancamento.valor.toString());
    
    if (valorReal !== null) {
      const valor = parseFloat(valorReal);
      if (!isNaN(valor)) {
        this.apiService.realizarLancamento(lancamento.id, valor).subscribe({
          next: () => {
            this.snackBar.open('Lançamento realizado com sucesso', 'Fechar', { duration: 3000 });
            this.loadLancamentos();
          },
          error: (error: any) => {
            console.error('Erro ao realizar lançamento:', error);
            this.snackBar.open('Erro ao realizar lançamento', 'Fechar', { duration: 3000 });
          }
        });
      }
    }
  }

  getDarkerColor(hexColor: string): string {
    // Remove # if present
    const color = hexColor.replace('#', '');
    
    // Convert hex to RGB
    const r = parseInt(color.substr(0, 2), 16);
    const g = parseInt(color.substr(2, 2), 16);
    const b = parseInt(color.substr(4, 2), 16);
    
    // Darken by 30%
    const darkerR = Math.floor(r * 0.7);
    const darkerG = Math.floor(g * 0.7);
    const darkerB = Math.floor(b * 0.7);
    
    // Convert back to hex
    const toHex = (n: number) => n.toString(16).padStart(2, '0');
    return `#${toHex(darkerR)}${toHex(darkerG)}${toHex(darkerB)}`;
  }

  getFormattedDate(dateValue: any): string {
    if (!dateValue) return '';
    
    try {
      let date: Date;
      
      if (typeof dateValue === 'string') {
        // Verificar se é formato brasileiro: dd/MM/yyyy HH:mm:ss
        const brazilianDateRegex = /^(\d{2})\/(\d{2})\/(\d{4})\s+(\d{2}):(\d{2}):(\d{2})$/;
        const match = dateValue.match(brazilianDateRegex);
        
        if (match) {
          // Extrair componentes da data brasileira
          const [, day, month, year, hour, minute, second] = match;
          // Converter para formato ISO: yyyy-MM-ddTHH:mm:ss
          const isoString = `${year}-${month}-${day}T${hour}:${minute}:${second}`;
          date = new Date(isoString);
        } else {
          // Tentar conversão padrão
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
}

