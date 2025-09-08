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
    if (this.isEdit && this.data.categoria) {
      this.populateForm(this.data.categoria);
    }
  }

  createForm(): FormGroup {
    return this.fb.group({
      nome: ['', [Validators.required, Validators.minLength(2)]],
      descricao: [''],
      valorProvisionadoMensal: [0, [Validators.required, Validators.min(0)]],
      ativa: [true]
    });
  }

  // Removido método loadContas pois categorias não precisam mais de conta
  // As categorias agora são independentes de contas específicas

  populateForm(categoria: Categoria) {
    this.categoriaForm.patchValue({
      nome: categoria.nome,
      descricao: categoria.descricao || '',
      valorProvisionadoMensal: categoria.valorProvisionadoMensal,
      ativa: categoria.ativa
    });
  }

  onSave() {
    if (this.categoriaForm.valid) {
      this.isLoading = true;
      const formData = this.categoriaForm.value;
      
      const categoriaData = {
        nome: formData.nome,
        descricao: formData.descricao || '',
        valorProvisionadoMensal: formData.valorProvisionadoMensal,
        ativa: formData.ativa
      };

      console.log('Form Data:', formData);
      console.log('Categoria Data sendo enviada:', categoriaData);
      console.log('Form válido:', this.categoriaForm.valid);
      console.log('Erros do form:', this.categoriaForm.errors);

      const operation = this.isEdit 
        ? this.apiService.updateCategoria(this.data.categoria!.id, categoriaData)
        : this.apiService.createCategoria(categoriaData);

      operation.subscribe({
        next: (result: Categoria) => {
          this.isLoading = false;
          console.log('Categoria criada com sucesso:', result);
          this.dialogRef.close(result);
        },
        error: (error: any) => {
          this.isLoading = false;
          console.error('Erro completo ao salvar categoria:', error);
          console.error('Status:', error.status);
          console.error('Message:', error.message);
          console.error('Error body:', error.error);
          // TODO: Mostrar mensagem de erro para o usuário
        }
      });
    } else {
      console.log('Formulário inválido:', this.categoriaForm.errors);
      Object.keys(this.categoriaForm.controls).forEach(key => {
        const control = this.categoriaForm.get(key);
        if (control && control.errors) {
          console.log(`Campo ${key} tem erros:`, control.errors);
        }
      });
    }
  }

  setValorProvisionadoMensal(valor: number) {
    this.categoriaForm.patchValue({ valorProvisionadoMensal: valor });
  }

  onCurrencyInput(event: any, fieldName: string): void {
    const input = event.target;
    let value = input.value;
    
    // Remove todos os caracteres não numéricos
    value = value.replace(/\D/g, '');
    
    // Converte para número e divide por 100 para ter centavos
    const numericValue = parseInt(value) / 100;
    
    if (isNaN(numericValue)) {
      this.categoriaForm.get(fieldName)?.setValue(0);
      input.value = '';
      return;
    }
    
    // Atualiza o valor no formulário
    this.categoriaForm.get(fieldName)?.setValue(numericValue);
    
    // Formata o valor para exibição
    input.value = this.formatCurrency(numericValue);
  }

  onCurrencyBlur(event: any, fieldName: string): void {
    const input = event.target;
    const formValue = this.categoriaForm.get(fieldName)?.value;
    
    if (formValue && formValue > 0) {
      input.value = this.formatCurrency(formValue);
    } else {
      input.value = '';
    }
  }

  onCancel() {
    this.dialogRef.close();
  }


  private formatCurrency(value: number): string {
    if (value === null || value === undefined || isNaN(value)) {
      return '';
    }
    
    return new Intl.NumberFormat('pt-BR', {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2
    }).format(value);
  }
}
