using Bufunfa.Api.Data;
using Bufunfa.Api.Models;
using Bufunfa.Api.Factories;

namespace Bufunfa.Api.Services
{
    /// <summary>
    /// Serviço de exemplo demonstrando o uso das funcionalidades de conta conjunta
    /// Documenta os cenários de uso e regras de negócio implementadas
    /// </summary>
    public class ContaConjuntaExampleService
    {
        private readonly ApplicationDbContext _context;
        private readonly IContaConjuntaService _contaConjuntaService;
        private readonly ICartaoCompartilhadoService _cartaoCompartilhadoService;
        private readonly FolhaMensalService _folhaMensalService;
        private readonly ILancamentoFactory _lancamentoFactory;

        public ContaConjuntaExampleService(
            ApplicationDbContext context,
            IContaConjuntaService contaConjuntaService,
            ICartaoCompartilhadoService cartaoCompartilhadoService,
            FolhaMensalService folhaMensalService,
            ILancamentoFactory lancamentoFactory)
        {
            _context = context;
            _contaConjuntaService = contaConjuntaService;
            _cartaoCompartilhadoService = cartaoCompartilhadoService;
            _folhaMensalService = folhaMensalService;
            _lancamentoFactory = lancamentoFactory;
        }

        /// <summary>
        /// Exemplo: Criação de conta conjunta com dois usuários
        /// REGRA: Usuário criador é o administrador inicial
        /// </summary>
        public async Task<(ContaConjunta conta, List<ContaUsuario> usuarios)> ExemploCriarContaConjuntaAsync(
            int usuarioCriadorId, int usuarioParceiroId)
        {
            // Criar conta conjunta
            var contaConjunta = new ContaConjunta
            {
                Nome = "Conta Conjunta - Casal",
                SaldoInicial = 0,
                DiaApuracao = 25, // Todo dia 25 do mês
                ManterSaldoPositivo = false, // Distribuir saldo positivo automaticamente
                DataCriacao = DateTime.UtcNow
            };

            _context.Contas.Add(contaConjunta);
            await _context.SaveChangesAsync();

            // Adicionar usuário criador como proprietário/administrador
            var usuarioCriador = new ContaUsuario
            {
                ContaId = contaConjunta.Id,
                UsuarioId = usuarioCriadorId,
                EhProprietario = true,
                PercentualParticipacao = 60.00m, // 60% de participação
                PodeLer = true,
                PodeEscrever = true,
                PodeAdministrar = true,
                Ativo = true
            };

            // Adicionar usuário parceiro
            var usuarioParceiro = await _contaConjuntaService.AdicionarUsuarioAsync(
                contaConjunta.Id, usuarioParceiroId, 40.00m, // 40% de participação
                podeLer: true, podeEscrever: true, podeAdministrar: false);

            _context.ContaUsuarios.Add(usuarioCriador);
            await _context.SaveChangesAsync();

            return (contaConjunta, new List<ContaUsuario> { usuarioCriador, usuarioParceiro });
        }

        /// <summary>
        /// Exemplo: Cartão de crédito compartilhado vinculado à conta conjunta
        /// REGRA: Cartão pode ser usado por múltiplos usuários da conta conjunta
        /// </summary>
        public async Task<ContaCartaoCredito> ExemploCartaoCompartilhadoAsync(
            ContaConjunta contaConjunta, List<int> usuariosAutorizados)
        {
            // Criar cartão de crédito vinculado à conta conjunta
            var cartaoCompartilhado = new ContaCartaoCredito
            {
                Nome = "Cartão Conjunto - Nubank",
                LimiteCredito = 5000.00m,
                DiaFechamento = 15,
                DiaVencimento = 25,
                Bandeira = "Mastercard",
                UltimosDigitos = "1234",
                ContaCorrenteResponsavelId = contaConjunta.Id, // Conta conjunta é responsável
                DataCriacao = DateTime.UtcNow
            };

            _context.Contas.Add(cartaoCompartilhado);
            await _context.SaveChangesAsync();

            // Autorizar usuários da conta conjunta a usar o cartão
            foreach (var usuarioId in usuariosAutorizados)
            {
                await _cartaoCompartilhadoService.AdicionarUsuarioAutorizadoAsync(cartaoCompartilhado.Id, usuarioId);
            }

            return cartaoCompartilhado;
        }

