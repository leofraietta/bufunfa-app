import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { AuthService } from '../auth/auth.service';

export interface Conta {
  id: number;
  nome: string;
  tipo: TipoConta;
  saldoInicial: number;
  dataFechamento?: number;
  dataVencimento?: number;
  contaPagamentoId?: number;
  ativa: boolean;
  nivelPermissao?: NivelPermissao;
  ehAdministrador?: boolean;
  percentualParticipacao?: number;
}

export interface Categoria {
  id: number;
  nome: string;
  valorProvisionado: number;
  contaId: number;
  ativa: boolean;
}

export interface Lancamento {
  id: number;
  descricao: string;
  valor: number;
  valorProvisionado?: number;
  valorReal?: number;
  dataInicial: Date;
  dataFinal?: Date;
  data?: Date;
  tipo: TipoLancamento;
  tipoRecorrencia: TipoRecorrencia;
  tipoPeriodicidade?: TipoPeriodicidade;
  quantidadeParcelas?: number;
  contaId: number;
  categoriaId?: number;
  realizado: boolean;
  cancelado: boolean;
  quitado: boolean;
}

export interface FolhaMensal {
  id: number;
  contaId: number;
  mesAno: string;
  saldoInicial: number;
  saldoFinal: number;
  totalReceitas: number;
  totalDespesas: number;
  totalReceitasProvisionadas: number;
  totalDespesasProvisionadas: number;
  status: StatusFolhaMensal;
  lancamentos: Lancamento[];
}

export interface DashboardData {
  saldoTotal: number;
  receitasMensais: number;
  despesasMensais: number;
  projecaoSaldo: number;
  proximosVencimentos: Lancamento[];
}

export enum TipoConta {
  ContaCorrente = 1,
  ContaCartaoCredito = 2
}

export enum TipoLancamento {
  Receita = 1,
  Despesa = 2
}

export enum TipoRecorrencia {
  Esporadico = 1,
  Recorrente = 2,
  Parcelado = 3
}

export enum TipoPeriodicidade {
  Semanal = 1,
  Quinzenal = 2,
  Mensal = 3,
  Bimestral = 4,
  Trimestral = 5,
  Semestral = 6,
  Anual = 7,
  TodoDiaUtil = 8,
  Personalizado = 9
}

export enum StatusFolhaMensal {
  Aberta = 1,
  Fechada = 2
}

