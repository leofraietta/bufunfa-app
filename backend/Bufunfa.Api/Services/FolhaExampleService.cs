using Bufunfa.Api.Data;
using Bufunfa.Api.Models;
using Bufunfa.Api.Factories;

namespace Bufunfa.Api.Services
{
    /// <summary>
    /// Serviço de exemplo demonstrando o uso correto das regras de negócio de folhas
    /// Este serviço serve como documentação viva das funcionalidades implementadas
    /// </summary>
    public class FolhaExampleService
    {
        private readonly ApplicationDbContext _context;
        private readonly FolhaMensalService _folhaMensalService;
        private readonly IFolhaAutomaticaService _folhaAutomaticaService;
        private readonly ILancamentoFactory _lancamentoFactory;

        public FolhaExampleService(
            ApplicationDbContext context,
            FolhaMensalService folhaMensalService,
            IFolhaAutomaticaService folhaAutomaticaService,
            ILancamentoFactory lancamentoFactory)
        {
            _context = context;
            _folhaMensalService = folhaMensalService;
            _folhaAutomaticaService = folhaAutomaticaService;
            _lancamentoFactory = lancamentoFactory;
        }

        /// <summary>
        /// Exemplo: Criação de conta com folha automática do mês atual
        /// REGRA: Ao criar uma conta, automaticamente a folha do mês atual é criada
        /// </summary>
        public async Task<(Conta conta, FolhaMensal folha)> ExemploCriarContaComFolhaAutomaticaAsync(int usuarioId)
        {
            // Criar nova conta
            var conta = new ContaCorrente
            {
                Nome = "Conta Corrente Principal",
                SaldoInicial = 1000.00m,
                DataCriacao = DateTime.UtcNow
            };

            _context.Contas.Add(conta);
            await _context.SaveChangesAsync();

            // Criar folha automática do mês atual
            await _folhaAutomaticaService.CriarFolhaMensalInicialAsync(conta, usuarioId);

            // Obter a folha criada
            var agora = DateTime.Now;
            var folha = await _folhaMensalService.ObterFolhaMensalAsync(usuarioId, conta.Id, agora.Year, agora.Month);

            return (conta, folha);
        }

        /// <summary>
        /// Exemplo: Lançamento esporádico na folha do mês vigente
        /// REGRA: Lançamentos esporádicos são feitos diretamente na folha do mês correspondente
        /// </summary>
        public async Task<LancamentoFolha> ExemploLancamentoEsporadicoAsync(int usuarioId, int contaId)
        {
            // Criar lançamento esporádico
            var lancamentoEsporadico = _lancamentoFactory.CriarLancamentoEsporadico();
            lancamentoEsporadico.Descricao = "Compra no supermercado";
            lancamentoEsporadico.ValorProvisionado = 150.00m;
            lancamentoEsporadico.DataInicial = DateTime.Today;
            lancamentoEsporadico.Tipo = TipoLancamento.Despesa;
            lancamentoEsporadico.ContaId = contaId;
            lancamentoEsporadico.UsuarioId = usuarioId;

            _context.Lancamentos.Add(lancamentoEsporadico);
            await _context.SaveChangesAsync();

            // Obter folha do mês atual
            var agora = DateTime.Now;
            var folha = await _folhaMensalService.ObterFolhaMensalAsync(usuarioId, contaId, agora.Year, agora.Month);

            // Adicionar à folha
            var lancamentoFolha = await _folhaMensalService.AdicionarLancamentoEsporadicoAsync(folha.Id, lancamentoEsporadico);

            return lancamentoFolha;
        }