        /// <summary>
        /// Exemplo: Lançamentos na conta conjunta por diferentes usuários
        /// REGRA: Usuários com permissão podem adicionar lançamentos
        /// </summary>
        public async Task<List<Lancamento>> ExemploLancamentosContaConjuntaAsync(
            ContaConjunta contaConjunta, int usuario1Id, int usuario2Id)
        {
            var lancamentos = new List<Lancamento>();

            // Lançamento recorrente - Aluguel (usuário 1)
            var aluguel = _lancamentoFactory.CriarLancamentoRecorrente();
            aluguel.Descricao = "Aluguel do apartamento";
            aluguel.ValorProvisionado = 2500.00m;
            aluguel.DataInicial = new DateTime(2024, 1, 5);
            aluguel.DiaVencimento = 5;
            aluguel.Tipo = TipoLancamento.Despesa;
            aluguel.ContaId = contaConjunta.Id;
            aluguel.UsuarioId = usuario1Id;

            // Lançamento recorrente - Salário conjunto (usuário 2)
            var salarioConjunto = _lancamentoFactory.CriarLancamentoRecorrente();
            salarioConjunto.Descricao = "Renda familiar";
            salarioConjunto.ValorProvisionado = 8000.00m;
            salarioConjunto.DataInicial = new DateTime(2024, 1, 1);
            salarioConjunto.DiaVencimento = 1;
            salarioConjunto.Tipo = TipoLancamento.Receita;
            salarioConjunto.ContaId = contaConjunta.Id;
            salarioConjunto.UsuarioId = usuario2Id;

            // Lançamento parcelado - Móveis (usuário 1)
            var moveis = _lancamentoFactory.CriarLancamentoParcelado();
            moveis.Descricao = "Móveis da sala";
            moveis.ValorProvisionado = 3600.00m; // 12x de R$ 300
            moveis.DataInicial = new DateTime(2024, 2, 10);
            moveis.DiaVencimento = 10;
            moveis.QuantidadeParcelas = 12;
            moveis.Tipo = TipoLancamento.Despesa;
            moveis.ContaId = contaConjunta.Id;
            moveis.UsuarioId = usuario1Id;
            moveis.CalcularDataFinal();

            lancamentos.AddRange(new Lancamento[] { aluguel, salarioConjunto, moveis });
            _context.Lancamentos.AddRange(lancamentos);
            await _context.SaveChangesAsync();

            return lancamentos;
        }

        /// <summary>
        /// Exemplo: Consolidação de fatura do cartão compartilhado
        /// REGRA: Despesas do cartão são centralizadas em uma única fatura na conta responsável
        /// </summary>
        public async Task<ConsolidacaoFaturaResult> ExemploConsolidacaoFaturaAsync(
            ContaCartaoCredito cartaoCompartilhado, int usuario1Id, int usuario2Id)
        {
            // Simular gastos no cartão por diferentes usuários
            var gastoUsuario1 = _lancamentoFactory.CriarLancamentoEsporadico();
            gastoUsuario1.Descricao = "Supermercado";
            gastoUsuario1.ValorProvisionado = 350.00m;
            gastoUsuario1.DataInicial = DateTime.Today.AddDays(-10);
            gastoUsuario1.Tipo = TipoLancamento.Despesa;
            gastoUsuario1.ContaId = cartaoCompartilhado.Id;
            gastoUsuario1.UsuarioId = usuario1Id;

            var gastoUsuario2 = _lancamentoFactory.CriarLancamentoEsporadico();
            gastoUsuario2.Descricao = "Farmácia";
            gastoUsuario2.ValorProvisionado = 120.00m;
            gastoUsuario2.DataInicial = DateTime.Today.AddDays(-5);
            gastoUsuario2.Tipo = TipoLancamento.Despesa;
            gastoUsuario2.ContaId = cartaoCompartilhado.Id;
            gastoUsuario2.UsuarioId = usuario2Id;

            _context.Lancamentos.AddRange(new Lancamento[] { gastoUsuario1, gastoUsuario2 });
            await _context.SaveChangesAsync();

            // Consolidar fatura do mês atual
            var agora = DateTime.Now;
            var consolidacao = await _cartaoCompartilhadoService.ConsolidarFaturaAsync(
                cartaoCompartilhado.Id, agora.Year, agora.Month);

            return consolidacao;
        }

