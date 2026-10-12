using Sincro.Application.DTOs;

namespace Sincro.Application.Interfaces
{
    public interface IEventoService
    {
        Task<List<EventoDto>> ListarPorPedidoAsync(int pedidoId, UsuarioAtual usuario);
        Task<EventoDto> ObterAsync(int id, UsuarioAtual usuario);
        Task<EventoDto> CriarAnotacaoAsync(CriarAnotacaoDto dto, UsuarioAtual usuario);
        Task<List<EventoPublicoDto>> ListarPublicoAsync(string token, int pedidoId);
    }
}
