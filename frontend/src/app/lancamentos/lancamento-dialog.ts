import { Component, Inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule, NativeDateAdapter, DateAdapter, MAT_DATE_FORMATS, MAT_DATE_LOCALE } from '@angular/material/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { CommonModule } from '@angular/common';
import { ApiService } from '../services/api.service';

// Definir formato de data brasileiro
const MY_DATE_FORMATS = {
  parse: {
    dateInput: 'DD/MM/YYYY',
  },
  display: {
    dateInput: 'DD/MM/YYYY',
    monthYearLabel: 'MMM YYYY',
    dateA11yLabel: 'LL',
    monthYearA11yLabel: 'MMMM YYYY',
  },
};

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
  providers: [
    { provide: DateAdapter, useClass: NativeDateAdapter },
    { provide: MAT_DATE_LOCALE, useValue: 'pt-BR' },
    { provide: MAT_DATE_FORMATS, useValue: MY_DATE_FORMATS }
  ],
  templateUrl: './lancamento-dialog.html',
  styleUrls: ['./lancamento-dialog.css']
})
export class LancamentoDialogComponent implements OnInit {
  lancamentoForm: FormGroup;
  isEdit: boolean;
  isLoading = false;
  isParcelada = false;
  isRecorrente = false;
  isPersonalizado = false;
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

  // Método para formatar valor como moeda brasileira
  formatCurrency(value: number): string {
    if (!value) return '';
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL'
    }).format(value);
  }

  // Método para converter string formatada em número
  parseCurrency(value: string): number {
    if (!value) return 0;
    // Remove todos os caracteres não numéricos exceto vírgula
    const cleanValue = value.replace(/[^\d,]/g, '');
    // Substitui vírgula por ponto para conversão
    const numericValue = cleanValue.replace(',', '.');
    return parseFloat(numericValue) || 0;
  }

  // Método para aplicar máscara de moeda durante digitação
  onCurrencyInput(event: any, fieldName: string): void {
    const input = event.target;
    let value = input.value;
    
    // Remove todos os caracteres não numéricos
    value = value.replace(/\D/g, '');
    
    // Converte para número e divide por 100 para ter centavos
    const numericValue = parseInt(value) / 100;
    
    if (isNaN(numericValue)) {
      this.lancamentoForm.get(fieldName)?.setValue(0);
      input.value = '';
      return;
    }
    
    // Atualiza o valor no formulário
    this.lancamentoForm.get(fieldName)?.setValue(numericValue);
    
    // Formata o valor para exibição
    input.value = this.formatCurrencyDisplay(numericValue);
  }

  // Método para formatar valor para exibição no input
  formatCurrencyDisplay(value: number): string {
    if (!value || value === 0) return '';
    return new Intl.NumberFormat('pt-BR', {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2
    }).format(value);
  }

  // Método para tratar evento de blur nos campos de moeda
  onCurrencyBlur(event: any, fieldName: string): void {
    const input = event.target;
    const formValue = this.lancamentoForm.get(fieldName)?.value;
    
    if (formValue && formValue > 0) {
      input.value = this.formatCurrencyDisplay(formValue);
    } else {
      input.value = '';
    }
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
      status: ['Provisional'],
      tipoPeriodicidade: [null],
      intervaloDias: [null],
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
      status: lancamento.status || 'Provisional',
      tipoPeriodicidade: lancamento.tipoPeriodicidade,
      intervaloDias: lancamento.intervaloDias,
      numeroParcelas: lancamento.numeroParcelas,
      contaId: lancamento.contaId,
      categoriaId: lancamento.categoriaId
    });
    this.onTipoRecorrenciaChange(lancamento.tipoRecorrencia);
    
    if (lancamento.tipoPeriodicidade) {
      this.onPeriodicidadeChange(lancamento.tipoPeriodicidade);
    }
    
    // Atualizar os campos de moeda com formatação adequada
    setTimeout(() => {
      const valorProvisionadoInput = document.querySelector('input[formControlName="valorProvisionado"]') as HTMLInputElement;
      const valorRealInput = document.querySelector('input[formControlName="valorReal"]') as HTMLInputElement;
      
      if (valorProvisionadoInput && lancamento.valorProvisionado) {
        valorProvisionadoInput.value = this.formatCurrencyDisplay(lancamento.valorProvisionado);
      }
      
      if (valorRealInput && lancamento.valorReal) {
        valorRealInput.value = this.formatCurrencyDisplay(lancamento.valorReal);
      }
    }, 100);
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
    this.isRecorrente = tipo === 2;
    
    if (this.isParcelada) {
      // Adicionar validações para parcelada
      this.lancamentoForm.get('numeroParcelas')?.setValidators([Validators.required, Validators.min(1)]);
      // Limpar campos de recorrente
      this.lancamentoForm.get('tipoPeriodicidade')?.clearValidators();
      this.lancamentoForm.get('intervaloDias')?.clearValidators();
    } else if (this.isRecorrente) {
      // Adicionar validações para recorrente
      this.lancamentoForm.get('tipoPeriodicidade')?.setValidators([Validators.required]);
      // Limpar campos de parcelada
      this.lancamentoForm.get('numeroParcelas')?.clearValidators();
      this.lancamentoForm.get('numeroParcelas')?.setValue(null);
    } else {
      // Limpar todas as validações para esporádica
      this.lancamentoForm.get('numeroParcelas')?.clearValidators();
      this.lancamentoForm.get('numeroParcelas')?.setValue(null);
      this.lancamentoForm.get('tipoPeriodicidade')?.clearValidators();
      this.lancamentoForm.get('tipoPeriodicidade')?.setValue(null);
      this.lancamentoForm.get('intervaloDias')?.clearValidators();
      this.lancamentoForm.get('intervaloDias')?.setValue(null);
    }
    
    // Atualizar validações
    this.lancamentoForm.get('numeroParcelas')?.updateValueAndValidity();
    this.lancamentoForm.get('tipoPeriodicidade')?.updateValueAndValidity();
    this.lancamentoForm.get('intervaloDias')?.updateValueAndValidity();
  }

  onPeriodicidadeChange(periodicidade: string) {
    this.isPersonalizado = periodicidade === 'Personalizado';
    
    if (this.isPersonalizado) {
      // Adicionar validações para intervalo personalizado
      this.lancamentoForm.get('intervaloDias')?.setValidators([
        Validators.required, 
        Validators.min(1), 
        Validators.max(6)
      ]);
    } else {
      // Limpar validações e valor para outros tipos
      this.lancamentoForm.get('intervaloDias')?.clearValidators();
      this.lancamentoForm.get('intervaloDias')?.setValue(null);
    }
    
    this.lancamentoForm.get('intervaloDias')?.updateValueAndValidity();
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
        status: formData.status,
        tipoPeriodicidade: formData.tipoPeriodicidade,
        intervaloDias: formData.intervaloDias,
        numeroParcelas: formData.numeroParcelas,
        contaId: formData.contaId,
        categoriaId: formData.categoriaId
      };

      const operation = this.isEdit 
        ? this.apiService.updateLancamento(this.data.lancamento.id, lancamentoData)
        : this.apiService.createLancamento(lancamentoData);

      operation.subscribe({
        next: (result: any) => {
          this.isLoading = false;
          this.dialogRef.close(result);
        },
        error: (error: any) => {
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