        /// <summary>
        /// Exemplo: Apuração mensal da conta conjunta com distribuição de rateio
        /// REGRA: Saldo é rateado entre usuários na data de apuração configurada
        /// </summary>
        public async Task<ApuracaoResult> ExemploApuracaoMensalAsync(ContaConjunta contaConjunta)
        {
            var agora = DateTime.Now;
            var apuracao = await _contaConjuntaService.ProcessarApuracaoMensalAsync(
                contaConjunta.Id, agora.Year, agora.Month);

            return apuracao;
        }

        /// <summary>
        /// Exemplo: Verificação de permissões de usuário
        /// REGRA: Diferentes níveis de permissão controlam o acesso às funcionalidades
        /// </summary>
        public async Task<Dictionary<string, bool>> ExemploVerificacaoPermissoesAsync(
            int usuarioId, int contaConjuntaId)
        {
            var permissoes = new Dictionary<string, bool>
            {
                ["PodeLer"] = _contaConjuntaService.UsuarioTemPermissao(usuarioId, contaConjuntaId, TipoPermissao.Leitura),
                ["PodeEscrever"] = _contaConjuntaService.UsuarioTemPermissao(usuarioId, contaConjuntaId, TipoPermissao.Escrita),
                ["PodeAdministrar"] = _contaConjuntaService.UsuarioTemPermissao(usuarioId, contaConjuntaId, TipoPermissao.Administracao)
            };

            return permissoes;
        }

        /// <summary>
        /// Exemplo completo: Cenário de uso típico de conta conjunta
        /// </summary>
        public async Task<string> ExemploCompletoContaConjuntaAsync(int usuario1Id, int usuario2Id)
        {
            var resultado = new List<string>();

            // 1. Criar conta conjunta
            var (contaConjunta, usuarios) = await ExemploCriarContaConjuntaAsync(usuario1Id, usuario2Id);
            resultado.Add($"✅ Conta conjunta criada: {contaConjunta.Nome}");
            resultado.Add($"✅ Usuários adicionados: {usuarios.Count} participantes");

            // 2. Criar cartão compartilhado
            var cartaoCompartilhado = await ExemploCartaoCompartilhadoAsync(
                contaConjunta, new List<int> { usuario1Id, usuario2Id });
            resultado.Add($"✅ Cartão compartilhado criado: {cartaoCompartilhado.Nome}");

            // 3. Adicionar lançamentos
            var lancamentos = await ExemploLancamentosContaConjuntaAsync(contaConjunta, usuario1Id, usuario2Id);
            resultado.Add($"✅ Lançamentos criados: {lancamentos.Count} itens");

            // 4. Consolidar fatura do cartão
            var consolidacao = await ExemploConsolidacaoFaturaAsync(cartaoCompartilhado, usuario1Id, usuario2Id);
            resultado.Add($"✅ Fatura consolidada: R$ {consolidacao.ValorTotal:F2}");

            // 5. Processar apuração mensal
            var apuracao = await ExemploApuracaoMensalAsync(contaConjunta);
            resultado.Add($"✅ Apuração processada: {apuracao.Rateios.Count} rateios distribuídos");

            // 6. Verificar permissões
            var permissoes = await ExemploVerificacaoPermissoesAsync(usuario1Id, contaConjunta.Id);
            resultado.Add($"✅ Permissões verificadas: {permissoes.Count(p => p.Value)} ativas");

            return string.Join("\n", resultado);
        }
    }
}
