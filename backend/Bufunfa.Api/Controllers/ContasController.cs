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
    public class ContasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ContasController(ApplicationDbContext context)
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

        // GET: api/Contas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Conta>>> GetContas()
        {
            var userId = GetUserId();
            
            // Buscar contas através do relacionamento ContaUsuario
            var contas = await _context.Contas
                .Include(c => c.ContaUsuarios)
                .Where(c => c.ContaUsuarios.Any(cu => cu.UsuarioId == userId && cu.Ativo))
                .ToListAsync();
                
            return contas;
        }

        // GET: api/Contas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Conta>> GetConta(int id)
        {
            var userId = GetUserId();
            
            // Buscar conta através do relacionamento ContaUsuario
            var conta = await _context.Contas
                .Include(c => c.ContaUsuarios)
                .FirstOrDefaultAsync(c => c.Id == id && c.ContaUsuarios.Any(cu => cu.UsuarioId == userId && cu.Ativo));

            if (conta == null)
            {
                return NotFound();
            }

            return conta;
        }

        // POST: api/Contas
        [HttpPost]
        public async Task<ActionResult<Conta>> PostConta(CriarContaDto criarContaDto)
        {
            var userId = GetUserId();
            
            // Converter DTO para entidade
            var conta = criarContaDto.ToConta();
            
            // Adicionar a conta
            _context.Contas.Add(conta);
            await _context.SaveChangesAsync();
            
            // Criar relacionamento ContaUsuario como proprietário
            var contaUsuario = new ContaUsuario
            {
                ContaId = conta.Id,
                UsuarioId = userId,
                EhProprietario = true,
                PercentualParticipacao = 100.00m,
                PodeLer = true,
                PodeEscrever = true,
                PodeAdministrar = true
            };
            
            _context.ContaUsuarios.Add(contaUsuario);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetConta", new { id = conta.Id }, conta);
        }

        // PUT: api/Contas/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutConta(int id, Conta conta)
        {
            if (id != conta.Id)
            {
                return BadRequest();
            }

            var userId = GetUserId();
            
            // Verificar se o usuário tem permissão para alterar esta conta
            var temPermissao = await _context.ContaUsuarios
                .AnyAsync(cu => cu.ContaId == id && cu.UsuarioId == userId && cu.Ativo && cu.PodeEscrever);
                
            if (!temPermissao)
            {
                return Forbid(); // Usuário não tem permissão para alterar esta conta
            }

            _context.Entry(conta).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ContaExists(id))
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

        // DELETE: api/Contas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteConta(int id)
        {
            var userId = GetUserId();
            
            // Verificar se o usuário tem permissão para deletar esta conta
            var conta = await _context.Contas
                .Include(c => c.ContaUsuarios)
                .FirstOrDefaultAsync(c => c.Id == id && c.ContaUsuarios.Any(cu => cu.UsuarioId == userId && cu.Ativo && cu.PodeAdministrar));
                
            if (conta == null)
            {
                return NotFound();
            }

            _context.Contas.Remove(conta);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ContaExists(int id)
        {
            return _context.Contas.Any(e => e.Id == id);
        }
    }
}

