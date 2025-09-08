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
        public ActionResult<DashboardDto> GetDashboardData()
        {
            try
            {
                var dashboard = new DashboardDto
                {
                    SaldoTotal = 5420.50m,
                    ReceitasMensais = 8500.00m,
                    DespesasMensais = 3200.75m,
                    ProjecaoSaldo = 6800.25m,
                    ProximosVencimentos = new List<LancamentoResumoDto>
                    {
                        new LancamentoResumoDto
                        {
                            Id = 1,
                            Descricao = "Aluguel",
                            Valor = 1200.00m,
                            DataInicial = DateTime.SpecifyKind(DateTime.Now.AddDays(5), DateTimeKind.Local),
                            Tipo = 2,
                            TipoRecorrencia = 2,
                            ContaId = 1,
                            Realizado = false,
                            Cancelado = false,
                            Quitado = false
                        },
                        new LancamentoResumoDto
                        {
                            Id = 2,
                            Descricao = "Salário",
                            Valor = 5000.00m,
                            DataInicial = DateTime.SpecifyKind(DateTime.Now.AddDays(10), DateTimeKind.Local),
                            Tipo = 1,
                            TipoRecorrencia = 2,
                            ContaId = 1,
                            Realizado = false,
                            Cancelado = false,
                            Quitado = false
                        }
                    }
                };

                return dashboard;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }
    }
}
