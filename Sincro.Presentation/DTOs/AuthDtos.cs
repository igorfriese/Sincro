namespace Sincro.Presentation.Dtos
{
    public record LoginRequestDto(string Email, string Senha);

    public record UsuarioDto(string Id, string Nome, string Email, string Perfil);

    public record LoginResponseDto(string Token, UsuarioDto Usuario);

    public record ErroDto(string Erro);
}
