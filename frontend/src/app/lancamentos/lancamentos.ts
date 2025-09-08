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
  isLoading = false;
  error: string | null = null;
  displayedColumns: string[] = ['descricao', 'valor', 'data', 'tipo', 'status', 'acoes'];
  filtroTipo: string = 'todos';
  filtroStatus: string = 'todos';

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
        this.aplicarFiltros();
        this.isLoading = false;
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
            tipoRecorrencia: 1, // TipoRecorrencia.Esporadica
            contaId: 1,
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

    // Filtro por status
    if (this.filtroStatus !== 'todos') {
      switch (this.filtroStatus) {
        case 'realizados':
          lancamentos = lancamentos.filter(l => l.realizado);
          break;
        case 'pendentes':
          lancamentos = lancamentos.filter(l => !l.realizado && !l.cancelado);
          break;
        case 'cancelados':
          lancamentos = lancamentos.filter(l => l.cancelado);
          break;
      }
    }

    // Ordenar por data
    this.lancamentosFiltrados = lancamentos.sort((a, b) => 
      new Date(b.dataInicial).getTime() - new Date(a.dataInicial).getTime()
    );
  }

  onFiltroChange() {
    this.aplicarFiltros();
  }

  getTipoLancamentoText(tipo: number): string {
    return this.apiService.getTipoLancamentoText(tipo);
  }

  getTipoRecorrenciaText(tipo: number): string {
    return this.apiService.getTipoRecorrenciaText(tipo);
  }

  getTipoLancamentoIcon(tipo: number): string {
    return tipo === 1 ? 'trending_up' : 'trending_down';
  }

  getTipoLancamentoColor(tipo: number): string {
    return tipo === 1 ? 'primary' : 'warn';
  }

  getStatusText(lancamento: Lancamento): string {
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
}

