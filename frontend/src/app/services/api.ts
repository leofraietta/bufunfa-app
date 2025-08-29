import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private baseUrl = 'http://localhost:5000/api'; // URL base do seu backend .NET

  constructor(private http: HttpClient) { }

  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('jwt_token');
    return new HttpHeaders({
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    });
  }

  // Métodos de autenticação
  googleLogin(): void {
    window.location.href = `${this.baseUrl}/Auth/google-login`;
  }

  // Métodos para Contas
  getContas(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/Contas`, { headers: this.getHeaders() });
  }

  getConta(id: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/Contas/${id}`, { headers: this.getHeaders() });
  }

  addConta(conta: any): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/Contas`, conta, { headers: this.getHeaders() });
  }

  updateConta(id: number, conta: any): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/Contas/${id}`, conta, { headers: this.getHeaders() });
  }

  deleteConta(id: number): Observable<any> {
    return this.http.delete<any>(`${this.baseUrl}/Contas/${id}`, { headers: this.getHeaders() });
  }

  // Métodos para Categorias
  getCategorias(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/Categorias`, { headers: this.getHeaders() });
  }

  getCategoria(id: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/Categorias/${id}`, { headers: this.getHeaders() });
  }

  addCategoria(categoria: any): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/Categorias`, categoria, { headers: this.getHeaders() });
  }

  updateCategoria(id: number, categoria: any): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/Categorias/${id}`, categoria, { headers: this.getHeaders() });
  }

  deleteCategoria(id: number): Observable<any> {
    return this.http.delete<any>(`${this.baseUrl}/Categorias/${id}`, { headers: this.getHeaders() });
  }

  // Métodos para Lançamentos
  getLancamentos(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/Lancamentos`, { headers: this.getHeaders() });
  }

  getLancamento(id: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/Lancamentos/${id}`, { headers: this.getHeaders() });
  }

  addLancamento(lancamento: any): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/Lancamentos`, lancamento, { headers: this.getHeaders() });
  }

  updateLancamento(id: number, lancamento: any): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/Lancamentos/${id}`, lancamento, { headers: this.getHeaders() });
  }

  deleteLancamento(id: number): Observable<any> {
    return this.http.delete<any>(`${this.baseUrl}/Lancamentos/${id}`, { headers: this.getHeaders() });
  }

  consolidarFaturaCartao(contaId: number): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/Lancamentos/consolidar-fatura/${contaId}`, {}, { headers: this.getHeaders() });
  }

  // Métodos para Cartão de Crédito
  podeAdicionarLancamentoCartao(contaId: number, dataLancamento: Date): Observable<boolean> {
    const params = `?dataLancamento=${dataLancamento.toISOString()}`;
    return this.http.get<boolean>(`${this.baseUrl}/CartaoCredito/${contaId}/pode-adicionar-lancamento${params}`, { headers: this.getHeaders() });
  }

  fecharFaturaCartao(contaId: number, ano: number, mes: number): Observable<any> {
    const body = { ano, mes };
    return this.http.post<any>(`${this.baseUrl}/CartaoCredito/${contaId}/fechar-fatura`, body, { headers: this.getHeaders() });
  }

  consolidarFaturaCartaoAvancado(contaId: number, contaPrincipalId: number, ano: number, mes: number): Observable<any> {
    const body = { contaPrincipalId, ano, mes };
    return this.http.post<any>(`${this.baseUrl}/CartaoCredito/${contaId}/consolidar-fatura`, body, { headers: this.getHeaders() });
  }

  calcularTotalFaturaCartao(contaId: number, ano: number, mes: number): Observable<number> {
    const params = `?ano=${ano}&mes=${mes}`;
    return this.http.get<number>(`${this.baseUrl}/CartaoCredito/${contaId}/total-fatura${params}`, { headers: this.getHeaders() });
  }

  faturaCartaoEstaFechada(contaId: number, ano: number, mes: number): Observable<boolean> {
    const params = `?ano=${ano}&mes=${mes}`;
    return this.http.get<boolean>(`${this.baseUrl}/CartaoCredito/${contaId}/fatura-fechada${params}`, { headers: this.getHeaders() });
  }

  processarFaturasVencidas(): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/CartaoCredito/processar-faturas-vencidas`, {}, { headers: this.getHeaders() });
  }

  // Métodos para Provisionamento de Mercado
  getProvisionamentos(ano: number, mes: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/Provisionamento/${ano}/${mes}`, { headers: this.getHeaders() });
  }

  getProvisionamento(id: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/Provisionamento/${id}`, { headers: this.getHeaders() });
  }

  criarProvisionamento(contaId: number, categoriaId: number, valorMensal: number, ano: number, mes: number): Observable<any> {
    const body = { contaId, categoriaId, valorMensal, ano, mes };
    return this.http.post<any>(`${this.baseUrl}/Provisionamento`, body, { headers: this.getHeaders() });
  }

  adicionarGastoReal(provisionamentoId: number, valor: number, descricao: string, data: Date): Observable<any> {
    const body = { valor, descricao, data };
    return this.http.post<any>(`${this.baseUrl}/Provisionamento/${provisionamentoId}/gasto`, body, { headers: this.getHeaders() });
  }

  getResumoProvisionamento(provisionamentoId: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/Provisionamento/${provisionamentoId}/resumo`, { headers: this.getHeaders() });
  }

  // Métodos para Folhas Mensais
  getFolhasMensais(ano: number, mes: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/FolhasMensais/${ano}/${mes}`, { headers: this.getHeaders() });
  }

  getFolhaMensal(contaId: number, ano: number, mes: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/FolhasMensais/${contaId}/${ano}/${mes}`, { headers: this.getHeaders() });
  }

  abrirFolhaMensal(contaId: number, ano: number, mes: number): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/FolhasMensais/${contaId}/${ano}/${mes}/abrir`, {}, { headers: this.getHeaders() });
  }

  realizarLancamentoFolha(lancamentoFolhaId: number, valorReal: number, dataRealizacao?: Date): Observable<any> {
    const body = {
      valorReal: valorReal,
      dataRealizacao: dataRealizacao || new Date()
    };
    return this.http.put<any>(`${this.baseUrl}/FolhasMensais/lancamentos/${lancamentoFolhaId}/realizar`, body, { headers: this.getHeaders() });
  }

  getResumoMensal(ano: number, mes: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/FolhasMensais/resumo/${ano}/${mes}`, { headers: this.getHeaders() });
  }

  // Métodos para Contas Conjuntas
  getContasConjuntas(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/ContasConjuntas`, { headers: this.getHeaders() });
  }

  getContaConjunta(id: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/ContasConjuntas/${id}`, { headers: this.getHeaders() });
  }

  addContaConjunta(contaConjunta: any): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/ContasConjuntas`, contaConjunta, { headers: this.getHeaders() });
  }

  updateContaConjunta(id: number, contaConjunta: any): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/ContasConjuntas/${id}`, contaConjunta, { headers: this.getHeaders() });
  }

  deleteContaConjunta(id: number): Observable<any> {
    return this.http.delete<any>(`${this.baseUrl}/ContasConjuntas/${id}`, { headers: this.getHeaders() });
  }

  adicionarUsuarioContaConjunta(contaConjuntaId: number, emailUsuario: string): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/ContasConjuntas/${contaConjuntaId}/adicionar-usuario`, `"${emailUsuario}"`, { headers: this.getHeaders() });
  }

  atualizarRateio(contaConjuntaId: number, rateioId: number, percentual: number): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/ContasConjuntas/${contaConjuntaId}/atualizar-rateio/${rateioId}`, percentual, { headers: this.getHeaders() });
  }

  apurarContaConjunta(contaConjuntaId: number): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/ContasConjuntas/${contaConjuntaId}/apurar`, {}, { headers: this.getHeaders() });
  }
}


