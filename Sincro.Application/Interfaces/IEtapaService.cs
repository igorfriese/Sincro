using Sincro.Application.DTOs;

namespace Sincro.Application.Interfaces
{
    public interface IEtapaService
    {
        Task<List<EtapaDto>> ListarAsync();
        Task<EtapaDto> ObterAsync(int id);
        Task<EtapaDto> CriarAsync(CriarEtapaDto dto);
        Task AtualizarAsync(int id, AtualizarEtapaDto dto);
        Task ReordenarAsync(ReordenarEtapasDto dto);
        Task RemoverAsync(int id);
    }
}
