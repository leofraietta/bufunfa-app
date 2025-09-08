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
import { CommonModule } from '@angular/common';
import { ApiService, Conta } from '../services/api.service';
import { ContaDialogComponent } from './conta-dialog';

@Component({
  selector: 'app-contas',
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
    MatTooltipModule
  ],
  templateUrl: './contas.html',
  styleUrls: ['./contas.scss']
})
export class ContasComponent implements OnInit {
  contas: Conta[] = [];
  isLoading = false;
  error: string | null = null;
  displayedColumns: string[] = ['nome', 'tipo', 'saldoInicial', 'status', 'acoes'];

  constructor(
    private dialog: MatDialog,
    private apiService: ApiService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit() {
    this.loadContas();
  }

  loadContas() {
    this.isLoading = true;
    this.error = null;
    this.apiService.getContas().subscribe({
      next: (contas) => {
        this.contas = contas;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Erro ao carregar contas:', error);
        this.error = 'Erro ao carregar contas. Usando dados de exemplo.';
        this.isLoading = false;
        // Fallback para dados mockados em caso de erro
        this.contas = [
          { 
            id: 1, 
            nome: 'Conta Corrente', 
            tipo: 1, // TipoConta.ContaCorrente
            saldoInicial: 1000.00,
            ativa: true
          },
          { 
            id: 2, 
            nome: 'Cartão Visa', 
            tipo: 2, // TipoConta.ContaCartaoCredito
            saldoInicial: 0.00,
            ativa: true,
            dataFechamento: 5,
            dataVencimento: 15
          }
        ];
      }
    });
  }

  openDialog() {
    const dialogRef = this.dialog.open(ContaDialogComponent, {
      width: '500px',
      data: { isEdit: false }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        console.log('Nova conta criada:', result);
        this.loadContas(); // Recarregar lista
      }
    });
  }

  editConta(conta: Conta) {
    const dialogRef = this.dialog.open(ContaDialogComponent, {
      width: '500px',
      data: { conta, isEdit: true }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        console.log('Conta atualizada:', result);
        this.loadContas(); // Recarregar lista
      }
    });
  }

  deleteConta(id: number) {
    if (confirm('Tem certeza que deseja excluir esta conta?')) {
      this.apiService.deleteConta(id).subscribe({
        next: () => {
          this.snackBar.open('Conta excluída com sucesso', 'Fechar', { duration: 3000 });
          this.loadContas(); // Recarregar lista
        },
        error: (error) => {
          console.error('Erro ao deletar conta:', error);
          this.snackBar.open('Erro ao excluir conta', 'Fechar', { duration: 3000 });
          // Fallback para remoção local em caso de erro
          this.contas = this.contas.filter(c => c.id !== id);
        }
      });
    }
  }

  getTipoContaText(tipo: number): string {
    return this.apiService.getTipoContaText(tipo);
  }

  getTipoContaIcon(tipo: number): string {
    switch (tipo) {
      case 1: return 'account_balance'; // ContaCorrente
      case 2: return 'credit_card'; // ContaCartaoCredito
      default: return 'account_balance_wallet';
    }
  }

  getStatusColor(ativa: boolean): string {
    return ativa ? 'primary' : 'warn';
  }

  getStatusText(ativa: boolean): string {
    return ativa ? 'Ativa' : 'Inativa';
  }

  getContasAtivas(): number {
    return this.contas.filter(conta => conta.ativa).length;
  }

  getTotalSaldoInicial(): number {
    return this.contas.reduce((total, conta) => total + conta.saldoInicial, 0);
  }
}

