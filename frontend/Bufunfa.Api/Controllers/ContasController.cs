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
            return await _context.Contas.Where(c => c.UsuarioId == userId).ToListAsync();
        }

        // GET: api/Contas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Conta>> GetConta(int id)
        {
            var userId = GetUserId();
            var conta = await _context.Contas.FirstOrDefaultAsync(c => c.Id == id && c.UsuarioId == userId);

            if (conta == null)
            {
                return NotFound();
            }

            return conta;
        }

        // POST: api/Contas
        [HttpPost]
        public async Task<ActionResult<Conta>> PostConta(Conta conta)
        {
            var userId = GetUserId();
            conta.UsuarioId = userId;

            _context.Contas.Add(conta);
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
            if (conta.UsuarioId != userId)
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
            var conta = await _context.Contas.FirstOrDefaultAsync(c => c.Id == id && c.UsuarioId == userId);
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

