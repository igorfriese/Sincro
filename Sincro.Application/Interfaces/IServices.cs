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
        Task<List<ResponsavelDto>> ListarResponsaveisAsync();
        Task<UsuarioListaDto> ObterAsync(string id, string? usuarioLogadoId, bool usuarioLogadoEhAdmin);
        Task<UsuarioListaDto> CriarAsync(CriarUsuarioDto dto);
        Task<UsuarioListaDto> AtualizarAsync(string id, AtualizarUsuarioDto dto);
        Task DeletarAsync(string id, string? usuarioLogadoId);
        Task<PerfilDto> ObterPerfilAsync(string? usuarioLogadoId);
        Task<PerfilDto> AtualizarPerfilAsync(string? usuarioLogadoId, AtualizarMeuPerfilDto dto);
        Task AlterarSenhaAsync(string? usuarioLogadoId, AlterarSenhaDto dto);
    }

    public interface IClienteService
    {
        // incluirToken = false para o Vendedor (não distribui links do portal)
        Task<ListaClientesDto> ListarAsync(int pagina, int tamanho, bool incluirToken);
        Task<ClienteDto> ObterAsync(int id, bool incluirToken);
        Task<ClienteDto> CriarAsync(CriarClienteDto dto);
        Task<ClienteDto> AtualizarAsync(int id, AtualizarClienteDto dto);
        Task DeletarAsync(int id);
    }

    public interface IProdutoService
    {
        Task<ListaProdutosDto> ListarAsync(int pagina, int tamanho);
        Task<ProdutoDto> ObterAsync(int id);
        Task<ProdutoDto> CriarAsync(CriarProdutoDto dto);
        Task<ProdutoDto> AtualizarAsync(int id, AtualizarProdutoDto dto);
        Task DeletarAsync(int id);
    }
}