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

export interface LancamentoDialogData {
  lancamento?: any;
  isEdit: boolean;
}

@Component({
  selector: 'app-lancamento-dialog',
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
  templateUrl: './lancamento-dialog.html',
  styleUrls: ['./lancamento-dialog.css']
})
export class LancamentoDialogComponent implements OnInit {
  lancamentoForm: FormGroup;
  isEdit: boolean;
  isLoading = false;
  isParcelada = false;
  contas: any[] = [];
  categorias: any[] = [];

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<LancamentoDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: LancamentoDialogData,
    private apiService: ApiService
  ) {
    this.isEdit = data.isEdit;
    this.lancamentoForm = this.createForm();
  }

  ngOnInit() {
    this.loadContas();
    this.loadCategorias();
    
    if (this.isEdit && this.data.lancamento) {
      this.populateForm(this.data.lancamento);
    }
  }

  createForm(): FormGroup {
    return this.fb.group({
      descricao: ['', [Validators.required, Validators.minLength(2)]],
      tipo: ['', Validators.required],
      valorProvisionado: [0, [Validators.required, Validators.min(0.01)]],
      valorReal: [null],
      data: [new Date(), Validators.required],
      tipoRecorrencia: ['', Validators.required],
      numeroParcelas: [null],
      contaId: ['', Validators.required],
      categoriaId: [null]
    });
  }

  populateForm(lancamento: any) {
    this.lancamentoForm.patchValue({
      descricao: lancamento.descricao,
      tipo: lancamento.tipo,
      valorProvisionado: lancamento.valorProvisionado || lancamento.valor,
      valorReal: lancamento.valorReal,
      data: lancamento.data ? new Date(lancamento.data) : new Date(),
      tipoRecorrencia: lancamento.tipoRecorrencia,
      numeroParcelas: lancamento.numeroParcelas,
      contaId: lancamento.contaId,
      categoriaId: lancamento.categoriaId
    });
    this.onTipoRecorrenciaChange(lancamento.tipoRecorrencia);
  }

  loadContas() {
    this.apiService.getContas().subscribe({
      next: (contas) => {
        this.contas = contas;
      },
      error: (error) => {
        console.error('Erro ao carregar contas:', error);
        // Fallback para dados mockados
        this.contas = [
          { id: 1, nome: 'Conta Corrente' },
          { id: 2, nome: 'Cartão Visa' }
        ];
      }
    });
  }

  loadCategorias() {
    this.apiService.getCategorias().subscribe({
      next: (categorias) => {
        this.categorias = categorias;
      },
      error: (error) => {
        console.error('Erro ao carregar categorias:', error);
        // Fallback para dados mockados
        this.categorias = [
          { id: 1, nome: 'Alimentação' },
          { id: 2, nome: 'Transporte' },
          { id: 3, nome: 'Moradia' },
          { id: 4, nome: 'Saúde' },
          { id: 5, nome: 'Educação' }
        ];
      }
    });
  }

  onTipoRecorrenciaChange(tipo: number) {
    this.isParcelada = tipo === 3;
    
    if (this.isParcelada) {
      // Adicionar validações para parcelada
      this.lancamentoForm.get('numeroParcelas')?.setValidators([Validators.required, Validators.min(1)]);
    } else {
      // Remover validações para outros tipos
      this.lancamentoForm.get('numeroParcelas')?.clearValidators();
      this.lancamentoForm.get('numeroParcelas')?.setValue(null);
    }
    
    this.lancamentoForm.get('numeroParcelas')?.updateValueAndValidity();
  }

  onSave() {
    if (this.lancamentoForm.valid) {
      this.isLoading = true;
      const formData = this.lancamentoForm.value;
      
      // Preparar dados para envio
      const lancamentoData = {
        descricao: formData.descricao,
        tipo: formData.tipo,
        valorProvisionado: formData.valorProvisionado,
        valorReal: formData.valorReal,
        data: formData.data,
        tipoRecorrencia: formData.tipoRecorrencia,
        numeroParcelas: formData.numeroParcelas,
        contaId: formData.contaId,
        categoriaId: formData.categoriaId
      };

      const operation = this.isEdit 
        ? this.apiService.updateLancamento(this.data.lancamento.id, lancamentoData)
        : this.apiService.addLancamento(lancamentoData);

      operation.subscribe({
        next: (result) => {
          this.isLoading = false;
          this.dialogRef.close(result);
        },
        error: (error) => {
          this.isLoading = false;
          console.error('Erro ao salvar lançamento:', error);
          // TODO: Mostrar mensagem de erro para o usuário
        }
      });
    }
  }

  onCancel() {
    this.dialogRef.close();
  }
}

