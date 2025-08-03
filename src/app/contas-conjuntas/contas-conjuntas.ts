import { Component, OnInit } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { CommonModule } from '@angular/common';

interface ContaConjunta {
  id: number;
  nome: string;
  saldoAtual: number;
  dataApuracao: Date;
  manterSaldoPositivo: boolean;
  usuarioCriadorId: number;
  rateios: Rateio[];
}

interface Rateio {
  id: number;
  percentualRateio: number;
  usuarioId: number;
  usuario: { nome: string; email: string; };
}

@Component({
  selector: 'app-contas-conjuntas',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatListModule,
    MatIconModule
  ],
  templateUrl: './contas-conjuntas.html',
  styleUrls: ['./contas-conjuntas.css']
})
export class ContasConjuntasComponent implements OnInit {
  contasConjuntas: ContaConjunta[] = [];

  ngOnInit() {
    this.loadContasConjuntas();
  }

  loadContasConjuntas() {
    // Aqui você faria uma chamada para a API para carregar as contas conjuntas
    // Por enquanto, vamos usar dados mockados
    this.contasConjuntas = [
      { 
        id: 1, 
        nome: 'Casa Compartilhada', 
        saldoAtual: 250.00, 
        dataApuracao: new Date(), 
        manterSaldoPositivo: true,
        usuarioCriadorId: 1,
        rateios: [
          { id: 1, percentualRateio: 50, usuarioId: 1, usuario: { nome: 'João', email: 'joao@email.com' } },
          { id: 2, percentualRateio: 50, usuarioId: 2, usuario: { nome: 'Maria', email: 'maria@email.com' } }
        ]
      }
    ];
  }

  openDialog() {
    // Aqui você abriria um dialog para criar uma nova conta conjunta
    console.log('Abrir dialog para criar conta conjunta');
  }

  viewDetails(conta: ContaConjunta) {
    // Aqui você abriria um dialog para ver os detalhes da conta conjunta
    console.log('Ver detalhes da conta conjunta:', conta);
  }

  apurarConta(id: number) {
    // Aqui você faria uma chamada para a API para apurar a conta conjunta
    console.log('Apurar conta conjunta:', id);
  }

  deleteConta(id: number) {
    // Aqui você faria uma chamada para a API para deletar a conta conjunta
    this.contasConjuntas = this.contasConjuntas.filter(c => c.id !== id);
    console.log('Conta conjunta deletada:', id);
  }
}