        /// <summary>
        /// Exemplo: Lançamento recorrente mensal
        /// REGRA: Lançamentos recorrentes aparecem todo dia X de cada mês
        /// </summary>
        public async Task<Lancamento> ExemploLancamentoRecorrenteAsync(int usuarioId, int contaId)
        {
            var lancamentoRecorrente = _lancamentoFactory.CriarLancamentoRecorrente();
            lancamentoRecorrente.Descricao = "Salário";
            lancamentoRecorrente.ValorProvisionado = 5000.00m;
            lancamentoRecorrente.DataInicial = new DateTime(2024, 1, 5); // Todo dia 5
            lancamentoRecorrente.DiaVencimento = 5;
            lancamentoRecorrente.Tipo = TipoLancamento.Receita;
            lancamentoRecorrente.ContaId = contaId;
            lancamentoRecorrente.UsuarioId = usuarioId;
            // DataFinal = null (sem fim)

            _context.Lancamentos.Add(lancamentoRecorrente);
            await _context.SaveChangesAsync();

            return lancamentoRecorrente;
        }

        /// <summary>
        /// Exemplo: Lançamento parcelado
        /// REGRA: Lançamentos parcelados ocorrem por N meses consecutivos
        /// </summary>
        public async Task<Lancamento> ExemploLancamentoParceladoAsync(int usuarioId, int contaId)
        {
            var lancamentoParcelado = _lancamentoFactory.CriarLancamentoParcelado();
            lancamentoParcelado.Descricao = "Financiamento do carro";
            lancamentoParcelado.ValorProvisionado = 12000.00m; // Valor total
            lancamentoParcelado.DataInicial = new DateTime(2024, 2, 15);
            lancamentoParcelado.DiaVencimento = 15;
            lancamentoParcelado.QuantidadeParcelas = 24; // 24 meses
            lancamentoParcelado.Tipo = TipoLancamento.Despesa;
            lancamentoParcelado.ContaId = contaId;
            lancamentoParcelado.UsuarioId = usuarioId;

            // Calcular data final automaticamente
            lancamentoParcelado.CalcularDataFinal();

            _context.Lancamentos.Add(lancamentoParcelado);
            await _context.SaveChangesAsync();

            return lancamentoParcelado;
        }

        /// <summary>
        /// Exemplo: Lançamento periódico semanal
        /// REGRA: Lançamentos periódicos semanais ocorrem toda semana no mesmo dia
        /// </summary>
        public async Task<Lancamento> ExemploLancamentoPeriodicoSemanalAsync(int usuarioId, int contaId)
        {
            var lancamentoPeriodico = _lancamentoFactory.CriarLancamentoPeriodico();
            lancamentoPeriodico.Descricao = "Academia - Mensalidade semanal";
            lancamentoPeriodico.ValorProvisionado = 50.00m;
            lancamentoPeriodico.DataInicial = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday); // Próxima segunda
            lancamentoPeriodico.TipoPeriodicidade = TipoPeriodicidade.Semanal;
            lancamentoPeriodico.Tipo = TipoLancamento.Despesa;
            lancamentoPeriodico.ContaId = contaId;
            lancamentoPeriodico.UsuarioId = usuarioId;

            if (lancamentoPeriodico is LancamentoPeriodico periodico)
            {
                periodico.DiaDaSemana = DayOfWeek.Monday; // Toda segunda-feira
            }

            _context.Lancamentos.Add(lancamentoPeriodico);
            await _context.SaveChangesAsync();

            return lancamentoPeriodico;
        }

        /// <summary>
        /// Exemplo: Lançamento periódico quinzenal
        /// REGRA: Lançamentos quinzenais ocorrem a cada 15 dias
        /// </summary>
        public async Task<Lancamento> ExemploLancamentoPeriodicoQuinzenalAsync(int usuarioId, int contaId)
        {
            var lancamentoPeriodico = _lancamentoFactory.CriarLancamentoPeriodico();
            lancamentoPeriodico.Descricao = "Mesada quinzenal";
            lancamentoPeriodico.ValorProvisionado = 200.00m;
            lancamentoPeriodico.DataInicial = DateTime.Today;
            lancamentoPeriodico.TipoPeriodicidade = TipoPeriodicidade.Quinzenal;
            lancamentoPeriodico.Tipo = TipoLancamento.Despesa;
            lancamentoPeriodico.ContaId = contaId;
            lancamentoPeriodico.UsuarioId = usuarioId;
            lancamentoPeriodico.DataFinal = DateTime.Today.AddMonths(6); // Por 6 meses

            _context.Lancamentos.Add(lancamentoPeriodico);
            await _context.SaveChangesAsync();

            return lancamentoPeriodico;
        }

