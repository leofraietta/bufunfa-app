using Bufunfa.Api.Data;
using Bufunfa.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Bufunfa.Api.Services
{
    public interface IProvisionamentoService
    {
        Task<ProvisionamentoMercado> CriarProvisionamentoMercado(int usuarioId, int contaId, int categoriaId, decimal valorMensal, int ano, int mes);
        Task<ProvisionamentoMercado> ObterProvisionamentoMercado(int usuarioId, int contaId, int categoriaId, int ano, int mes);
        Task AdicionarGastoReal(int provisionamentoId, decimal valor, string descricao, DateTime data);
        Task<ResumoProvisionamento> ObterResumoProvisionamento(int provisionamentoId);
        Task<List<ResumoProvisionamento>> ObterTodosProvisionamentos(int usuarioId, int ano, int mes);
    }

    public class ProvisionamentoService : IProvisionamentoService
    {
        private readonly ApplicationDbContext _context;

        public ProvisionamentoService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ProvisionamentoMercado> CriarProvisionamentoMercado(int usuarioId, int contaId, int categoriaId, decimal valorMensal, int ano, int mes)
        {
            // Verificar se já existe provisionamento para este período
            var existente = await _context.ProvisionamentosMercado
                .FirstOrDefaultAsync(p => p.UsuarioId == usuarioId && 
                                         p.ContaId == contaId && 
                                         p.CategoriaId == categoriaId && 
                                         p.Ano == ano && 
                                         p.Mes == mes);

            if (existente != null)
            {
                existente.ValorProvisionado = valorMensal;
                existente.DataAtualizacao = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return existente;
            }

            var novoProvisionamento = new ProvisionamentoMercado
            {
                UsuarioId = usuarioId,
                ContaId = contaId,
                CategoriaId = categoriaId,
                Ano = ano,
                Mes = mes,
                ValorProvisionado = valorMensal,
                ValorGastoReal = 0,
                DataCriacao = DateTime.UtcNow
            };

            _context.ProvisionamentosMercado.Add(novoProvisionamento);
            await _context.SaveChangesAsync();

            return novoProvisionamento;
        }

        public async Task<ProvisionamentoMercado> ObterProvisionamentoMercado(int usuarioId, int contaId, int categoriaId, int ano, int mes)
        {
            return await _context.ProvisionamentosMercado
                .Include(p => p.Conta)
                .Include(p => p.Categoria)
                .Include(p => p.GastosReais)
                .FirstOrDefaultAsync(p => p.UsuarioId == usuarioId && 
                                         p.ContaId == contaId && 
                                         p.CategoriaId == categoriaId && 
                                         p.Ano == ano && 
                                         p.Mes == mes);
        }

        public async Task AdicionarGastoReal(int provisionamentoId, decimal valor, string descricao, DateTime data)
        {
            var provisionamento = await _context.ProvisionamentosMercado.FindAsync(provisionamentoId);
            if (provisionamento == null)
            {
                throw new ArgumentException("Provisionamento não encontrado");
            }

            var gastoReal = new GastoRealMercado
            {
                ProvisionamentoMercadoId = provisionamentoId,
                Valor = valor,
                Descricao = descricao,
                Data = data,
                DataCriacao = DateTime.UtcNow
            };

            _context.GastosReaisMercado.Add(gastoReal);

            // Atualizar valor total gasto
            provisionamento.ValorGastoReal += valor;
            provisionamento.DataAtualizacao = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task<ResumoProvisionamento> ObterResumoProvisionamento(int provisionamentoId)
        {
            var provisionamento = await _context.ProvisionamentosMercado
                .Include(p => p.Conta)
                .Include(p => p.Categoria)
                .Include(p => p.GastosReais)
                .FirstOrDefaultAsync(p => p.Id == provisionamentoId);

            if (provisionamento == null)
            {
                return null;
            }

            return new ResumoProvisionamento
            {
                Id = provisionamento.Id,
                ContaNome = provisionamento.Conta.Nome,
                CategoriaNome = provisionamento.Categoria.Nome,
                Ano = provisionamento.Ano,
                Mes = provisionamento.Mes,
                ValorProvisionado = provisionamento.ValorProvisionado,
                ValorGastoReal = provisionamento.ValorGastoReal,
                Diferenca = provisionamento.ValorProvisionado - provisionamento.ValorGastoReal,
                PercentualUtilizado = provisionamento.ValorProvisionado > 0 
                    ? (provisionamento.ValorGastoReal / provisionamento.ValorProvisionado) * 100 
                    : 0,
                QuantidadeGastos = provisionamento.GastosReais.Count,
                GastosReais = provisionamento.GastosReais.Select(g => new GastoRealResumo
                {
                    Id = g.Id,
                    Descricao = g.Descricao,
                    Valor = g.Valor,
                    Data = g.Data
                }).OrderByDescending(g => g.Data).ToList()
            };
        }

        public async Task<List<ResumoProvisionamento>> ObterTodosProvisionamentos(int usuarioId, int ano, int mes)
        {
            var provisionamentos = await _context.ProvisionamentosMercado
                .Include(p => p.Conta)
                .Include(p => p.Categoria)
                .Include(p => p.GastosReais)
                .Where(p => p.UsuarioId == usuarioId && p.Ano == ano && p.Mes == mes)
                .ToListAsync();

            return provisionamentos.Select(p => new ResumoProvisionamento
            {
                Id = p.Id,
                ContaNome = p.Conta.Nome,
                CategoriaNome = p.Categoria.Nome,
                Ano = p.Ano,
                Mes = p.Mes,
                ValorProvisionado = p.ValorProvisionado,
                ValorGastoReal = p.ValorGastoReal,
                Diferenca = p.ValorProvisionado - p.ValorGastoReal,
                PercentualUtilizado = p.ValorProvisionado > 0 
                    ? (p.ValorGastoReal / p.ValorProvisionado) * 100 
                    : 0,
                QuantidadeGastos = p.GastosReais.Count,
                GastosReais = p.GastosReais.Select(g => new GastoRealResumo
                {
                    Id = g.Id,
                    Descricao = g.Descricao,
                    Valor = g.Valor,
                    Data = g.Data
                }).OrderByDescending(g => g.Data).ToList()
            }).ToList();
        }
    }

    // Modelos para provisionamento de mercado
    public class ProvisionamentoMercado
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; }
        public int ContaId { get; set; }
        public Conta Conta { get; set; }
        public int CategoriaId { get; set; }
        public Categoria Categoria { get; set; }
        public int Ano { get; set; }
        public int Mes { get; set; }
        public decimal ValorProvisionado { get; set; }
        public decimal ValorGastoReal { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime? DataAtualizacao { get; set; }
        public ICollection<GastoRealMercado> GastosReais { get; set; } = new List<GastoRealMercado>();
    }

    public class GastoRealMercado
    {
        public int Id { get; set; }
        public int ProvisionamentoMercadoId { get; set; }
        public ProvisionamentoMercado ProvisionamentoMercado { get; set; }
        public string Descricao { get; set; }
        public decimal Valor { get; set; }
        public DateTime Data { get; set; }
        public DateTime DataCriacao { get; set; }
    }

    // DTOs para resposta
    public class ResumoProvisionamento
    {
        public int Id { get; set; }
        public string ContaNome { get; set; }
        public string CategoriaNome { get; set; }
        public int Ano { get; set; }
        public int Mes { get; set; }
        public decimal ValorProvisionado { get; set; }
        public decimal ValorGastoReal { get; set; }
        public decimal Diferenca { get; set; }
        public decimal PercentualUtilizado { get; set; }
        public int QuantidadeGastos { get; set; }
        public List<GastoRealResumo> GastosReais { get; set; }
    }

    public class GastoRealResumo
    {
        public int Id { get; set; }
        public string Descricao { get; set; }
        public decimal Valor { get; set; }
        public DateTime Data { get; set; }
    }
}

