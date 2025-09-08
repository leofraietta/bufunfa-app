import { Component, Inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { CommonModule } from '@angular/common';
import { ApiService } from '../services/api';

export interface ContaDialogData {
  conta?: any;
  isEdit: boolean;
}

@Component({
  selector: 'app-conta-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatProgressSpinnerModule,
    MatIconModule
  ],
  templateUrl: './conta-dialog.html',
  styleUrls: ['./conta-dialog.css']
})
export class ContaDialogComponent implements OnInit {
  contaForm: FormGroup;
  isEdit: boolean;
  isLoading = false;
  isCartaoCredito = false;

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<ContaDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ContaDialogData,
    private apiService: ApiService
  ) {
    this.isEdit = data.isEdit;
    this.contaForm = this.createForm();
  }

  ngOnInit() {
    this.initializeForm();
    this.loadContas();
  }


  initializeForm() {
    if (this.isEdit && this.data.conta) {
      this.populateForm(this.data.conta);
    }
  }

  loadContas() {
    // TO DO: implementar carregamento de contas
  }

  createForm(): FormGroup {
    return this.fb.group({
      nome: ['', [Validators.required, Validators.minLength(2)]],
      descricao: ['', [Validators.required, Validators.minLength(2)]],
      tipo: ['', Validators.required],
      saldoInicial: [null, [Validators.required, Validators.min(0)]],
      diaFechamento: [''],
      diaVencimento: ['']
    });
  }

  populateForm(conta: any) {
    this.contaForm.patchValue({
      nome: conta.nome,
      descricao: conta.descricao,
      tipo: conta.tipo,
      saldoInicial: conta.saldoInicial,
      diaFechamento: conta.diaFechamento || null,
      diaVencimento: conta.diaVencimento || null
    });
    this.onTipoChange(conta.tipo);
  }

  onTipoChange(tipo: number) {
    this.isCartaoCredito = tipo === 3; // Fixed: ContaCartaoCredito = 3
    
    if (this.isCartaoCredito) {
      // Adicionar validações para cartão de crédito
      this.contaForm.get('diaFechamento')?.setValidators([Validators.required, Validators.min(1), Validators.max(31)]);
      this.contaForm.get('diaVencimento')?.setValidators([Validators.required, Validators.min(1), Validators.max(31)]);
    } else {
      // Remover validações para conta principal
      this.contaForm.get('diaFechamento')?.clearValidators();
      this.contaForm.get('diaVencimento')?.clearValidators();
      this.contaForm.get('diaFechamento')?.setValue('');
      this.contaForm.get('diaVencimento')?.setValue('');
    }
    
    this.contaForm.get('diaFechamento')?.updateValueAndValidity();
    this.contaForm.get('diaVencimento')?.updateValueAndValidity();
  }

  onSave() {
    if (this.contaForm.valid) {
      this.isLoading = true;
      const formData = this.contaForm.value;
      
      // Preparar dados para envio
      const contaData = {
        nome: formData.nome,
        descricao: formData.descricao,
        tipo: formData.tipo,
        saldoInicial: formData.saldoInicial,
        diaFechamento: formData.diaFechamento || null,
        diaVencimento: formData.diaVencimento || null
      };

      const operation = this.isEdit 
        ? this.apiService.updateConta(this.data.conta.id, contaData)
        : this.apiService.addConta(contaData);

      operation.subscribe({
        next: (result) => {
          this.isLoading = false;
          this.dialogRef.close(result);
        },
        error: (error) => {
          this.isLoading = false;
          console.error('Erro ao salvar conta:', error);
          // TODO: Mostrar mensagem de erro para o usuário
        }
      });
    }
  }

  onCancel() {
    this.dialogRef.close();
  }

  onCurrencyInput(event: any, fieldName: string): void {
    const input = event.target;
    let value = input.value;
    
    // Remove todos os caracteres não numéricos
    value = value.replace(/\D/g, '');
    
    // Converte para número e divide por 100 para ter centavos
    const numericValue = parseInt(value) / 100;
    
    if (isNaN(numericValue)) {
      this.contaForm.get(fieldName)?.setValue(0);
      input.value = '';
      return;
    }
    
    // Atualiza o valor no formulário
    this.contaForm.get(fieldName)?.setValue(numericValue);
    
    // Formata o valor para exibição
    input.value = this.formatCurrencyForInput(numericValue);
  }

  onCurrencyBlur(event: any, fieldName: string): void {
    const input = event.target;
    const formValue = this.contaForm.get(fieldName)?.value;
    
    if (formValue && formValue > 0) {
      input.value = this.formatCurrencyForInput(formValue);
    } else {
      input.value = '';
    }
  }

  private formatCurrencyForInput(value: number): string {
    if (!value || value === 0) return '';
    return new Intl.NumberFormat('pt-BR', {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2
    }).format(value);
  }
}

