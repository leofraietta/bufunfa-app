import { Component, OnInit } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { CommonModule } from '@angular/common';

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
    MatIconModule
  ],
  templateUrl: './contas.html',
  styleUrls: ['./contas.css']
})
export class ContasComponent implements OnInit {
  contas: Conta[] = [];

  ngOnInit() {
    this.loadContas();
  }

  loadContas() {
    // Aqui você faria uma chamada para a API para carregar as contas
    // Por enquanto, vamos usar dados mockados
    this.contas = [
      { id: 1, nome: 'Conta Corrente', tipo: 1, saldoInicial: 1000.00 },
      { id: 2, nome: 'Cartão Visa', tipo: 2, saldoInicial: 0.00 }
    ];
  }

  openDialog() {
    // Aqui você abriria um dialog para adicionar uma nova conta
    console.log('Abrir dialog para adicionar conta');
  }

  editConta(conta: Conta) {
    // Aqui você abriria um dialog para editar a conta
    console.log('Editar conta:', conta);
  }

  deleteConta(id: number) {
    // Aqui você faria uma chamada para a API para deletar a conta
    this.contas = this.contas.filter(c => c.id !== id);
    console.log('Conta deletada:', id);
  }
}

