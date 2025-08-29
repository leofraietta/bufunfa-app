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
      descricao: ['', [Validators.required, Validators.minLength(2)]],
      tipo: ['', Validators.required],
      saldoInicial: [0, [Validators.required, Validators.min(0)]],
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
}

