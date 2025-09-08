import { Component, Inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatIconModule } from '@angular/material/icon';
import { CommonModule } from '@angular/common';
import { ApiService, Categoria, Conta } from '../services/api.service';

export interface CategoriaDialogData {
  categoria?: Categoria;
  isEdit: boolean;
}

@Component({
  selector: 'app-categoria-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatCheckboxModule,
    MatIconModule
  ],
  templateUrl: './categoria-dialog.html',
  styleUrls: ['./categoria-dialog.scss']
})
export class CategoriaDialogComponent implements OnInit {
  categoriaForm: FormGroup;
  isEdit: boolean;
  isLoading = false;
  contas: Conta[] = [];

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<CategoriaDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: CategoriaDialogData,
    private apiService: ApiService
  ) {
    this.isEdit = data.isEdit;
    this.categoriaForm = this.createForm();
  }

  ngOnInit() {
    this.loadContas();
    if (this.isEdit && this.data.categoria) {
      this.populateForm(this.data.categoria);
    }
  }

  createForm(): FormGroup {
    return this.fb.group({
      nome: ['', [Validators.required, Validators.minLength(2)]],
      valorProvisionado: [0, [Validators.required, Validators.min(0)]],
      contaId: ['', Validators.required],
      ativa: [true]
    });
  }

  loadContas() {
    this.apiService.getContas().subscribe({
      next: (contas) => {
        this.contas = contas.filter(conta => conta.ativa);
      },
      error: (error) => {
        console.error('Erro ao carregar contas:', error);
        // Fallback para dados mockados
        this.contas = [
          { id: 1, nome: 'Conta Corrente', tipo: 1, saldoInicial: 1000, ativa: true },
          { id: 2, nome: 'Cartão Visa', tipo: 2, saldoInicial: 0, ativa: true }
        ];
      }
    });
  }

  populateForm(categoria: Categoria) {
    this.categoriaForm.patchValue({
      nome: categoria.nome,
      valorProvisionado: categoria.valorProvisionado,
      contaId: categoria.contaId,
      ativa: categoria.ativa
    });
  }

  onSave() {
    if (this.categoriaForm.valid) {
      this.isLoading = true;
      const formData = this.categoriaForm.value;
      
      const categoriaData = {
        nome: formData.nome,
        valorProvisionado: formData.valorProvisionado,
        contaId: formData.contaId,
        ativa: formData.ativa
      };

      const operation = this.isEdit 
        ? this.apiService.updateCategoria(this.data.categoria!.id, categoriaData)
        : this.apiService.createCategoria(categoriaData);

      operation.subscribe({
        next: (result: Categoria) => {
          this.isLoading = false;
          this.dialogRef.close(result);
        },
        error: (error: any) => {
          this.isLoading = false;
          console.error('Erro ao salvar categoria:', error);
          // TODO: Mostrar mensagem de erro para o usuário
        }
      });
    }
  }

  onCancel() {
    this.dialogRef.close();
  }

  getContaNome(contaId: number): string {
    const conta = this.contas.find(c => c.id === contaId);
    return conta ? conta.nome : `Conta #${contaId}`;
  }
}