export enum NivelPermissao {
  Visualizacao = 1,
  Completo = 2
}

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private baseUrl = environment.apiUrl;
  private isApiAvailable = false;

  constructor(private http: HttpClient, private authService: AuthService) {}

  private getHeaders(): HttpHeaders {
    return new HttpHeaders(this.authService.getAuthHeaders());
  }

  // Dashboard
  getDashboardData(): Observable<DashboardData> {
    return this.http.get<DashboardData>(`${this.baseUrl}/dashboard`, { 
      headers: this.getHeaders()
    }).pipe(
      catchError((error: any) => {
        console.warn('API não disponível, usando dados mockados:', error);
        return of(this.getMockDashboardData());
      })
    );
  }

  // Mock dashboard data for fallback
  getMockDashboardData(): DashboardData {
    return {
      saldoTotal: 5420.50,
      receitasMensais: 8500.00,
      despesasMensais: 3200.75,
      projecaoSaldo: 6800.25,
      proximosVencimentos: [
        {
          id: 1,
          descricao: 'Aluguel',
          valor: 1200.00,
          dataInicial: new Date('2024-01-15'),
          tipo: TipoLancamento.Despesa,
          tipoRecorrencia: TipoRecorrencia.Recorrente,
          contaId: 1,
          realizado: false,
          cancelado: false,
          quitado: false
        },
        {
          id: 2,
          descricao: 'Salário',
          valor: 5000.00,
          dataInicial: new Date('2024-01-05'),
          tipo: TipoLancamento.Receita,
          tipoRecorrencia: TipoRecorrencia.Recorrente,
          contaId: 1,
          realizado: false,
          cancelado: false,
          quitado: false
        }
      ]
    };
  }

  // Contas
  getContas(): Observable<Conta[]> {
    return this.http.get<Conta[]>(`${this.baseUrl}/contas`, { headers: this.getHeaders() });
  }

  getConta(id: number): Observable<Conta> {
    return this.http.get<Conta>(`${this.baseUrl}/contas/${id}`, { headers: this.getHeaders() });
  }

  createConta(conta: Partial<Conta>): Observable<Conta> {
    return this.http.post<Conta>(`${this.baseUrl}/contas`, conta, { headers: this.getHeaders() });
  }

  updateConta(id: number, conta: Partial<Conta>): Observable<Conta> {
    return this.http.put<Conta>(`${this.baseUrl}/contas/${id}`, conta, { headers: this.getHeaders() });
  }

  deleteConta(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/contas/${id}`, { headers: this.getHeaders() });
  }

  // Categorias
  getCategorias(contaId?: number): Observable<Categoria[]> {
    const url = contaId ? `${this.baseUrl}/categorias?contaId=${contaId}` : `${this.baseUrl}/categorias`;
    return this.http.get<Categoria[]>(url, { headers: this.getHeaders() });
  }

  getCategoria(id: number): Observable<Categoria> {
    return this.http.get<Categoria>(`${this.baseUrl}/categorias/${id}`, { headers: this.getHeaders() });
  }

  createCategoria(categoria: Partial<Categoria>): Observable<Categoria> {
    return this.http.post<Categoria>(`${this.baseUrl}/categorias`, categoria, { headers: this.getHeaders() });
  }

  updateCategoria(id: number, categoria: Partial<Categoria>): Observable<Categoria> {
    return this.http.put<Categoria>(`${this.baseUrl}/categorias/${id}`, categoria, { headers: this.getHeaders() });
  }

  deleteCategoria(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/categorias/${id}`, { headers: this.getHeaders() });
  }

  // Lançamentos
  getLancamentos(contaId?: number, mesAno?: string): Observable<Lancamento[]> {
    let url = `${this.baseUrl}/lancamentos`;
    const params = [];
    if (contaId) params.push(`contaId=${contaId}`);
    if (mesAno) params.push(`mesAno=${mesAno}`);
    if (params.length > 0) url += `?${params.join('&')}`;
    
    return this.http.get<Lancamento[]>(url, { headers: this.getHeaders() });
  }

  getLancamento(id: number): Observable<Lancamento> {
    return this.http.get<Lancamento>(`${this.baseUrl}/lancamentos/${id}`, { headers: this.getHeaders() });
  }

  createLancamento(lancamento: Partial<Lancamento>): Observable<Lancamento> {
    return this.http.post<Lancamento>(`${this.baseUrl}/lancamentos`, lancamento, { headers: this.getHeaders() });
  }

  updateLancamento(id: number, lancamento: Partial<Lancamento>): Observable<Lancamento> {
    return this.http.put<Lancamento>(`${this.baseUrl}/lancamentos/${id}`, lancamento, { headers: this.getHeaders() });
  }

  deleteLancamento(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/lancamentos/${id}`, { headers: this.getHeaders() });
  }

  realizarLancamento(id: number, valorReal: number): Observable<Lancamento> {
    return this.http.post<Lancamento>(`${this.baseUrl}/lancamentos/${id}/realizar`, 
      { valorReal }, { headers: this.getHeaders() });
  }

  cancelarLancamento(id: number): Observable<Lancamento> {
    return this.http.post<Lancamento>(`${this.baseUrl}/lancamentos/${id}/cancelar`, 
      {}, { headers: this.getHeaders() });
  }

  // Folhas Mensais
  getFolhasMensais(contaId?: number): Observable<FolhaMensal[]> {
    const url = contaId ? `${this.baseUrl}/folhas-mensais?contaId=${contaId}` : `${this.baseUrl}/folhas-mensais`;
    return this.http.get<FolhaMensal[]>(url, { headers: this.getHeaders() });
  }

  getFolhaMensal(contaId: number, mesAno: string): Observable<FolhaMensal> {
    return this.http.get<FolhaMensal>(`${this.baseUrl}/folhas-mensais/${contaId}/${mesAno}`, 
      { headers: this.getHeaders() });
  }

  fecharFolhaMensal(contaId: number, mesAno: string): Observable<FolhaMensal> {
    return this.http.post<FolhaMensal>(`${this.baseUrl}/folhas-mensais/${contaId}/${mesAno}/fechar`, 
      {}, { headers: this.getHeaders() });
  }

  reabrirFolhaMensal(contaId: number, mesAno: string): Observable<FolhaMensal> {
    return this.http.post<FolhaMensal>(`${this.baseUrl}/folhas-mensais/${contaId}/${mesAno}/reabrir`, 
      {}, { headers: this.getHeaders() });
  }

  // Utility methods
  getTipoContaText(tipo: TipoConta): string {
    switch (tipo) {
      case TipoConta.ContaCorrente: return 'Conta Corrente';
      case TipoConta.ContaCartaoCredito: return 'Cartão de Crédito';
      default: return 'Desconhecido';
    }
  }

  getTipoLancamentoText(tipo: TipoLancamento): string {
    switch (tipo) {
      case TipoLancamento.Receita: return 'Receita';
      case TipoLancamento.Despesa: return 'Despesa';
      default: return 'Desconhecido';
    }
  }

  getTipoRecorrenciaText(tipo: TipoRecorrencia): string {
    switch (tipo) {
      case TipoRecorrencia.Esporadico: return 'Esporádico';
      case TipoRecorrencia.Recorrente: return 'Recorrente';
      case TipoRecorrencia.Parcelado: return 'Parcelado';
      default: return 'Desconhecido';
    }
  }

  getTipoPeriodicidadeText(tipo?: TipoPeriodicidade): string {
    if (!tipo) return '';
    switch (tipo) {
      case TipoPeriodicidade.Semanal: return 'Semanal';
      case TipoPeriodicidade.Quinzenal: return 'Quinzenal';
      case TipoPeriodicidade.Mensal: return 'Mensal';
      case TipoPeriodicidade.Bimestral: return 'Bimestral';
      case TipoPeriodicidade.Trimestral: return 'Trimestral';
      case TipoPeriodicidade.Semestral: return 'Semestral';
      case TipoPeriodicidade.Anual: return 'Anual';
      case TipoPeriodicidade.TodoDiaUtil: return 'Todo Dia Útil';
      case TipoPeriodicidade.Personalizado: return 'Personalizado';
      default: return 'Desconhecido';
    }
  }

  formatMesAno(mesAno: string): string {
    const [ano, mes] = mesAno.split('-');
    const meses = [
      'Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho',
      'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro'
    ];
    return `${meses[parseInt(mes) - 1]} ${ano}`;
  }

  getCurrentMesAno(): string {
    const now = new Date();
    const year = now.getFullYear();
    const month = (now.getMonth() + 1).toString().padStart(2, '0');
    return `${year}-${month}`;
  }

  getMesesDisponiveis(quantidadeMeses: number = 12): { value: string, label: string }[] {
    const meses = [];
    const now = new Date();
    
    for (let i = -6; i < quantidadeMeses - 6; i++) {
      const date = new Date(now.getFullYear(), now.getMonth() + i, 1);
      const year = date.getFullYear();
      const month = (date.getMonth() + 1).toString().padStart(2, '0');
      const mesAno = `${year}-${month}`;
      
      meses.push({
        value: mesAno,
        label: this.formatMesAno(mesAno)
      });
    }
    
    return meses;
  }
}
