using Bufunfa.Api.Data;
using Bufunfa.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Bufunfa.Api.Services
{
    public interface ICartaoCreditoService
    {
        Task<bool> PodeAdicionarLancamento(int contaId, DateTime dataLancamento);
        Task FecharFatura(int contaId, int ano, int mes);
        Task ConsolidarFatura(int contaId, int contaPrincipalId, int ano, int mes);
        Task<decimal> CalcularTotalFatura(int contaId, int ano, int mes);
        Task<bool> FaturaEstaFechada(int contaId, int ano, int mes);
    }

    public class CartaoCreditoService : ICartaoCreditoService
    {
        private readonly ApplicationDbContext _context;
        private readonly FolhaMensalService _folhaMensalService;

        public CartaoCreditoService(ApplicationDbContext context, FolhaMensalService folhaMensalService)
        {
            _context = context;
            _folhaMensalService = folhaMensalService;
        }

        public async Task<bool> PodeAdicionarLancamento(int contaId, DateTime dataLancamento)
        {
            var conta = await _context.Contas.FindAsync(contaId);
            
            if (conta == null || conta.Tipo != TipoConta.ContaCartaoCredito)
            {
                return true; // Não é cartão de crédito, pode adicionar
            }

            var cartaoCredito = conta as ContaCartaoCredito;
            if (cartaoCredito == null)
            {
                return true; // Não conseguiu fazer cast para cartão de crédito
            }

            // Verificar se a fatura do mês do lançamento já foi fechada
            var anoMes = new DateTime(dataLancamento.Year, dataLancamento.Month, 1);
            var dataFechamentoMes = new DateTime(dataLancamento.Year, dataLancamento.Month, cartaoCredito.DiaFechamento);
            
            // Se a data de fechamento já passou no mês do lançamento, não pode adicionar
            return DateTime.Now <= dataFechamentoMes;
        }

        public async Task FecharFatura(int contaId, int ano, int mes)
        {
            var conta = await _context.Contas.FindAsync(contaId);
            
            if (conta == null || conta.Tipo != TipoConta.ContaCartaoCredito)
            {
                throw new InvalidOperationException("Conta não é um cartão de crédito");
            }

            // Marcar a fatura como fechada (implementar lógica de controle de fechamento)
            // Por enquanto, vamos usar a lógica de não permitir novos lançamentos após a data de fechamento
            
            // Verificar se há lançamentos para consolidar
            var totalFatura = await CalcularTotalFatura(contaId, ano, mes);
            
            if (totalFatura > 0)
            {
                // A consolidação será feita automaticamente na data de vencimento
                // ou pode ser chamada manualmente
            }
        }

        public async Task ConsolidarFatura(int contaId, int contaPrincipalId, int ano, int mes)
        {
            var conta = await _context.Contas.FindAsync(contaId);
            var contaPrincipal = await _context.Contas.FindAsync(contaPrincipalId);
            
            if (conta == null || conta.Tipo != TipoConta.ContaCartaoCredito)
            {
                throw new InvalidOperationException("Conta não é um cartão de crédito");
            }

            var cartaoCredito = conta as ContaCartaoCredito;
            if (cartaoCredito == null)
            {
                throw new InvalidOperationException("Não foi possível fazer cast para ContaCartaoCredito");
            }

            if (contaPrincipal == null || contaPrincipal.Tipo != TipoConta.ContaCorrente)
            {
                throw new InvalidOperationException("Conta principal inválida");
            }

            // Calcular total da fatura
            var totalFatura = await CalcularTotalFatura(contaId, ano, mes);
            
            if (totalFatura <= 0)
            {
                return; // Não há valor para consolidar
            }

            // Criar lançamento de consolidação na conta principal
            var dataConsolidacao = cartaoCredito.CalcularDataVencimento(ano, mes);
            
            // Se a data de vencimento já passou no mês, usar o próximo mês
            if (dataConsolidacao < DateTime.Now.Date)
            {
                dataConsolidacao = dataConsolidacao.AddMonths(1);
            }

            var lancamentoConsolidacao = new LancamentoEsporadico
            {
                Descricao = $"Fatura {conta.Nome} - {mes:D2}/{ano}",
                ValorProvisionado = totalFatura,
                ValorReal = totalFatura,
                DataInicial = dataConsolidacao,
                Tipo = TipoLancamento.Despesa,
                ContaId = contaPrincipalId,
                UsuarioId = conta.UsuarioId ?? throw new InvalidOperationException("Conta sem usuário proprietário"),
                DataCriacao = DateTime.UtcNow,
                Ativo = true
            };

            _context.Lancamentos.Add(lancamentoConsolidacao);
            await _context.SaveChangesAsync();

            // Atualizar folha mensal da conta principal se necessário
            var usuarioId = conta.UsuarioId ?? throw new InvalidOperationException("Conta sem usuário proprietário");
            var folhaPrincipal = await _folhaMensalService.ObterFolhaMensalAsync(
                usuarioId, 
                contaPrincipalId, 
                dataConsolidacao.Year, 
                dataConsolidacao.Month
            );
        }

        public async Task<decimal> CalcularTotalFatura(int contaId, int ano, int mes)
        {
            var dataInicio = new DateTime(ano, mes, 1);
            var dataFim = dataInicio.AddMonths(1).AddDays(-1);

            // Buscar todos os lançamentos do cartão no período
            var totalFatura = await _context.LancamentosFolha
                .Include(lf => lf.FolhaMensal)
                .Where(lf => lf.FolhaMensal.ContaId == contaId && 
                            lf.FolhaMensal.Ano == ano && 
                            lf.FolhaMensal.Mes == mes &&
                            lf.Tipo == TipoLancamento.Despesa)
                .SumAsync(lf => lf.ValorReal ?? lf.ValorProvisionado);

            return totalFatura;
        }

        public async Task<bool> FaturaEstaFechada(int contaId, int ano, int mes)
        {
            var conta = await _context.Contas.FindAsync(contaId);
            
            if (conta == null || conta.Tipo != TipoConta.ContaCartaoCredito)
            {
                return false;
            }

            var cartaoCredito = conta as ContaCartaoCredito;
            if (cartaoCredito == null)
            {
                return false;
            }

            var dataFechamento = new DateTime(ano, mes, cartaoCredito.DiaFechamento);
            return DateTime.Now > dataFechamento;
        }

        public async Task ProcessarFaturasVencidas()
        {
            // Buscar cartões com data de vencimento
            var cartoesComVencimento = await _context.Contas
                .Where(c => c.Tipo == TipoConta.ContaCartaoCredito)
                .OfType<ContaCartaoCredito>()
                .ToListAsync();

            foreach (var cartao in cartoesComVencimento)
            {
                // Verificar se há faturas para consolidar
                var hoje = DateTime.Now.Date;
                var diaVencimento = cartao.DiaVencimento;
                
                // Verificar mês atual e anterior
                for (int i = 0; i <= 1; i++)
                {
                    var dataReferencia = hoje.AddMonths(-i);
                    var dataVencimentoMes = new DateTime(dataReferencia.Year, dataReferencia.Month, diaVencimento);
                    
                    if (dataVencimentoMes <= hoje)
                    {
                        // Verificar se já foi consolidada
                        var jaConsolidada = await _context.Lancamentos
                            .AnyAsync(l => l.Descricao.Contains($"Fatura {cartao.Nome} - {dataReferencia.Month:D2}/{dataReferencia.Year}"));
                        
                        if (!jaConsolidada)
                        {
                            // Buscar conta principal do mesmo usuário
                            var contaPrincipal = await _context.Contas
                                .FirstOrDefaultAsync(c => c.UsuarioId == cartao.UsuarioId && c.Tipo == TipoConta.ContaCorrente);
                            
                            if (contaPrincipal != null)
                            {
                                await ConsolidarFatura(cartao.Id, contaPrincipal.Id, dataReferencia.Year, dataReferencia.Month);
                            }
                        }
                    }
                }
            }
        }
    }
}

