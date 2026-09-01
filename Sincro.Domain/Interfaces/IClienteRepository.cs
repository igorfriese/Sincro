using Sincro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sincro.Domain.Interfaces
{
    public interface IClienteRepository
    {
        Task<List<Cliente>> ListarTodosAsync();
        Task<Cliente?> ObterPorIdAsync(int id);
        Task AdicionarAsync(Cliente cliente);
        void Atualizar(Cliente cliente);
        void Remover(Cliente cliente);

    }
}
