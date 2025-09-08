using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Bufunfa.Api.Data;
using Bufunfa.Api.DTOs;

namespace Bufunfa.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<DashboardDto>> GetDashboardData()
        {
            try
            {
                var userId = 1; // Temporário para debug
                var agora = DateTime.UtcNow;
                var mesAtual = agora.Month;
                var anoAtual = agora.Year;

                // Calcular saldo total das contas ativas
                var saldoTotal = await _context.Contas
                    .Where(c => c.ContaUsuarios.Any(cu => cu.UsuarioId == userId && cu.Ativo))
                    .SumAsync(c => c.SaldoInicial);

                // Calcular receitas mensais (lançamentos de receita do mês atual)
                var receitasMensais = await _context.Lancamentos
                    .Where(l => l.UsuarioId == userId && 
                               l.Tipo == Models.TipoLancamento.Receita &&
                               l.DataInicial.Month == mesAtual && 
                               l.DataInicial.Year == anoAtual)
                    .SumAsync(l => l.ValorProvisionado);

                // Calcular despesas mensais (lançamentos de despesa do mês atual)
                var despesasMensais = await _context.Lancamentos
                    .Where(l => l.UsuarioId == userId && 
                               l.Tipo == Models.TipoLancamento.Despesa &&
                               l.DataInicial.Month == mesAtual && 
                               l.DataInicial.Year == anoAtual)
                    .SumAsync(l => l.ValorProvisionado);

                // Projeção de saldo (saldo atual + receitas - despesas)
                var projecaoSaldo = saldoTotal + receitasMensais - despesasMensais;

                // Próximos vencimentos (próximos 30 dias)
                var dataLimite = agora.AddDays(30);
                var proximosVencimentos = await _context.Lancamentos
                    .Where(l => l.UsuarioId == userId && 
                               l.DataInicial >= agora && 
                               l.DataInicial <= dataLimite &&
                               l.Status != Models.StatusLancamento.Realizado)
                    .OrderBy(l => l.DataInicial)
                    .Take(5)
                    .Select(l => new LancamentoResumoDto
                    {
                        Id = l.Id,
                        Descricao = l.Descricao,
                        Valor = l.ValorProvisionado,
                        DataInicial = l.DataInicial,
                        Tipo = (int)l.Tipo,
                        TipoRecorrencia = (int)l.TipoRecorrencia,
                        ContaId = l.ContaId,
                        Realizado = l.Status == Models.StatusLancamento.Realizado,
                        Cancelado = l.Status == Models.StatusLancamento.Cancelado,
                        Quitado = l.Status == Models.StatusLancamento.Quitado
                    })
                    .ToListAsync();

                var dashboard = new DashboardDto
                {
                    SaldoTotal = saldoTotal,
                    ReceitasMensais = receitasMensais,
                    DespesasMensais = despesasMensais,
                    ProjecaoSaldo = projecaoSaldo,
                    ProximosVencimentos = proximosVencimentos
                };

                Console.WriteLine($"Dashboard calculado - Saldo: {saldoTotal}, Receitas: {receitasMensais}, Despesas: {despesasMensais}");

                return dashboard;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro no dashboard: {ex.Message}");
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }
    }
}
