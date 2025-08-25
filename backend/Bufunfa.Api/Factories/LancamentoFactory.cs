using Bufunfa.Api.Models;

namespace Bufunfa.Api.Factories
{
    /// <summary>
    /// Factory para criação de instâncias especializadas de Lançamento
    /// Implementa o padrão Factory Method seguindo princípios SOLID
    /// </summary>
    public class LancamentoFactory : ILancamentoFactory
    {
        /// <summary>
        /// Cria uma instância de lançamento baseada no tipo de recorrência
        /// </summary>
        public Lancamento CriarLancamento(TipoRecorrencia tipoRecorrencia)
        {
            return tipoRecorrencia switch
            {
                TipoRecorrencia.Esporadico => new LancamentoEsporadico(),
                TipoRecorrencia.Recorrente => new LancamentoRecorrente(),
                TipoRecorrencia.Parcelado => new LancamentoParcelado(),
                TipoRecorrencia.Periodico => new LancamentoPeriodico(),
                _ => throw new ArgumentException($"Tipo de recorrência não suportado: {tipoRecorrencia}")
            };
        }

        /// <summary>
        /// Cria um lançamento esporádico
        /// </summary>
        public LancamentoEsporadico CriarLancamentoEsporadico()
        {
            return new LancamentoEsporadico();
        }

        /// <summary>
        /// Cria um lançamento recorrente
        /// </summary>
        public LancamentoRecorrente CriarLancamentoRecorrente()
        {
            return new LancamentoRecorrente();
        }

        /// <summary>
        /// Cria um lançamento parcelado
        /// </summary>
        public LancamentoParcelado CriarLancamentoParcelado()
        {
            var lancamento = new LancamentoParcelado();
            // Configurar data final automaticamente quando as parcelas forem definidas
            return lancamento;
        }

        /// <summary>
        /// Cria um lançamento periódico
        /// </summary>
        public LancamentoPeriodico CriarLancamentoPeriodico()
        {
            return new LancamentoPeriodico();
        }

        /// <summary>
        /// Valida se os parâmetros são válidos para o tipo de lançamento
        /// </summary>
        public bool ValidarParametros(TipoRecorrencia tipo, object parametros)
        {
            var lancamento = CriarLancamento(tipo);
            
            // Configurar propriedades básicas para validação
            if (parametros is LancamentoValidationDto dto)
            {
                ConfigurarLancamentoParaValidacao(lancamento, dto);
                return lancamento.EhValido();
            }

            return false;
        }

        /// <summary>
        /// Configura um lançamento com dados para validação
        /// </summary>
        private void ConfigurarLancamentoParaValidacao(Lancamento lancamento, LancamentoValidationDto dto)
        {
            lancamento.Descricao = dto.Descricao ?? "Teste";
            lancamento.ValorProvisionado = dto.ValorProvisionado;
            lancamento.DataInicial = dto.DataInicial;
            lancamento.Tipo = dto.Tipo;
            lancamento.ContaId = dto.ContaId;
            lancamento.UsuarioId = dto.UsuarioId;

            switch (lancamento.TipoRecorrencia)
            {
                case TipoRecorrencia.Recorrente:
                    lancamento.DiaVencimento = dto.DiaVencimento;
                    lancamento.DataFinal = dto.DataFinal;
                    break;

                case TipoRecorrencia.Parcelado:
                    lancamento.QuantidadeParcelas = dto.QuantidadeParcelas;
                    lancamento.DiaVencimento = dto.DiaVencimento;
                    if (lancamento is LancamentoParcelado parcelado)
                    {
                        parcelado.CalcularDataFinal();
                    }
                    break;

                case TipoRecorrencia.Periodico:
                    lancamento.TipoPeriodicidade = dto.TipoPeriodicidade;
                    lancamento.DataFinal = dto.DataFinal;
                    
                    if (lancamento is LancamentoPeriodico periodico)
                    {
                        switch (dto.TipoPeriodicidade)
                        {
                            case TipoPeriodicidade.Semanal:
                                periodico.DiaDaSemana = dto.DiaDaSemana;
                                break;
                            case TipoPeriodicidade.Mensal:
                                lancamento.DiaVencimento = dto.DiaVencimento;
                                break;
                            case TipoPeriodicidade.Anual:
                                periodico.DiaDoAno = dto.DiaDoAno;
                                break;
                            case TipoPeriodicidade.Personalizado:
                                lancamento.IntervaloDias = dto.IntervaloDias;
                                break;
                        }
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// DTO para validação de parâmetros de lançamento
    /// </summary>
    public class LancamentoValidationDto
    {
        public string? Descricao { get; set; }
        public decimal ValorProvisionado { get; set; }
        public DateTime DataInicial { get; set; }
        public TipoLancamento Tipo { get; set; }
        public int ContaId { get; set; }
        public int UsuarioId { get; set; }
        public DateTime? DataFinal { get; set; }
        
        // Para recorrentes e parcelados
        public int? DiaVencimento { get; set; }
        
        // Para parcelados
        public int? QuantidadeParcelas { get; set; }
        
        // Para periódicos
        public TipoPeriodicidade? TipoPeriodicidade { get; set; }
        public DayOfWeek? DiaDaSemana { get; set; }
        public int? DiaDoAno { get; set; }
        public int? IntervaloDias { get; set; }
    }
}
