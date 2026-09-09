using Sincro.Domain.Entities;
using Sincro.Infrastructure.Data;
using Sincro.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sincro.Infrastructure.Repositories
{
    public class ClienteRepository : IClienteRepository
    {
        private readonly ApplicationDbContext _context;

        public ClienteRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Cliente>> ListarTodosAsync()
        {
            return await _context.Clientes.ToListAsync();
        }

        public async Task<Cliente?> ObterPorIdAsync(int id)
        {
            return await _context.Clientes.FindAsync(id);
        }

        public async Task AdicionarAsync(Cliente cliente)
        {
            await _context.Clientes.AddAsync(cliente);
        }

        public void Atualizar(Cliente cliente)
        {
            _context.Clientes.Update(cliente);
        }

        public void Remover(Cliente cliente)
        {
            _context.Clientes.Remove(cliente);
        }
    }
}