        /// <summary>
        /// Exemplo: Lançamento periódico personalizado (a cada N dias)
        /// REGRA: Lançamentos personalizados ocorrem a cada N dias configurados
        /// </summary>
        public async Task<Lancamento> ExemploLancamentoPeriodicoPersonalizadoAsync(int usuarioId, int contaId)
        {
            var lancamentoPeriodico = _lancamentoFactory.CriarLancamentoPeriodico();
            lancamentoPeriodico.Descricao = "Suplemento vitamínico (a cada 10 dias)";
            lancamentoPeriodico.ValorProvisionado = 25.00m;
            lancamentoPeriodico.DataInicial = DateTime.Today;
            lancamentoPeriodico.TipoPeriodicidade = TipoPeriodicidade.Personalizado;
            lancamentoPeriodico.IntervaloDias = 10; // A cada 10 dias
            lancamentoPeriodico.Tipo = TipoLancamento.Despesa;
            lancamentoPeriodico.ContaId = contaId;
            lancamentoPeriodico.UsuarioId = usuarioId;

            _context.Lancamentos.Add(lancamentoPeriodico);
            await _context.SaveChangesAsync();

            return lancamentoPeriodico;
        }

        /// <summary>
        /// Exemplo: Criação de folha futura para projeção
        /// REGRA: Usuário pode criar folhas futuras para visualizar projeções
        /// </summary>
        public async Task<FolhaMensal> ExemploFolhaFuturaAsync(int usuarioId, int contaId)
        {
            var proximoMes = DateTime.Today.AddMonths(1);
            var folhaFutura = await _folhaAutomaticaService.CriarFolhaFuturaAsync(
                usuarioId, contaId, proximoMes.Year, proximoMes.Month);

            return folhaFutura;
        }

        /// <summary>
        /// Exemplo completo: Cenário de uso típico
        /// </summary>
        public async Task<string> ExemploCompletoAsync(int usuarioId)
        {
            var resultado = new List<string>();

            // 1. Criar conta com folha automática
            var (conta, folhaAtual) = await ExemploCriarContaComFolhaAutomaticaAsync(usuarioId);
            resultado.Add($"✅ Conta criada: {conta.Nome} (ID: {conta.Id})");
            resultado.Add($"✅ Folha automática criada para {folhaAtual.Mes}/{folhaAtual.Ano}");

            // 2. Criar lançamentos diversos
            var salario = await ExemploLancamentoRecorrenteAsync(usuarioId, conta.Id);
            resultado.Add($"✅ Salário recorrente criado: {salario.Descricao}");

            var financiamento = await ExemploLancamentoParceladoAsync(usuarioId, conta.Id);
            resultado.Add($"✅ Financiamento parcelado criado: {financiamento.Descricao}");

            var academia = await ExemploLancamentoPeriodicoSemanalAsync(usuarioId, conta.Id);
            resultado.Add($"✅ Academia semanal criada: {academia.Descricao}");

            // 3. Lançamento esporádico na folha atual
            var compra = await ExemploLancamentoEsporadicoAsync(usuarioId, conta.Id);
            resultado.Add($"✅ Compra esporádica adicionada à folha: {compra.Descricao}");

            // 4. Criar folha futura
            var folhaFutura = await ExemploFolhaFuturaAsync(usuarioId, conta.Id);
            resultado.Add($"✅ Folha futura criada para {folhaFutura.Mes}/{folhaFutura.Ano}");

            // 5. Processar projeções automáticas
            await _folhaAutomaticaService.ProcessarFolhasFuturasAutomaticasAsync(usuarioId, conta.Id, 6);
            resultado.Add("✅ Projeções automáticas processadas para 6 meses");

            return string.Join("\n", resultado);
        }
    }
}
