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
import { MatIconModule } from '@angular/material/icon';
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
    MatProgressSpinnerModule,
    MatIconModule
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
  parseCurrency(value: any): number {
    if (!value) return 0;
    if (typeof value === 'number') return value;
    if (typeof value !== 'string') return 0;
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
    } else {
      // Para novos lançamentos, inicializar com Recorrente selecionado
      this.onTipoRecorrenciaChange(2);
    }
  }

  createForm(): FormGroup {
    return this.fb.group({
      descricao: ['', Validators.required],
      tipo: [1, Validators.required],
      valorProvisionado: ['', Validators.required],
      data: [new Date(), Validators.required],
      tipoRecorrencia: [2, Validators.required], // Padrão: Recorrente
      tipoPeriodicidade: ['Mensal', Validators.required], // Padrão: Mensal para recorrente
      intervaloDias: [null],
      numeroParcelas: [null],
      contaId: [null, Validators.required],
      categoriaId: [null]
    });
  }

  populateForm(lancamento: any) {
    console.log('Preenchendo formulário com lançamento:', lancamento);
    
    // Converter data do formato brasileiro para Date object
    let dataFormatada = new Date();
    if (lancamento.dataInicial || lancamento.data) {
      const dataString = lancamento.dataInicial || lancamento.data;
      if (typeof dataString === 'string') {
        // Verificar se é formato brasileiro: dd/MM/yyyy HH:mm:ss
        const brazilianDateRegex = /^(\d{2})\/(\d{2})\/(\d{4})\s+(\d{2}):(\d{2}):(\d{2})$/;
        const match = dataString.match(brazilianDateRegex);
        
        if (match) {
          const [, day, month, year, hour, minute, second] = match;
          dataFormatada = new Date(parseInt(year), parseInt(month) - 1, parseInt(day), parseInt(hour), parseInt(minute), parseInt(second));
        } else {
          dataFormatada = new Date(dataString);
        }
      } else {
        dataFormatada = new Date(dataString);
      }
    }
    
    // Mapear status numérico para string se necessário
    let statusMapeado = 'Provisional';
    if (lancamento.status) {
      switch (lancamento.status) {
        case 1:
          statusMapeado = 'Provisional';
          break;
        case 2:
          statusMapeado = 'Realizado';
          break;
        case 3:
          statusMapeado = 'Cancelado';
          break;
        case 4:
          statusMapeado = 'Quitado';
          break;
        default:
          statusMapeado = lancamento.status;
      }
    }

    // Converter strings do backend para valores numéricos esperados pelos dropdowns
    let tipoNumerico = lancamento.tipo;
    if (typeof lancamento.tipo === 'string') {
      switch (lancamento.tipo.toLowerCase()) {
        case 'receita':
          tipoNumerico = 1;
          break;
        case 'despesa':
          tipoNumerico = 2;
          break;
        default:
          tipoNumerico = parseInt(lancamento.tipo) || 1;
      }
    }

    let tipoRecorrenciaNumerico = lancamento.tipoRecorrencia;
    if (typeof lancamento.tipoRecorrencia === 'string') {
      switch (lancamento.tipoRecorrencia.toLowerCase()) {
        case 'esporadico':
          tipoRecorrenciaNumerico = 1;
          break;
        case 'recorrente':
          tipoRecorrenciaNumerico = 2;
          break;
        case 'parcelado':
          tipoRecorrenciaNumerico = 3;
          break;
        default:
          tipoRecorrenciaNumerico = parseInt(lancamento.tipoRecorrencia) || 1;
      }
    }

    let tipoPeriodicidadeNumerico = lancamento.tipoPeriodicidade;
    if (typeof lancamento.tipoPeriodicidade === 'string') {
      switch (lancamento.tipoPeriodicidade.toLowerCase()) {
        case 'semanal':
          tipoPeriodicidadeNumerico = 1;
          break;
        case 'quinzenal':
          tipoPeriodicidadeNumerico = 2;
          break;
        case 'mensal':
          tipoPeriodicidadeNumerico = 3;
          break;
        case 'bimestral':
          tipoPeriodicidadeNumerico = 4;
          break;
        case 'trimestral':
          tipoPeriodicidadeNumerico = 5;
          break;
        case 'semestral':
          tipoPeriodicidadeNumerico = 6;
          break;
        case 'anual':
          tipoPeriodicidadeNumerico = 7;
          break;
        case 'tododiautl':
          tipoPeriodicidadeNumerico = 8;
          break;
        case 'personalizado':
          tipoPeriodicidadeNumerico = 9;
          break;
        default:
          tipoPeriodicidadeNumerico = parseInt(lancamento.tipoPeriodicidade) || null;
      }
    }
    
    this.lancamentoForm.patchValue({
      descricao: lancamento.descricao,
      tipo: tipoNumerico,
      valorProvisionado: lancamento.valorProvisionado || lancamento.valor,
      data: dataFormatada,
      tipoRecorrencia: tipoRecorrenciaNumerico,
      tipoPeriodicidade: tipoPeriodicidadeNumerico,
      intervaloDias: lancamento.intervaloDias,
      numeroParcelas: lancamento.quantidadeParcelas || lancamento.numeroParcelas,
      contaId: lancamento.contaId,
      categoriaId: lancamento.categoriaId
    });
    
    console.log('Valores definidos no formulário:', this.lancamentoForm.value);
    
    // Atualizar flags baseadas no tipo de recorrência
    this.onTipoRecorrenciaChange(tipoRecorrenciaNumerico);
    
    if (tipoPeriodicidadeNumerico) {
      this.onPeriodicidadeChange(tipoPeriodicidadeNumerico);
    }
    
    // Atualizar os campos de moeda com formatação adequada
    setTimeout(() => {
      const valorProvisionadoInput = document.querySelector('input[formControlName="valorProvisionado"]') as HTMLInputElement;
      
      if (valorProvisionadoInput && lancamento.valorProvisionado) {
        valorProvisionadoInput.value = this.formatCurrencyDisplay(lancamento.valorProvisionado);
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
    
    console.log('Mudança de tipo de recorrência:', tipo, 'isRecorrente:', this.isRecorrente);
    
    if (this.isParcelada) {
      // Adicionar validações para parcelada
      this.lancamentoForm.get('numeroParcelas')?.setValidators([Validators.required, Validators.min(1)]);
      // Limpar campos de recorrente
      this.lancamentoForm.get('tipoPeriodicidade')?.clearValidators();
      this.lancamentoForm.get('tipoPeriodicidade')?.setValue(null);
      this.lancamentoForm.get('intervaloDias')?.clearValidators();
      this.lancamentoForm.get('intervaloDias')?.setValue(null);
    } else if (this.isRecorrente) {
      // Adicionar validações para recorrente
      this.lancamentoForm.get('tipoPeriodicidade')?.setValidators([Validators.required]);
      // Sempre definir valor padrão para periodicidade
      this.lancamentoForm.get('tipoPeriodicidade')?.setValue('Mensal');
      console.log('Valor de periodicidade definido como:', this.lancamentoForm.get('tipoPeriodicidade')?.value);
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

  getTipoNumerico(tipo: string): number {
    switch (tipo) {
      case 'Receita': return 1;
      case 'Despesa': return 2;
      default: return parseInt(tipo) || 1;
    }
  }

  getTipoRecorrenciaNumerico(tipoRecorrencia: string): number {
    switch (tipoRecorrencia) {
      case 'Esporadico': return 1;
      case 'Recorrente': return 2;
      case 'Parcelado': return 3;
      default: return parseInt(tipoRecorrencia) || 1;
    }
  }

  onSave() {
    if (this.lancamentoForm.valid) {
      this.isLoading = true;
      const formData = this.lancamentoForm.value;
      
      console.log('Dados do formulário antes do envio:', formData);
      console.log('tipoPeriodicidade no form:', formData.tipoPeriodicidade);
      console.log('Valor atual no controle:', this.lancamentoForm.get('tipoPeriodicidade')?.value);
      
      // Converter valores de moeda para números
      const valorProvisionado = this.parseCurrency(formData.valorProvisionado);
      
      // Converter data para formato ISO local (sem conversão UTC)
      const dataLocal = new Date(formData.data);
      const dataISO = new Date(dataLocal.getTime() - (dataLocal.getTimezoneOffset() * 60000)).toISOString();
      
      // Preparar dados para envio - converter strings para números quando necessário
      const lancamentoData = {
        ...(this.isEdit && { id: this.data.lancamento.id }), // Incluir ID apenas para edição
        descricao: formData.descricao,
        tipo: typeof formData.tipo === 'string' ? this.getTipoNumerico(formData.tipo) : formData.tipo,
        valorProvisionado: valorProvisionado,
        valor: valorProvisionado,
        dataInicial: dataISO,
        tipoRecorrencia: formData.tipoRecorrencia,
        tipoPeriodicidade: formData.tipoPeriodicidade,
        intervaloDias: formData.intervaloDias,
        numeroParcelas: formData.numeroParcelas,
        contaId: formData.contaId,
        categoriaId: formData.categoriaId,
        status: 'Provisional'
      };

      console.log('Dados do lançamento a serem salvos:', lancamentoData);

      const request = this.isEdit 
        ? this.apiService.updateLancamento(this.data.lancamento.id, lancamentoData)
        : this.apiService.createLancamento(lancamentoData);

      request.subscribe({
        next: (result) => {
          console.log('Lançamento salvo com sucesso:', result);
          this.isLoading = false;
          this.dialogRef.close(result);
        },
        error: (error) => {
          console.error('Erro ao salvar lançamento:', error);
          this.isLoading = false;
        }
      });
    }
  }

  onCancel() {
    this.dialogRef.close();
  }
}
