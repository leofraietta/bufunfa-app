using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Bufunfa.Api.Data;
using Bufunfa.Api.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

public class CategoriaCreateDto
{
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public decimal ValorProvisionadoMensal { get; set; }
}

namespace Bufunfa.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoriasController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int GetUserId()
        {
            // Temporário: retorna usuário padrão para teste (ID = 1)
            // TODO: Implementar autenticação adequada
            return 1;
        }

        // GET: api/Categorias
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Categoria>>> GetCategorias()
        {
            var userId = GetUserId();
            return await _context.Categorias.Where(c => c.UsuarioId == userId).ToListAsync();
        }

        // GET: api/Categorias/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Categoria>> GetCategoria(int id)
        {
            var userId = GetUserId();
            var categoria = await _context.Categorias.FirstOrDefaultAsync(c => c.Id == id && c.UsuarioId == userId);

            if (categoria == null)
            {
                return NotFound();
            }

            return categoria;
        }

        // POST: api/Categorias/test
        [HttpPost("test")]
        [AllowAnonymous]
        public async Task<ActionResult<string>> PostCategoriaTest([FromBody] object data)
        {
            Console.WriteLine("=== MÉTODO TEST CHAMADO ===");
            Console.WriteLine($"Dados recebidos: {System.Text.Json.JsonSerializer.Serialize(data)}");
            return Ok("Método test funcionando");
        }

        // POST: api/Categorias
        [HttpPost]
        [AllowAnonymous] // Temporariamente para debug
        public async Task<ActionResult<Categoria>> PostCategoria([FromBody] CategoriaCreateDto dto)
        {
            Console.WriteLine("=== MÉTODO POST CATEGORIA CHAMADO ===");
            
            if (dto == null || string.IsNullOrEmpty(dto.Nome))
            {
                return BadRequest(new { error = "Nome da categoria é obrigatório" });
            }

            var categoria = new Categoria
            {
                Nome = dto.Nome,
                Descricao = dto.Descricao ?? "",
                ValorProvisionadoMensal = dto.ValorProvisionadoMensal,
                UsuarioId = 1, // Temporariamente fixo para debug
                DataCriacao = DateTime.UtcNow,
                Ativa = true
            };

            _context.Categorias.Add(categoria);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCategoria", new { id = categoria.Id }, categoria);
        }

        // PUT: api/Categorias/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategoria(int id, Categoria categoria)
        {
            if (id != categoria.Id)
            {
                return BadRequest();
            }

            var userId = GetUserId();
            if (categoria.UsuarioId != userId)
            {
                return Forbid(); // Usuário não tem permissão para alterar esta categoria
            }

            // Atualizar data de modificação
            categoria.DataAtualizacao = DateTime.UtcNow;

            _context.Entry(categoria).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoriaExists(id))
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

        // GET: api/Categorias/5/saldo/2024/1
        [HttpGet("{id}/saldo/{ano}/{mes}")]
        public async Task<ActionResult<object>> GetSaldoCategoria(int id, int ano, int mes)
        {
            var userId = GetUserId();
            var categoria = await _context.Categorias
                .Include(c => c.Lancamentos)
                .FirstOrDefaultAsync(c => c.Id == id && c.UsuarioId == userId);

            if (categoria == null)
            {
                return NotFound();
            }

            var gastoReal = categoria.CalcularGastoReal(ano, mes);
            var saldo = categoria.CalcularSaldo(ano, mes);

            return Ok(new
            {
                categoria.ValorProvisionadoMensal,
                GastoReal = gastoReal,
                Saldo = saldo,
                Ano = ano,
                Mes = mes
            });
        }

        // DELETE: api/Categorias/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategoria(int id)
        {
            var userId = GetUserId();
            var categoria = await _context.Categorias.FirstOrDefaultAsync(c => c.Id == id && c.UsuarioId == userId);
            if (categoria == null)
            {
                return NotFound();
            }

            _context.Categorias.Remove(categoria);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CategoriaExists(int id)
        {
            return _context.Categorias.Any(e => e.Id == id);
        }
    }
}

