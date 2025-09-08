using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Bufunfa.Api.Data;
using Bufunfa.Api.Models;
using Bufunfa.Api.DTOs;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Bufunfa.Api.Controllers
{
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
            // Temporário: retorna usuário padrão para teste (ID = 1)
            // TODO: Implementar autenticação adequada
            return 1;
        }

        // GET: api/Lancamentos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Lancamento>>> GetLancamentos()
        {
            var userId = GetUserId();
            
            try
            {
                return await _context.Lancamentos
                    .Include(l => l.Conta)
                    .Include(l => l.Categoria)
                    .Where(l => l.UsuarioId == userId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
            }
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
            Console.WriteLine("=== MÉTODO POST LANÇAMENTO CHAMADO ===");
            Console.WriteLine($"TipoPeriodicidade recebido: {criarLancamentoDto.TipoPeriodicidade}");
            Console.WriteLine($"TipoRecorrencia: {criarLancamentoDto.TipoRecorrencia}");
            
            var userId = GetUserId();
            
            // Converter DTO para entidade
            var lancamento = criarLancamentoDto.ToLancamento();
            lancamento.UsuarioId = userId;
            
            Console.WriteLine($"TipoPeriodicidade na entidade: {lancamento.TipoPeriodicidade}");

            var conta = await _context.Contas
                .Include(c => c.ContaUsuarios)
                .FirstOrDefaultAsync(c => c.Id == lancamento.ContaId && c.ContaUsuarios.Any(cu => cu.UsuarioId == userId && cu.Ativo));
            if (conta == null)
            {
                return BadRequest("Conta não encontrada ou não pertence ao usuário.");
            }

            // Validar campos específicos baseados no tipo de recorrência
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
                    Status = lancamento.Status,
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
                // Validar periodicidade para recorrentes
                if (!lancamento.TipoPeriodicidade.HasValue)
                {
                    return BadRequest("Tipo de periodicidade é obrigatório para lançamentos recorrentes.");
                }
                
                // Validar intervalo para periodicidade personalizada
                if (lancamento.TipoPeriodicidade == TipoPeriodicidade.Personalizado)
                {
                    if (!lancamento.IntervaloDias.HasValue || lancamento.IntervaloDias < 1 || lancamento.IntervaloDias > 6)
                    {
                        return BadRequest("Intervalo deve ser entre 1 e 6 dias para periodicidade personalizada.");
                    }
                }
                
                var lancamentoRecorrente = new LancamentoRecorrente
                {
                    Descricao = lancamento.Descricao,
                    ValorProvisionado = lancamento.ValorProvisionado,
                    DataInicial = lancamento.DataInicial,
                    DataFinal = lancamento.DataFinal,
                    Tipo = lancamento.Tipo,
                    Status = lancamento.Status,
                    TipoPeriodicidade = lancamento.TipoPeriodicidade.Value,
                    IntervaloDias = lancamento.IntervaloDias,
                    AjustarDiaUtil = lancamento.AjustarDiaUtil,
                    DiaVencimento = lancamento.DataInicial.Day,
                    ContaId = lancamento.ContaId,
                    CategoriaId = lancamento.CategoriaId,
                    UsuarioId = userId,
                    DataCriacao = DateTime.Now,
                    Ativo = true
                };
                
                _context.Lancamentos.Add(lancamentoRecorrente);
            }
            else // Esporádico
            {
                var lancamentoEsporadico = new LancamentoEsporadico
                {
                    Descricao = lancamento.Descricao,
                    ValorProvisionado = lancamento.ValorProvisionado,
                    ValorReal = lancamento.ValorReal,
                    DataInicial = lancamento.DataInicial,
                    Tipo = lancamento.Tipo,
                    Status = lancamento.Status,
                    ContaId = lancamento.ContaId,
                    CategoriaId = lancamento.CategoriaId,
                    UsuarioId = userId,
                    DataCriacao = DateTime.Now,
                    Ativo = true
                };
                
                _context.Lancamentos.Add(lancamentoEsporadico);
            }

            await _context.SaveChangesAsync();

            return CreatedAtAction("GetLancamento", new { id = lancamento.Id }, lancamento);
        }

        // PUT: api/Lancamentos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLancamento(int id, AtualizarLancamentoDto atualizarDto)
        {
            if (id != atualizarDto.Id)
            {
                return BadRequest("ID do lançamento não confere.");
            }

            var userId = GetUserId();
            
            // Buscar o lançamento existente
            var lancamento = await _context.Lancamentos
                .FirstOrDefaultAsync(l => l.Id == id && l.UsuarioId == userId);

            if (lancamento == null)
            {
                return NotFound("Lançamento não encontrado ou não pertence ao usuário.");
            }

            // Verificar se a conta existe e pertence ao usuário
            var conta = await _context.Contas
                .Include(c => c.ContaUsuarios)
                .FirstOrDefaultAsync(c => c.Id == atualizarDto.ContaId && c.ContaUsuarios.Any(cu => cu.UsuarioId == userId && cu.Ativo));
            
            if (conta == null)
            {
                return BadRequest("Conta não encontrada ou não pertence ao usuário.");
            }

            // Aplicar as alterações do DTO ao lançamento existente
            atualizarDto.AplicarAlteracoes(lancamento);

            try
            {
                await _context.SaveChangesAsync();
                return NoContent();
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
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao atualizar lançamento: {ex.Message}");
            }
        }

        // POST: api/Lancamentos/5/realizar
        [HttpPost("{id}/realizar")]
        public async Task<IActionResult> RealizarLancamento(int id)
        {
            var userId = GetUserId();
            
            var lancamento = await _context.Lancamentos
                .FirstOrDefaultAsync(l => l.Id == id && l.UsuarioId == userId);
            
            if (lancamento == null)
            {
                return NotFound("Lançamento não encontrado ou não pertence ao usuário.");
            }
            
            if (lancamento.Status == StatusLancamento.Realizado)
            {
                return BadRequest("Lançamento já foi realizado.");
            }
            
            // Marcar como realizado
            lancamento.Status = StatusLancamento.Realizado;
            
            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Lançamento realizado com sucesso." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao realizar lançamento: {ex.Message}");
            }
        }

        // DELETE: api/Lancamentos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLancamento(int id)
        {
            var userId = GetUserId();
            
            // Buscar o lançamento incluindo a conta e os lançamentos de folha relacionados
            var lancamento = await _context.Lancamentos
                .Include(l => l.Conta)
                .ThenInclude(c => c.ContaUsuarios)
                .Include(l => l.LancamentosFolha)
                .FirstOrDefaultAsync(l => l.Id == id && l.UsuarioId == userId);
            
            if (lancamento == null)
            {
                return NotFound("Lançamento não encontrado ou não pertence ao usuário.");
            }

            // Verificar se o usuário tem acesso à conta do lançamento
            var temAcesso = lancamento.Conta.ContaUsuarios.Any(cu => cu.UsuarioId == userId && cu.Ativo);
            if (!temAcesso)
            {
                return Forbid("Usuário não tem acesso a esta conta.");
            }

            try
            {
                // Primeiro, remover todos os lançamentos de folha relacionados
                if (lancamento.LancamentosFolha.Any())
                {
                    _context.LancamentosFolha.RemoveRange(lancamento.LancamentosFolha);
                }

                // Depois, remover o lançamento principal
                _context.Lancamentos.Remove(lancamento);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
            }
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

