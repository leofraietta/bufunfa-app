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
    MatProgressSpinnerModule
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
    if (this.isEdit && this.data.conta) {
      this.populateForm(this.data.conta);
    }
  }

  createForm(): FormGroup {
    return this.fb.group({
      nome: ['', [Validators.required, Validators.minLength(2)]],
      tipo: ['', Validators.required],
      saldoInicial: [0, [Validators.required, Validators.min(0)]],
      dataFechamento: [''],
      dataVencimento: ['']
    });
  }

  populateForm(conta: any) {
    this.contaForm.patchValue({
      nome: conta.nome,
      tipo: conta.tipo,
      saldoInicial: conta.saldoInicial,
      dataFechamento: conta.dataFechamento ? new Date(conta.dataFechamento) : null,
      dataVencimento: conta.dataVencimento ? new Date(conta.dataVencimento) : null
    });
    this.onTipoChange(conta.tipo);
  }

  onTipoChange(tipo: number) {
    this.isCartaoCredito = tipo === 2;
    
    if (this.isCartaoCredito) {
      // Adicionar validações para cartão de crédito
      this.contaForm.get('dataFechamento')?.setValidators([Validators.required]);
      this.contaForm.get('dataVencimento')?.setValidators([Validators.required]);
    } else {
      // Remover validações para conta principal
      this.contaForm.get('dataFechamento')?.clearValidators();
      this.contaForm.get('dataVencimento')?.clearValidators();
      this.contaForm.get('dataFechamento')?.setValue('');
      this.contaForm.get('dataVencimento')?.setValue('');
    }
    
    this.contaForm.get('dataFechamento')?.updateValueAndValidity();
    this.contaForm.get('dataVencimento')?.updateValueAndValidity();
  }

  onSave() {
    if (this.contaForm.valid) {
      this.isLoading = true;
      const formData = this.contaForm.value;
      
      // Preparar dados para envio
      const contaData = {
        nome: formData.nome,
        tipo: formData.tipo,
        saldoInicial: formData.saldoInicial,
        dataFechamento: formData.dataFechamento || null,
        dataVencimento: formData.dataVencimento || null
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
}

