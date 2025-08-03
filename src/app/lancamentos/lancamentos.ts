import { Component, OnInit } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { CommonModule } from '@angular/common';

interface Lancamento {
  id: number;
  descricao: string;
  valor: number;
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
    MatIconModule
  ],
  templateUrl: './lancamentos.html',
  styleUrls: ['./lancamentos.css']
})
export class LancamentosComponent implements OnInit {
  lancamentos: Lancamento[] = [];

  ngOnInit() {
    this.loadLancamentos();
  }

  loadLancamentos() {
    // Aqui você faria uma chamada para a API para carregar os lançamentos
    // Por enquanto, vamos usar dados mockados
    this.lancamentos = [
      { 
        id: 1, 
        descricao: 'Salário', 
        valor: 3000.00, 
        data: new Date(), 
        tipo: 1, 
        tipoRecorrencia: 2, 
        contaId: 1 
      },
      { 
        id: 2, 
        descricao: 'Supermercado', 
        valor: 150.00, 
        data: new Date(), 
        tipo: 2, 
        tipoRecorrencia: 1, 
        contaId: 1 
      }
    ];
  }

  openDialog() {
    // Aqui você abriria um dialog para adicionar um novo lançamento
    console.log('Abrir dialog para adicionar lançamento');
  }

  editLancamento(lancamento: Lancamento) {
    // Aqui você abriria um dialog para editar o lançamento
    console.log('Editar lançamento:', lancamento);
  }

  deleteLancamento(id: number) {
    // Aqui você faria uma chamada para a API para deletar o lançamento
    this.lancamentos = this.lancamentos.filter(l => l.id !== id);
    console.log('Lançamento deletado:', id);
  }
}

