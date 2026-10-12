using Sincro.Application.DTOs;

namespace Sincro.Application.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto> LoginAsync(LoginRequestDto dto);
        Task<LoginResponseDto> RenovarTokenAsync(RefreshTokenRequestDto dto);
    }

    public interface IUsuarioService
    {
        Task<ListaUsuariosDto> ListarAsync(int pagina, int tamanho);
        Task<UsuarioListaDto> ObterAsync(string id, string? usuarioLogadoId, bool usuarioLogadoEhAdmin);
        Task<UsuarioListaDto> CriarAsync(CriarUsuarioDto dto);
        Task AtualizarAsync(string id, AtualizarUsuarioDto dto);
        Task DeletarAsync(string id, string? usuarioLogadoId);
        Task<PerfilDto> ObterPerfilAsync(string? usuarioLogadoId);
        Task AtualizarPerfilAsync(string? usuarioLogadoId, AtualizarMeuPerfilDto dto);
        Task AlterarSenhaAsync(string? usuarioLogadoId, AlterarSenhaDto dto);
    }

    public interface IClienteService
    {
        Task<ListaClientesDto> ListarAsync(int pagina, int tamanho);
        Task<ClienteDetailDto> ObterAsync(int id);
        Task<ClienteListaDto> CriarAsync(CriarClienteDto dto);
        Task AtualizarAsync(int id, AtualizarClienteDto dto);
        Task DeletarAsync(int id);
    }

    public interface IProdutoService
    {
        Task<ListaProdutosDto> ListarAsync(int pagina, int tamanho);
        Task<ProdutoDetailDto> ObterAsync(int id);
        Task<ProdutoListaDto> CriarAsync(CriarProdutoDto dto);
        Task AtualizarAsync(int id, AtualizarProdutoDto dto);
        Task DeletarAsync(int id);
    }
}