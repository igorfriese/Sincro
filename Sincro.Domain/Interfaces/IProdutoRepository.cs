using Sincro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sincro.Domain.Interfaces
{
    public interface IProdutoRepository
    {
        Task<List<Produto>> ListarTodosAsync();
        Task<Produto?> ObterPorIdAsync(int id);
        Task AdicionarAsync(Produto produto);
        void Atualizar(Produto produto);
        void Remover(Produto produto);
    }
}
