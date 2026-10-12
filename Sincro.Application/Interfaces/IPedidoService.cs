using Sincro.Application.DTOs;

namespace Sincro.Application.Interfaces
{
    public interface IPedidoService
    {
        Task<List<PedidoDto>> ListarAsync(UsuarioAtual usuario);
        Task<PedidoDto> ObterAsync(int id, UsuarioAtual usuario);
        Task<PedidoDto> CriarAsync(CriarPedidoDto dto, UsuarioAtual usuario);
        Task AtualizarAsync(int id, AtualizarPedidoDto dto, UsuarioAtual usuario);
        Task AlterarEtapaAsync(int id, AlterarEtapaDto dto, UsuarioAtual usuario);
        Task RemoverAsync(int id);
        Task<PortalClienteDto> AcompanharPorTokenAsync(string token);
    }
}
