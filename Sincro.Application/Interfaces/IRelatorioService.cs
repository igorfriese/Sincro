using Sincro.Application.DTOs;

namespace Sincro.Application.Interfaces
{
    public interface IRelatorioService
    {
        // Período opcional: sem datas, considera todos os pedidos finalizados
        Task<ProducaoDto> ProducaoAsync(DateTime? dataInicio, DateTime? dataFim);
        Task<PontualidadeDto> PontualidadeAsync(DateTime? dataInicio, DateTime? dataFim);
    }
}
