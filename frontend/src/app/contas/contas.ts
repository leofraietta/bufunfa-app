import { Component, OnInit } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { CommonModule } from '@angular/common';
import { ApiService } from '../services/api';
import { ContaDialogComponent } from './conta-dialog';

interface Conta {
  id: number;
  nome: string;
  tipo: number;
  saldoInicial: number;
  dataFechamento?: Date;
  dataVencimento?: Date;
}

@Component({
  selector: 'app-contas',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatListModule,
    MatIconModule,
    MatDialogModule
  ],
  templateUrl: './contas.html',
  styleUrls: ['./contas.css']
})
export class ContasComponent implements OnInit {
  contas: Conta[] = [];
  isLoading = false;

  constructor(
    private dialog: MatDialog,
    private apiService: ApiService
  ) {}

  ngOnInit() {
    this.loadContas();
  }

  loadContas() {
    this.isLoading = true;
    this.apiService.getContas().subscribe({
      next: (contas) => {
        this.contas = contas;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Erro ao carregar contas:', error);
        this.isLoading = false;
        // Fallback para dados mockados em caso de erro
        this.contas = [
          { id: 1, nome: 'Conta Corrente', tipo: 1, saldoInicial: 1000.00 },
          { id: 2, nome: 'Cartão Visa', tipo: 2, saldoInicial: 0.00 }
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
          console.log('Conta deletada:', id);
          this.loadContas(); // Recarregar lista
        },
        error: (error) => {
          console.error('Erro ao deletar conta:', error);
          // Fallback para remoção local em caso de erro
          this.contas = this.contas.filter(c => c.id !== id);
        }
      });
    }
  }
}

