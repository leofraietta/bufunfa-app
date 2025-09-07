import { Injectable } from '@angular/core';
import { formatCurrency, formatDate, formatNumber } from '@angular/common';

@Injectable({
  providedIn: 'root'
})
export class BrazilianFormatterService {
  private readonly locale = 'pt-BR';
  private readonly currency = 'BRL';
  private readonly timezone = 'America/Sao_Paulo';

  /**
   * Formatar valor monetário em padrão brasileiro
   */
  formatCurrency(value: number): string {
    return formatCurrency(value, this.locale, 'R$', this.currency, '1.2-2');
  }

  /**
   * Formatar número em padrão brasileiro (vírgula como separador decimal)
   */
  formatNumber(value: number, digitsInfo: string = '1.2-2'): string {
    return formatNumber(value, this.locale, digitsInfo);
  }

  /**
   * Formatar data em padrão brasileiro
   */
  formatDate(value: Date | string, format: string = 'dd/MM/yyyy'): string {
    return formatDate(value, format, this.locale, this.timezone);
  }

  /**
   * Formatar data e hora em padrão brasileiro
   */
  formatDateTime(value: Date | string): string {
    return formatDate(value, 'dd/MM/yyyy HH:mm', this.locale, this.timezone);
  }

  /**
   * Formatar data curta (apenas dia/mês)
   */
  formatShortDate(value: Date | string): string {
    return formatDate(value, 'dd/MM', this.locale, this.timezone);
  }

  /**
   * Formatar percentual em padrão brasileiro
   */
  formatPercent(value: number): string {
    return formatNumber(value / 100, this.locale, '1.2-2') + '%';
  }

  /**
   * Converter string de data do backend para Date
   */
  parseBackendDate(dateString: string): Date {
    if (!dateString) return new Date();
    
    // Tentar múltiplos formatos
    const formats = [
      /^(\d{2})\/(\d{2})\/(\d{4}) (\d{2}):(\d{2}):(\d{2})$/, // dd/MM/yyyy HH:mm:ss
      /^(\d{2})\/(\d{2})\/(\d{4})$/, // dd/MM/yyyy
      /^(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2}):(\d{2})/, // ISO format
    ];

    for (const format of formats) {
      const match = dateString.match(format);
      if (match) {
        if (format === formats[0]) { // dd/MM/yyyy HH:mm:ss
          return new Date(+match[3], +match[2] - 1, +match[1], +match[4], +match[5], +match[6]);
        } else if (format === formats[1]) { // dd/MM/yyyy
          return new Date(+match[3], +match[2] - 1, +match[1]);
        }
      }
    }

    // Fallback para parsing padrão
    return new Date(dateString);
  }

  /**
   * Obter nome do mês em português
   */
  getMonthName(monthNumber: number): string {
    const months = [
      'Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho',
      'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro'
    ];
    return months[monthNumber - 1] || 'Mês';
  }

  /**
   * Obter nome do dia da semana em português
   */
  getDayName(date: Date): string {
    const days = ['Domingo', 'Segunda', 'Terça', 'Quarta', 'Quinta', 'Sexta', 'Sábado'];
    return days[date.getDay()];
  }

  /**
   * Validar se uma string é uma data válida no formato brasileiro
   */
  isValidBrazilianDate(dateString: string): boolean {
    const regex = /^(\d{2})\/(\d{2})\/(\d{4})$/;
    const match = dateString.match(regex);
    
    if (!match) return false;
    
    const day = +match[1];
    const month = +match[2];
    const year = +match[3];
    
    const date = new Date(year, month - 1, day);
    return date.getFullYear() === year && 
           date.getMonth() === month - 1 && 
           date.getDate() === day;
  }
}
