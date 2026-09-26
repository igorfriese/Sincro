namespace Sincro.Presentation.Dtos
{
    public record LoginRequestDto(string Email, string Senha);

    public record UsuarioDto(string Id, string Nome, string Email, string Perfil);

    public record LoginResponseDto(string Token, string RefreshToken, UsuarioDto Usuario);

    public record ErroDto(string Erro);

    public record AtualizarMeuPerfilDto(string Nome, string? Telefone);

    public record RefreshTokenRequestDto(string RefreshToken);
}
