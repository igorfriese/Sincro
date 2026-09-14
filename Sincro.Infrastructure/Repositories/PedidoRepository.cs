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
    public class PedidoRepository : IPedidoRepository
    {
        private readonly ApplicationDbContext _context;

        public PedidoRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Pedido>> ListarTodosAsync()
        {
            return await _context.Pedidos.ToListAsync();
        }

        public async Task<Pedido?> ObterPorIdAsync(int id)
        {
            return await _context.Pedidos.FindAsync(id);
        }

        public async Task AdicionarAsync(Pedido pedido)
        {
            await _context.Pedidos.AddAsync(pedido);
        }

        public void Atualizar(Pedido pedido)
        {
            _context.Pedidos.Update(pedido);
        }

        public void Remover(Pedido pedido)
        {
            _context.Pedidos.Remove(pedido);
        }

        public async Task<List<Pedido>> ListarPorClienteIdAsync(int clienteId)
        {
            return await _context.Pedidos
                .Where(p => p.ClienteId == clienteId)
                .ToListAsync();
        }
    }
}
