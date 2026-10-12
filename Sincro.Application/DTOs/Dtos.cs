using System.ComponentModel.DataAnnotations;

namespace Sincro.Application.DTOs
{
    // ---------- Comuns ----------
    public record ErroDto(string Erro);
    public record MensagemDto(string Mensagem);

    // ---------- Auth ----------
    public record LoginRequestDto(
        [Required(ErrorMessage = "Email é obrigatório")] string Email,
        [Required(ErrorMessage = "Senha é obrigatória")] string Senha);

    public record RefreshTokenRequestDto(
        [Required(ErrorMessage = "Refresh token é obrigatório")] string RefreshToken);

    public record UsuarioDto(string Id, string Nome, string Email, string Perfil);
    public record LoginResponseDto(string Token, string RefreshToken, UsuarioDto Usuario);

    // ---------- Usuários ----------
    public record CriarUsuarioDto(
        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        string Email,

        [Required(ErrorMessage = "Senha é obrigatória")]
        [MinLength(6, ErrorMessage = "Senha deve ter no mínimo 6 caracteres")]
        string Senha,

        [Required(ErrorMessage = "Nome é obrigatório")]
        string Nome,

        [Required(ErrorMessage = "Role é obrigatório")]
        string Role);

    // Administrador editando outro usuário: NovaSenha vazia mantém a senha atual
    public record AtualizarUsuarioDto(
        [Required(ErrorMessage = "Nome é obrigatório")]
        string Nome,

        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        string Email,

        [Required(ErrorMessage = "Perfil é obrigatório")]
        string Role,

        [MinLength(6, ErrorMessage = "Senha deve ter no mínimo 6 caracteres")]
        string? NovaSenha);

    public record AtualizarMeuPerfilDto(
        [Required(ErrorMessage = "Nome é obrigatório")] string Nome,
        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")] string Email,
        string? Telefone);

    public record AlterarSenhaDto(
        [Required(ErrorMessage = "Informe a senha atual")] string SenhaAnterior,
        [Required(ErrorMessage = "Informe a nova senha")]
        [MinLength(6, ErrorMessage = "A nova senha deve ter no mínimo 6 caracteres")] string NovaSenha);

    public record UsuarioListaDto(string Id, string Email, string Nome, string Role, DateTime DataCriacao);
    public record ListaUsuariosDto(int Total, IReadOnlyList<UsuarioListaDto> Usuarios);
    public record PerfilDto(string Id, string? Email, string Nome, string? Role);

    // Lista enxuta para escolher o responsável de um pedido (Administrador e Gestor)
    public record ResponsavelDto(string Id, string Nome, string Role);

    // ---------- Clientes ----------
    // Documento = CNPJ (o mesmo nome da entidade e do front)
    public record CriarClienteDto(
        [Required(ErrorMessage = "Nome é obrigatório")] string Nome,
        [Required(ErrorMessage = "CNPJ é obrigatório")] string Documento,
        [EmailAddress(ErrorMessage = "Email inválido")] string? Email,
        [Phone(ErrorMessage = "Telefone inválido")] string? Telefone,
        string? Endereco);

    public record AtualizarClienteDto(
        [Required(ErrorMessage = "Nome é obrigatório")] string Nome,
        [Required(ErrorMessage = "CNPJ é obrigatório")] string Documento,
        [EmailAddress(ErrorMessage = "Email inválido")] string? Email,
        [Phone(ErrorMessage = "Telefone inválido")] string? Telefone,
        string? Endereco);

    // TokenAcompanhamento vem null para o Vendedor (o link do portal é de Administrador/Gestor)
    public record ClienteDto(
        int Id,
        string Codigo,
        string Nome,
        string? Documento,
        string? Telefone,
        string? Email,
        string? Endereco,
        string? TokenAcompanhamento);

    public record ListaClientesDto(int Total, int Pagina, int Tamanho, IReadOnlyList<ClienteDto> Clientes);

    // ---------- Produtos ----------
    public record CriarProdutoDto(
        [Required(ErrorMessage = "Nome é obrigatório")] string Nome,
        string? Descricao,
        [Range(0.01, 99999999.99, ErrorMessage = "Preço deve ser maior que 0")] decimal Preco,
        [Range(0, int.MaxValue, ErrorMessage = "Estoque não pode ser negativo")] int Estoque);

    public record AtualizarProdutoDto(
        [Required(ErrorMessage = "Nome é obrigatório")] string Nome,
        string? Descricao,
        [Range(0.01, 99999999.99, ErrorMessage = "Preço deve ser maior que 0")] decimal Preco,
        [Range(0, int.MaxValue, ErrorMessage = "Estoque não pode ser negativo")] int Estoque);

    public record ProdutoDto(int Id, string Codigo, string Nome, string? Descricao, decimal Preco, int Estoque);
    public record ListaProdutosDto(int Total, int Pagina, int Tamanho, IReadOnlyList<ProdutoDto> Produtos);
}