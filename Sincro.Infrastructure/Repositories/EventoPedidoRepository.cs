using Microsoft.EntityFrameworkCore;
using Sincro.Domain.Entities;
using Sincro.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sincro.Domain.Interfaces;

namespace Sincro.Infrastructure.Repositories
{
    public class EventoPedidoRepository : IEventoPedidoRepository
    {
        private readonly ApplicationDbContext _context;

        public EventoPedidoRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<EventoPedido?> ObterPorIdAsync(int id)
        {
            return await _context.EventoPedidos.FindAsync(id);
        }

        public async Task<List<EventoPedido>> ListarPorPedidoIdAsync(int pedidoId)
        {
            return await _context.EventoPedidos
                .AsNoTracking()
                .Where(e => e.PedidoId == pedidoId)
                .OrderBy(e => e.DataHora)
                .ToListAsync();
        }

        // Uma consulta só para todos os pedidos (evita N+1 nos relatórios)
        public async Task<List<EventoPedido>> ListarPorEtapaAsync(string chaveEtapa)
        {
            return await _context.EventoPedidos
                .AsNoTracking()
                .Where(e => e.Etapa == chaveEtapa)
                .ToListAsync();
        }

        public async Task AdicionarAsync(EventoPedido evento)
        {
            await _context.EventoPedidos.AddAsync(evento);
        }
    }
}
