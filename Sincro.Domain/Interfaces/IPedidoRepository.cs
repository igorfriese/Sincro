using Sincro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sincro.Domain.Interfaces
{
    public interface IPedidoRepository
    {
        Task<List<Pedido>> ListarTodosAsync();
        Task<Pedido?> ObterPorIdAsync(int id);
        Task AdicionarAsync(Pedido pedido);
        void Atualizar(Pedido pedido);
        void Remover(Pedido pedido);
    }
}
