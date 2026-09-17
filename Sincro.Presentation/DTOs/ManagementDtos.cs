namespace Sincro.Presentation.DTOs;

using System.ComponentModel.DataAnnotations;


//Usuarios
public record CriarUsuarioDto
(
    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    string Email,

    [Required(ErrorMessage = "Senha é obrigatória")]
    [MinLength(6, ErrorMessage = "Senha deve ter no mínimo 6 caracteres")]
    string Senha,

    [Required(ErrorMessage = "Nome é obrigatório")]
    string Nome,

    [Required(ErrorMessage = "Role é obrigatório")]
    string Role
);
public record AtualizarUsuarioDto

(
    [Required]
    string Nome
);
public record UsuarioListaDto(string Id, string Email, string Nome, string Role, DateTime DataCriacao);
public record AlterarSenhaDto

(
    [Required]
    string SenhaAnterior,

    [Required]
    [MinLength(6)]
    string NovaSenha
);

//Clientes
public record CriarClienteDto

(
    [Required(ErrorMessage = "Nome é obrigatório")]
    string Nome,

    [EmailAddress(ErrorMessage = "Email inválido")]
    string Email,

    [Phone(ErrorMessage = "Telefone inválido")]
    string Telefone,

    [Required(ErrorMessage = "Endereço é obrigatório")]
    string Endereco,

    [Required(ErrorMessage = "CNPJ é obrigatório")]
    string CNPJ
);

public record AtualizarClienteDto

(
    [Required]
    string Nome,

    [EmailAddress]
    string Email,

    [Phone]
    string Telefone,

    [Required]
    string Endereco
);

public record ClienteListaDto(int Id, string Nome, string Email, string Telefone, DateTime DataCriacao);
public record ClienteDetailDto(int Id, string Nome, string Email, string Telefone, string Endereco, string Cnpj, bool Ativo, DateTime DataCriacao);

//Produtos
public record CriarProdutoDto
 
(
    [Required(ErrorMessage = "Nome é obrigatório")]
    string Nome,

    [Required(ErrorMessage = "Descrição é obrigatória")]
    string Descricao,

    [Range(0.01, double.MaxValue, ErrorMessage = "Preço deve ser maior que 0")]
    decimal Preco,

    [Range(0, int.MaxValue, ErrorMessage = "Estoque não pode ser negativo")]
    int Estoque
);

public record AtualizarProdutoDto
    
(
    [Required]
    string Nome,

    [Required]
    string Descricao,

    [Range(0.01, double.MaxValue)]
    decimal Preco,

    [Range(0, int.MaxValue)]
    int Estoque
);

public record ProdutoListaDto(int Id, string Nome, decimal Preco, int Estoque, bool Ativo);
public record ProdutoDetailDto(int Id, string Nome, string Descricao, decimal Preco, int Estoque, bool Ativo, DateTime DataCriacao);

//Mensagem erro
public record ErroDto(string Mensagem);