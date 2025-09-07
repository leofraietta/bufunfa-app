using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Bufunfa.Api.Data;
using Bufunfa.Api.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Bufunfa.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ContasConjuntasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ContasConjuntasController(ApplicationDbContext context)
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

        // GET: api/ContasConjuntas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ContaConjunta>>> GetContasConjuntas()
        {
            var userId = GetUserId();
            return await _context.ContasConjuntas
                .Include(cc => cc.Rateios)
                .ThenInclude(r => r.Usuario)
                .Where(cc => cc.UsuarioCriadorId == userId || cc.Rateios.Any(r => r.UsuarioId == userId))
                .ToListAsync();
        }

        // GET: api/ContasConjuntas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ContaConjunta>> GetContaConjunta(int id)
        {
            var userId = GetUserId();
            var contaConjunta = await _context.ContasConjuntas
                .Include(cc => cc.Rateios)
                .ThenInclude(r => r.Usuario)
                .FirstOrDefaultAsync(cc => cc.Id == id && (cc.UsuarioCriadorId == userId || cc.Rateios.Any(r => r.UsuarioId == userId)));

            if (contaConjunta == null)
            {
                return NotFound();
            }

            return contaConjunta;
        }

        // POST: api/ContasConjuntas
        [HttpPost]
        public async Task<ActionResult<ContaConjunta>> PostContaConjunta(ContaConjunta contaConjunta)
        {
            var userId = GetUserId();
            contaConjunta.SaldoAtual = 0; // Saldo inicial sempre zero

            _context.ContasConjuntas.Add(contaConjunta);
            await _context.SaveChangesAsync();

            // Criar relacionamento ContaUsuario como proprietário/criador com 100% de participação
            var contaUsuario = new ContaUsuario
            {
                ContaId = contaConjunta.Id,
                UsuarioId = userId,
                EhProprietario = true,
                PercentualParticipacao = 100.00m,
                NivelPermissao = PermissionLevel.FullAccess,
                EhAdministrador = true
            };
            
            _context.ContaUsuarios.Add(contaUsuario);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetContaConjunta", new { id = contaConjunta.Id }, contaConjunta);
        }

        // PUT: api/ContasConjuntas/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutContaConjunta(int id, ContaConjunta contaConjunta)
        {
            if (id != contaConjunta.Id)
            {
                return BadRequest();
            }

            var userId = GetUserId();
            var existingContaConjunta = await _context.ContasConjuntas.FindAsync(id);

            if (existingContaConjunta == null || (existingContaConjunta.UsuarioCriadorId != userId && !existingContaConjunta.Rateios.Any(r => r.UsuarioId == userId)))
            {
                return Forbid();
            }

            _context.Entry(contaConjunta).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ContaConjuntaExists(id))
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

        // DELETE: api/ContasConjuntas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteContaConjunta(int id)
        {
            var userId = GetUserId();
            var contaConjunta = await _context.ContasConjuntas.FirstOrDefaultAsync(cc => cc.Id == id && cc.UsuarioCriadorId == userId);
            if (contaConjunta == null)
            {
                return NotFound();
            }

            _context.ContasConjuntas.Remove(contaConjunta);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/ContasConjuntas/{contaConjuntaId}/adicionar-usuario
        [HttpPost("{contaConjuntaId}/adicionar-usuario")]
        public async Task<ActionResult<Rateio>> AdicionarUsuarioContaConjunta(int contaConjuntaId, [FromBody] string emailUsuario)
        {
            var userId = GetUserId();
            var contaConjunta = await _context.ContasConjuntas
                .Include(cc => cc.Rateios)
                .FirstOrDefaultAsync(cc => cc.Id == contaConjuntaId && cc.UsuarioCriadorId == userId);

            if (contaConjunta == null)
            {
                return NotFound("Conta conjunta não encontrada ou você não é o criador.");
            }

            var usuarioConvidado = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == emailUsuario);
            if (usuarioConvidado == null)
            {
                return BadRequest("Usuário convidado não encontrado.");
            }

            if (contaConjunta.Rateios.Any(r => r.UsuarioId == usuarioConvidado.Id))
            {
                return BadRequest("Usuário já faz parte desta conta conjunta.");
            }

            var novoRateio = new Rateio
            {
                ContaConjuntaId = contaConjuntaId,
                UsuarioId = usuarioConvidado.Id,
                PercentualRateio = 0 // Percentual inicial, pode ser ajustado depois
            };

            _context.Rateios.Add(novoRateio);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetContaConjunta", new { id = contaConjuntaId }, novoRateio);
        }

        // PUT: api/ContasConjuntas/{contaConjuntaId}/atualizar-rateio/{rateioId}
        [HttpPut("{contaConjuntaId}/atualizar-rateio/{rateioId}")]
        public async Task<IActionResult> AtualizarRateio(int contaConjuntaId, int rateioId, [FromBody] decimal percentual)
        {
            var userId = GetUserId();
            var contaConjunta = await _context.ContasConjuntas
                .Include(cc => cc.Rateios)
                .FirstOrDefaultAsync(cc => cc.Id == contaConjuntaId && cc.UsuarioCriadorId == userId);

            if (contaConjunta == null)
            {
                return NotFound("Conta conjunta não encontrada ou você não é o criador.");
            }

            var rateio = contaConjunta.Rateios.FirstOrDefault(r => r.Id == rateioId);
            if (rateio == null)
            {
                return NotFound("Rateio não encontrado.");
            }

            rateio.PercentualRateio = percentual;
            _context.Entry(rateio).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/ContasConjuntas/{contaConjuntaId}/apurar
        [HttpPost("{contaConjuntaId}/apurar")]
        public async Task<IActionResult> ApurarContaConjunta(int contaConjuntaId)
        {
            var userId = GetUserId();
            var contaConjunta = await _context.ContasConjuntas
                .Include(cc => cc.Rateios)
                .ThenInclude(r => r.Usuario)
                .FirstOrDefaultAsync(cc => cc.Id == contaConjuntaId && (cc.UsuarioCriadorId == userId || cc.Rateios.Any(r => r.UsuarioId == userId)));

            if (contaConjunta == null)
            {
                return NotFound("Conta conjunta não encontrada ou você não tem permissão.");
            }

            // Calcula o saldo total dos lançamentos da conta conjunta desde a última apuração
            var dataUltimaApuracao = contaConjunta.DataUltimaApuracao ?? DateTime.MinValue;
            var lancamentosContaConjunta = await _context.Lancamentos
                .Where(l => l.ContaId == contaConjunta.Id && l.Data >= dataUltimaApuracao)
                .ToListAsync();

            var totalReceitas = lancamentosContaConjunta.Where(l => l.Tipo == TipoLancamento.Receita).Sum(l => l.Valor);
            var totalDespesas = lancamentosContaConjunta.Where(l => l.Tipo == TipoLancamento.Despesa).Sum(l => l.Valor);
            var saldoApurado = totalReceitas - totalDespesas;

            if (saldoApurado < 0)
            {
                // Saldo devedor: criar despesas nas contas principais dos usuários
                foreach (var rateio in contaConjunta.Rateios)
                {
                    var valorRateado = Math.Abs(saldoApurado) * (rateio.PercentualRateio / 100);
                    var contaPrincipalUsuario = await _context.Contas.FirstOrDefaultAsync(c => c.UsuarioId == rateio.UsuarioId && c.Tipo == TipoConta.ContaCorrente);

                    if (contaPrincipalUsuario != null)
                    {
                        _context.Lancamentos.Add(new LancamentoEsporadico
                        {
                            Descricao = $"Rateio Despesa Conta Conjunta {contaConjunta.Nome}",
                            ValorProvisionado = valorRateado,
                            DataInicial = DateTime.Now,
                            Tipo = TipoLancamento.Despesa,
                            ContaId = contaPrincipalUsuario.Id,
                            UsuarioId = rateio.UsuarioId,
                            DataCriacao = DateTime.Now,
                            Ativo = true
                        });
                    }
                }
            }
            else if (saldoApurado > 0)
            {
                // Saldo credor
                if (!contaConjunta.ManterSaldoPositivo)
                {
                    // Distribuir como receita nas contas principais dos usuários
                    foreach (var rateio in contaConjunta.Rateios)
                    {
                        var valorRateado = saldoApurado * (rateio.PercentualRateio / 100);
                        var contaPrincipalUsuario = await _context.Contas.FirstOrDefaultAsync(c => c.UsuarioId == rateio.UsuarioId && c.Tipo == TipoConta.ContaCorrente);

                        if (contaPrincipalUsuario != null)
                        {
                            _context.Lancamentos.Add(new LancamentoEsporadico
                            {
                                Descricao = $"Rateio Receita Conta Conjunta {contaConjunta.Nome}",
                                ValorProvisionado = valorRateado,
                                DataInicial = DateTime.Now,
                                Tipo = TipoLancamento.Receita,
                                ContaId = contaPrincipalUsuario.Id,
                                UsuarioId = rateio.UsuarioId,
                                DataCriacao = DateTime.Now,
                                Ativo = true
                            });
                        }
                    }
                    contaConjunta.SaldoAtual = 0; // Zera o saldo após distribuição
                }
                else
                {
                    // Manter o saldo na conta conjunta
                    contaConjunta.SaldoAtual += saldoApurado;
                }
            }

            contaConjunta.DataUltimaApuracao = DateTime.Now; // Atualiza a data da última apuração
            _context.Entry(contaConjunta).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return Ok("Apuração da conta conjunta realizada com sucesso.");
        }

        private bool ContaConjuntaExists(int id)
        {
            return _context.ContasConjuntas.Any(e => e.Id == id);
        }
    }
}

