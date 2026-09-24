using Sincro.Domain.Entities;
using Sincro.Domain.Interfaces;
using Sincro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sincro.Infrastructure.Repositories
{
    public class ProdutoRepository : IProdutoRepository
    {
        private readonly ApplicationDbContext _context;

        public ProdutoRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Produto>> ListarTodosAsync()
        {
            return await _context.Produtos.ToListAsync();
        }

        public async Task<Produto?> ObterPorIdAsync(int id)
        {
            return await _context.Produtos.FindAsync(id);
        }

        public async Task AdicionarAsync(Produto produto)
        {
            await _context.Produtos.AddAsync(produto);
        }

        public void Atualizar(Produto produto)
        {
            _context.Produtos.Update(produto);
        }

        public void Remover(Produto produto)
        {
            _context.Produtos.Remove(produto);
        }
        public async Task SalvarAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
