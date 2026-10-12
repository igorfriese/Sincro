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
        Task<EventoPedido?> ObterPorIdAsync(int id);
        Task<List<EventoPedido>> ListarPorPedidoIdAsync(int pedidoId);
        Task<List<EventoPedido>> ListarPorEtapaAsync(string chaveEtapa);
        Task<Dictionary<int, DateTime>> UltimaMovimentacaoPorPedidoAsync(IEnumerable<int> pedidoIds);
        Task AdicionarAsync(EventoPedido evento);
    }
}
