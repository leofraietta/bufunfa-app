using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Bufunfa.Api.Services;
using Bufunfa.Api.Models;
using System.Security.Claims;

namespace Bufunfa.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FolhasMensaisController : ControllerBase
    {
        private readonly FolhaMensalService _folhaMensalService;

        public FolhasMensaisController(FolhaMensalService folhaMensalService)
        {
            _folhaMensalService = folhaMensalService;
        }

        private int ObterUsuarioId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Console.WriteLine($"DEBUG: UserIdClaim = {userIdClaim}");
            
            if (int.TryParse(userIdClaim, out var userId))
            {
                Console.WriteLine($"DEBUG: UserId parsed = {userId}");
                return userId;
            }
            
            Console.WriteLine("DEBUG: Failed to parse userId, returning 0");
            return 0;
        }

        [HttpGet("{ano}/{mes}")]
        public async Task<ActionResult<List<FolhaMensal>>> ObterFolhasMensais(int ano, int mes)
        {
            try
            {
                var usuarioId = ObterUsuarioId();
                if (usuarioId == 0)
                    return Unauthorized("Usuário não identificado");

                var folhas = await _folhaMensalService.ObterFolhasMensaisUsuarioAsync(usuarioId, ano, mes);
                return Ok(folhas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }

        [HttpGet("{contaId}/{ano}/{mes}")]
        public async Task<ActionResult<FolhaMensal>> ObterFolhaMensal(int contaId, int ano, int mes)
        {
            try
            {
                Console.WriteLine($"DEBUG: Endpoint chamado - contaId: {contaId}, ano: {ano}, mes: {mes}");
                
                var usuarioId = ObterUsuarioId();
                Console.WriteLine($"DEBUG: UsuarioId obtido: {usuarioId}");
                
                if (usuarioId == 0)
                {
                    Console.WriteLine("DEBUG: Retornando Unauthorized - usuário não identificado");
                    return Unauthorized("Usuário não identificado");
                }

                Console.WriteLine($"DEBUG: Chamando ObterFolhaMensalAsync com usuarioId: {usuarioId}, contaId: {contaId}, ano: {ano}, mes: {mes}");
                var folha = await _folhaMensalService.ObterFolhaMensalAsync(usuarioId, contaId, ano, mes);
                Console.WriteLine($"DEBUG: Folha obtida: {folha?.Id}");
                
                return Ok(folha);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG: Erro capturado: {ex.Message}");
                Console.WriteLine($"DEBUG: StackTrace: {ex.StackTrace}");
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }

        [HttpPost("{contaId}/{ano}/{mes}/abrir")]
        public async Task<ActionResult<FolhaMensal>> AbrirFolhaMensal(int contaId, int ano, int mes)
        {
            try
            {
                var usuarioId = ObterUsuarioId();
                if (usuarioId == 0)
                    return Unauthorized("Usuário não identificado");

                var folha = await _folhaMensalService.AbrirFolhaMensalAsync(usuarioId, contaId, ano, mes);
                return Ok(folha);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }

        [HttpPut("lancamentos/{lancamentoFolhaId}/realizar")]
        public async Task<ActionResult> RealizarLancamento(int lancamentoFolhaId, [FromBody] RealizarLancamentoRequest request)
        {
            try
            {
                await _folhaMensalService.AtualizarLancamentoFolhaAsync(
                    lancamentoFolhaId, 
                    request.ValorReal, 
                    request.DataRealizacao ?? DateTime.Now);

                return Ok(new { message = "Lançamento realizado com sucesso" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }

        [HttpGet("resumo/{ano}/{mes}")]
        public async Task<ActionResult<ResumoMensalResponse>> ObterResumoMensal(int ano, int mes)
        {
            try
            {
                var usuarioId = ObterUsuarioId();
                if (usuarioId == 0)
                    return Unauthorized("Usuário não identificado");

                var folhas = await _folhaMensalService.ObterFolhasMensaisUsuarioAsync(usuarioId, ano, mes);
                
                var resumo = new ResumoMensalResponse
                {
                    Ano = ano,
                    Mes = mes,
                    SaldoTotalReal = folhas.Sum(f => f.SaldoFinalReal),
                    SaldoTotalProvisionado = folhas.Sum(f => f.SaldoFinalProvisionado),
                    TotalReceitasReais = folhas.Sum(f => f.TotalReceitasReais),
                    TotalReceitasProvisionadas = folhas.Sum(f => f.TotalReceitasProvisionadas),
                    TotalDespesasReais = folhas.Sum(f => f.TotalDespesasReais),
                    TotalDespesasProvisionadas = folhas.Sum(f => f.TotalDespesasProvisionadas),
                    FolhasPorConta = folhas.Select(f => new FolhaResumo
                    {
                        ContaId = f.ContaId,
                        NomeConta = f.Conta?.Nome ?? "Conta",
                        SaldoReal = f.SaldoFinalReal,
                        SaldoProvisionado = f.SaldoFinalProvisionado
                    }).ToList()
                };

                return Ok(resumo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }
    }

    public class RealizarLancamentoRequest
    {
        public decimal ValorReal { get; set; }
        public DateTime? DataRealizacao { get; set; }
    }

    public class ResumoMensalResponse
    {
        public int Ano { get; set; }
        public int Mes { get; set; }
        public decimal SaldoTotalReal { get; set; }
        public decimal SaldoTotalProvisionado { get; set; }
        public decimal TotalReceitasReais { get; set; }
        public decimal TotalReceitasProvisionadas { get; set; }
        public decimal TotalDespesasReais { get; set; }
        public decimal TotalDespesasProvisionadas { get; set; }
        public List<FolhaResumo> FolhasPorConta { get; set; } = new();
    }

    public class FolhaResumo
    {
        public int ContaId { get; set; }
        public string NomeConta { get; set; } = string.Empty;
        public decimal SaldoReal { get; set; }
        public decimal SaldoProvisionado { get; set; }
    }
}

