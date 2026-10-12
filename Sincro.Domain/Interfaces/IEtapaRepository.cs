using Sincro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sincro.Domain.Interfaces
{
    public interface IEtapaRepository
    {
        Task<List<Etapa>> ListarTodosAsync();
        Task<Etapa?> ObterPorIdAsync(int id);
        Task AdicionarAsync(Etapa etapa);
        void Atualizar(Etapa etapa);
        void Remover(Etapa etapa);
    }
}
