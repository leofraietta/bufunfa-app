using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Bufunfa.Api.Services;
using System.Security.Claims;

namespace Bufunfa.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CartaoCreditoController : ControllerBase
    {
        private readonly ICartaoCreditoService _cartaoCreditoService;

        public CartaoCreditoController(ICartaoCreditoService cartaoCreditoService)
        {
            _cartaoCreditoService = cartaoCreditoService;
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                throw new UnauthorizedAccessException("User ID not found in token.");
            }
            return userId;
        }

        /// <summary>
        /// Verifica se é possível adicionar um lançamento ao cartão de crédito na data especificada
        /// </summary>
        [HttpGet("{contaId}/pode-adicionar-lancamento")]
        public async Task<ActionResult<bool>> PodeAdicionarLancamento(int contaId, [FromQuery] DateTime dataLancamento)
        {
            try
            {
                var podeAdicionar = await _cartaoCreditoService.PodeAdicionarLancamento(contaId, dataLancamento);
                return Ok(podeAdicionar);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Fecha a fatura do cartão de crédito para o mês especificado
        /// </summary>
        [HttpPost("{contaId}/fechar-fatura")]
        public async Task<ActionResult> FecharFatura(int contaId, [FromBody] FecharFaturaRequest request)
        {
            try
            {
                await _cartaoCreditoService.FecharFatura(contaId, request.Ano, request.Mes);
                return Ok(new { message = "Fatura fechada com sucesso" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Consolida a fatura do cartão de crédito lançando o total na conta principal
        /// </summary>
        [HttpPost("{contaId}/consolidar-fatura")]
        public async Task<ActionResult> ConsolidarFatura(int contaId, [FromBody] ConsolidarFaturaRequest request)
        {
            try
            {
                await _cartaoCreditoService.ConsolidarFatura(contaId, request.ContaPrincipalId, request.Ano, request.Mes);
                return Ok(new { message = "Fatura consolidada com sucesso" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Calcula o total da fatura do cartão para o mês especificado
        /// </summary>
        [HttpGet("{contaId}/total-fatura")]
        public async Task<ActionResult<decimal>> CalcularTotalFatura(int contaId, [FromQuery] int ano, [FromQuery] int mes)
        {
            try
            {
                var total = await _cartaoCreditoService.CalcularTotalFatura(contaId, ano, mes);
                return Ok(total);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Verifica se a fatura do cartão está fechada para o mês especificado
        /// </summary>
        [HttpGet("{contaId}/fatura-fechada")]
        public async Task<ActionResult<bool>> FaturaEstaFechada(int contaId, [FromQuery] int ano, [FromQuery] int mes)
        {
            try
            {
                var fechada = await _cartaoCreditoService.FaturaEstaFechada(contaId, ano, mes);
                return Ok(fechada);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Processa todas as faturas vencidas (consolidação automática)
        /// </summary>
        [HttpPost("processar-faturas-vencidas")]
        public async Task<ActionResult> ProcessarFaturasVencidas()
        {
            try
            {
                var cartaoCreditoService = _cartaoCreditoService as CartaoCreditoService;
                if (cartaoCreditoService != null)
                {
                    await cartaoCreditoService.ProcessarFaturasVencidas();
                }
                return Ok(new { message = "Faturas vencidas processadas com sucesso" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    public class FecharFaturaRequest
    {
        public int Ano { get; set; }
        public int Mes { get; set; }
    }

    public class ConsolidarFaturaRequest
    {
        public int ContaPrincipalId { get; set; }
        public int Ano { get; set; }
        public int Mes { get; set; }
    }
}

