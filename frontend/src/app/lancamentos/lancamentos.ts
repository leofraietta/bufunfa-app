import { Component, OnInit } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { CommonModule } from '@angular/common';
import { ApiService } from '../services/api';
import { LancamentoDialogComponent } from './lancamento-dialog';

interface Lancamento {
  id: number;
  descricao: string;
  valor: number;
  valorProvisionado?: number;
  valorReal?: number;
  data: Date;
  tipo: number;
  tipoRecorrencia: number;
  contaId: number;
  categoriaId?: number;
}

@Component({
  selector: 'app-lancamentos',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatListModule,
    MatIconModule,
    MatDialogModule
  ],
  templateUrl: './lancamentos.html',
  styleUrls: ['./lancamentos.css']
})
export class LancamentosComponent implements OnInit {
  lancamentos: Lancamento[] = [];
  isLoading = false;

  constructor(
    private dialog: MatDialog,
    private apiService: ApiService
  ) {}

  ngOnInit() {
    this.loadLancamentos();
  }

  loadLancamentos() {
    this.isLoading = true;
    this.apiService.getLancamentos().subscribe({
      next: (lancamentos) => {
        this.lancamentos = lancamentos;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Erro ao carregar lançamentos:', error);
        this.isLoading = false;
        // Fallback para dados mockados em caso de erro
        this.lancamentos = [
          { 
            id: 1, 
            descricao: 'Salário', 
            valor: 3000.00,
            valorProvisionado: 3000.00,
            valorReal: 3000.00,
            data: new Date(), 
            tipo: 1, 
            tipoRecorrencia: 2, 
            contaId: 1 
          },
          { 
            id: 2, 
            descricao: 'Supermercado', 
            valor: 150.00,
            valorProvisionado: 200.00,
            valorReal: 150.00,
            data: new Date(), 
            tipo: 2, 
            tipoRecorrencia: 1, 
            contaId: 1 
          }
        ];
      }
    });
  }

  openDialog() {
    const dialogRef = this.dialog.open(LancamentoDialogComponent, {
      width: '600px',
      data: { isEdit: false }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        console.log('Novo lançamento criado:', result);
        this.loadLancamentos(); // Recarregar lista
      }
    });
  }

  editLancamento(lancamento: Lancamento) {
    const dialogRef = this.dialog.open(LancamentoDialogComponent, {
      width: '600px',
      data: { lancamento, isEdit: true }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        console.log('Lançamento atualizado:', result);
        this.loadLancamentos(); // Recarregar lista
      }
    });
  }

  deleteLancamento(id: number) {
    if (confirm('Tem certeza que deseja excluir este lançamento?')) {
      this.apiService.deleteLancamento(id).subscribe({
        next: () => {
          console.log('Lançamento deletado:', id);
          this.loadLancamentos(); // Recarregar lista
        },
        error: (error) => {
          console.error('Erro ao deletar lançamento:', error);
          // Fallback para remoção local em caso de erro
          this.lancamentos = this.lancamentos.filter(l => l.id !== id);
        }
      });
    }
  }

  getTipoRecorrenciaText(tipo: number): string {
    switch(tipo) {
      case 1: return 'Esporádica';
      case 2: return 'Recorrente';
      case 3: return 'Parcelada';
      default: return 'Desconhecido';
    }
  }
}

