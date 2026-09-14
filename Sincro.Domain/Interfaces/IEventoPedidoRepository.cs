using Sincro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sincro.Domain.Interfaces
{
    public interface IEventoPedidoRepository
    {
        public interface IEventoPedidoRepository
        {
            Task<List<EventoPedido>> ListarPorPedidoIdAsync(int pedidoId);
            Task AdicionarAsync(EventoPedido evento);
        }
    }
}
