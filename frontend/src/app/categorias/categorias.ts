import { Component, OnInit } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { CommonModule } from '@angular/common';
import { ApiService, Categoria } from '../services/api.service';
import { CategoriaDialogComponent } from './categoria-dialog';

@Component({
  selector: 'app-categorias',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatDialogModule,
    MatTableModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatChipsModule,
    MatTooltipModule
  ],
  templateUrl: './categorias.html',
  styleUrls: ['./categorias.scss']
})
export class CategoriasComponent implements OnInit {
  categorias: Categoria[] = [];
  isLoading = false;
  error: string | null = null;
  displayedColumns: string[] = ['nome', 'valorProvisionado', 'conta', 'status', 'acoes'];

  constructor(
    private dialog: MatDialog,
    private apiService: ApiService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit() {
    this.loadCategorias();
  }

  loadCategorias() {
    this.isLoading = true;
    this.error = null;
    this.apiService.getCategorias().subscribe({
      next: (categorias) => {
        this.categorias = categorias;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Erro ao carregar categorias:', error);
        this.error = 'Erro ao carregar categorias. Usando dados de exemplo.';
        this.isLoading = false;
        // Fallback para dados mockados em caso de erro
        this.categorias = [
          { 
            id: 1, 
            nome: 'Alimentação', 
            valorProvisionado: 800.00,
            contaId: 1,
            ativa: true
          },
          { 
            id: 2, 
            nome: 'Transporte', 
            valorProvisionado: 300.00,
            contaId: 1,
            ativa: true
          },
          { 
            id: 3, 
            nome: 'Lazer', 
            valorProvisionado: 200.00,
            contaId: 1,
            ativa: false
          }
        ];
      }
    });
  }

  openDialog() {
    const dialogRef = this.dialog.open(CategoriaDialogComponent, {
      width: '500px',
      data: { isEdit: false }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.snackBar.open('Categoria criada com sucesso', 'Fechar', { duration: 3000 });
        this.loadCategorias();
      }
    });
  }

  editCategoria(categoria: Categoria) {
    const dialogRef = this.dialog.open(CategoriaDialogComponent, {
      width: '500px',
      data: { categoria, isEdit: true }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.snackBar.open('Categoria atualizada com sucesso', 'Fechar', { duration: 3000 });
        this.loadCategorias();
      }
    });
  }

  deleteCategoria(id: number) {
    if (confirm('Tem certeza que deseja excluir esta categoria?')) {
      this.apiService.deleteCategoria(id).subscribe({
        next: () => {
          this.snackBar.open('Categoria excluída com sucesso', 'Fechar', { duration: 3000 });
          this.loadCategorias();
        },
        error: (error) => {
          console.error('Erro ao deletar categoria:', error);
          this.snackBar.open('Erro ao excluir categoria', 'Fechar', { duration: 3000 });
          // Fallback para remoção local em caso de erro
          this.categorias = this.categorias.filter(c => c.id !== id);
        }
      });
    }
  }

  getStatusColor(ativa: boolean): string {
    return ativa ? 'primary' : 'warn';
  }

  getStatusText(ativa: boolean): string {
    return ativa ? 'Ativa' : 'Inativa';
  }

  getCategoriasAtivas(): number {
    return this.categorias.filter(categoria => categoria.ativa).length;
  }

  getTotalProvisionado(): number {
    return this.categorias
      .filter(categoria => categoria.ativa)
      .reduce((total, categoria) => total + categoria.valorProvisionado, 0);
  }

  getContaNome(contaId: number): string {
    // TODO: Implementar busca do nome da conta
    return `Conta #${contaId}`;
  }
}
