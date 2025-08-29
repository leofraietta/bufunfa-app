using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Bufunfa.Api.Data;
using Bufunfa.Api.Models;
using Bufunfa.Api.DTOs;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Bufunfa.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class LancamentosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LancamentosController(ApplicationDbContext context)
        {
            _context = context;
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

        // GET: api/Lancamentos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Lancamento>>> GetLancamentos()
        {
            var userId = GetUserId();
            return await _context.Lancamentos
                .Include(l => l.Conta)
                .Include(l => l.Categoria)
                .Where(l => l.UsuarioId == userId)
                .ToListAsync();
        }

        // GET: api/Lancamentos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Lancamento>> GetLancamento(int id)
        {
            var userId = GetUserId();
            var lancamento = await _context.Lancamentos
                .Include(l => l.Conta)
                .Include(l => l.Categoria)
                .FirstOrDefaultAsync(l => l.Id == id && l.UsuarioId == userId);

            if (lancamento == null)
            {
                return NotFound();
            }

            return lancamento;
        }

        // GET: api/Lancamentos/conta/5
        [HttpGet("conta/{contaId}")]
        public async Task<ActionResult<IEnumerable<Lancamento>>> GetLancamentosPorConta(int contaId)
        {
            var userId = GetUserId();
            return await _context.Lancamentos
                .Include(l => l.Conta)
                .Include(l => l.Categoria)
                .Where(l => l.ContaId == contaId && l.UsuarioId == userId)
                .ToListAsync();
        }

        // POST: api/Lancamentos
        [HttpPost]
        public async Task<ActionResult<Lancamento>> PostLancamento(CriarLancamentoDto criarLancamentoDto)
        {
            var userId = GetUserId();
            
            // Converter DTO para entidade
            var lancamento = criarLancamentoDto.ToLancamento();
            lancamento.UsuarioId = userId;

            var conta = await _context.Contas
                .Include(c => c.ContaUsuarios)
                .FirstOrDefaultAsync(c => c.Id == lancamento.ContaId && c.ContaUsuarios.Any(cu => cu.UsuarioId == userId && cu.Ativo));
            if (conta == null)
            {
                return BadRequest("Conta não encontrada ou não pertence ao usuário.");
            }

            // Lógica para despesas recorrentes, parceladas e esporádicas
            if (lancamento.Tipo == TipoLancamento.Despesa)
            {
                if (lancamento.TipoRecorrencia == TipoRecorrencia.Parcelado)
                {
                    if (!lancamento.QuantidadeParcelas.HasValue || lancamento.QuantidadeParcelas <= 0)
                    {
                        return BadRequest("Quantidade de parcelas é obrigatória para lançamentos parcelados.");
                    }
                    // Para lançamentos parcelados, criar um único lançamento parcelado
                    var lancamentoParcelado = new LancamentoParcelado
                    {
                        Descricao = lancamento.Descricao,
                        ValorProvisionado = lancamento.ValorProvisionado,
                        DataInicial = lancamento.DataInicial,
                        Tipo = lancamento.Tipo,
                        QuantidadeParcelas = lancamento.QuantidadeParcelas.Value,
                        DiaVencimento = lancamento.DataInicial.Day,
                        ContaId = lancamento.ContaId,
                        CategoriaId = lancamento.CategoriaId,
                        UsuarioId = userId,
                        DataCriacao = DateTime.Now,
                        Ativo = true
                    };
                    
                    // Calcular data final automaticamente
                    lancamentoParcelado.CalcularDataFinal();
                    
                    _context.Lancamentos.Add(lancamentoParcelado);
                }
                else if (lancamento.TipoRecorrencia == TipoRecorrencia.Recorrente)
                {
                    // Para despesas recorrentes, o valor é provisionado e o impacto no saldo é o valor real
                    // O valor provisionado é armazenado no próprio lançamento para referência
                    _context.Lancamentos.Add(lancamento);
                }
                else // Esporádico
                {
                    _context.Lancamentos.Add(lancamento);
                }
            }
            else // Receita
            {
                _context.Lancamentos.Add(lancamento);
            }

            await _context.SaveChangesAsync();

            return CreatedAtAction("GetLancamento", new { id = lancamento.Id }, lancamento);
        }

        // PUT: api/Lancamentos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLancamento(int id, Lancamento lancamento)
        {
            if (id != lancamento.Id)
            {
                return BadRequest();
            }

            var userId = GetUserId();
            if (lancamento.UsuarioId != userId)
            {
                return Forbid();
            }

            _context.Entry(lancamento).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LancamentoExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Lancamentos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLancamento(int id)
        {
            var userId = GetUserId();
            var lancamento = await _context.Lancamentos.FirstOrDefaultAsync(l => l.Id == id && l.UsuarioId == userId);
            if (lancamento == null)
            {
                return NotFound();
            }

            _context.Lancamentos.Remove(lancamento);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LancamentoExists(int id)
        {
            return _context.Lancamentos.Any(e => e.Id == id);
        }

        // Método para consolidar faturas de cartão de crédito
        [HttpPost("consolidar-fatura/{contaId}")]
        public async Task<IActionResult> ConsolidarFaturaCartao(int contaId)
        {
            var userId = GetUserId();
            var contaCartao = await _context.Contas
                .Include(c => c.Lancamentos)
                .FirstOrDefaultAsync(c => c.Id == contaId && c.UsuarioId == userId && c.Tipo == TipoConta.ContaCartaoCredito);

            if (contaCartao == null)
            {
                return NotFound("Conta de cartão de crédito não encontrada ou não pertence ao usuário.");
            }

            var cartaoCredito = contaCartao as ContaCartaoCredito;
            if (cartaoCredito == null)
            {
                return BadRequest("Não foi possível fazer cast para ContaCartaoCredito.");
            }

            // Verifica se a fatura já foi fechada para o mês atual
            if (cartaoCredito.DataAposFechamento(DateTime.Now))
            {
                // Calcula o total da fatura para o período
                var totalFatura = contaCartao.Lancamentos
                    .Where(l => l.DataInicial.Month == DateTime.Now.Month && l.DataInicial.Year == DateTime.Now.Year && l.Tipo == TipoLancamento.Despesa)
                    .Sum(l => l.Valor);

                // Cria um lançamento de despesa na conta principal do usuário
                var contaPrincipal = await _context.Contas.FirstOrDefaultAsync(c => c.UsuarioId == userId && c.Tipo == TipoConta.ContaCorrente);

                if (contaPrincipal == null)
                {
                    return BadRequest("Conta principal do usuário não encontrada.");
                }

                _context.Lancamentos.Add(new LancamentoEsporadico
                {
                    Descricao = $"Fatura Cartão {contaCartao.Nome} - {DateTime.Now.ToString("MM/yyyy")}",
                    ValorProvisionado = totalFatura,
                    DataInicial = cartaoCredito.CalcularDataVencimento(DateTime.Now.Year, DateTime.Now.Month), // Data de vencimento da fatura
                    Tipo = TipoLancamento.Despesa,
                    ContaId = contaPrincipal.Id,
                    UsuarioId = userId,
                    DataCriacao = DateTime.Now,
                    Ativo = true
                });

                await _context.SaveChangesAsync();

                return Ok("Fatura consolidada com sucesso.");
            }
            else
            {
                return BadRequest("A fatura ainda não pode ser consolidada. Aguarde a data de fechamento.");
            }
        }
    }
}

