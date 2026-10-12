using System.ComponentModel.DataAnnotations;

namespace Sincro.Application.DTOs
{
    // ---------- Pedidos: entrada ----------
    public record CriarPedidoDto(
        [Range(1, int.MaxValue, ErrorMessage = "Cliente é obrigatório")] int ClienteId,
        [Range(1, int.MaxValue, ErrorMessage = "Produto é obrigatório")] int ProdutoId,
        string? ResponsavelId,
        [Range(1, int.MaxValue, ErrorMessage = "Quantidade deve ser maior que 0")] int Quantidade,
        DateTime Prazo,
        bool Urgente,
        string? Coluna);

    // Sem Coluna: mudança de etapa só pelo PUT /api/pedidos/{id}/etapa (regra do Kanban + evento)
    public record AtualizarPedidoDto(
        [Range(1, int.MaxValue, ErrorMessage = "Cliente é obrigatório")] int ClienteId,
        [Range(1, int.MaxValue, ErrorMessage = "Produto é obrigatório")] int ProdutoId,
        string? ResponsavelId,
        [Range(1, int.MaxValue, ErrorMessage = "Quantidade deve ser maior que 0")] int Quantidade,
        DateTime Prazo,
        bool Urgente);

    public record AlterarEtapaDto(
        [Required(ErrorMessage = "Etapa é obrigatória")] string Etapa,
        string? Observacao);

    // ---------- Pedidos: saída (mesmo formato que o front já lê) ----------
    // TokenAcompanhamento vem null para o Vendedor
    public record ClienteResumoDto(int Id, string Nome, string? TokenAcompanhamento);
    public record ProdutoResumoDto(int Id, string Nome);

    public record PedidoDto(
        int Id,
        int ClienteId,
        ClienteResumoDto? Cliente,
        int ProdutoId,
        ProdutoResumoDto? Produto,
        string? ResponsavelId,
        int Quantidade,
        DateTime Prazo,
        bool Urgente,
        string Coluna,
        DateTime? UltimaMovimentacao); // evento mais recente (alerta de "sem movimentação")

    // ---------- Portal público do cliente ----------
    public record ClientePortalDto(int Id, string Nome);
    public record PedidoPortalDto(int Id, string Modelo, int Qtd, DateTime Prazo, bool Urgente, string Coluna);
    public record PortalClienteDto(ClientePortalDto Cliente, IReadOnlyList<PedidoPortalDto> Pedidos);

    // ---------- Quem está fazendo a requisição ----------
    // SomenteProprios = Vendedor: só enxerga e mexe nos pedidos em que é o responsável
    public record UsuarioAtual(string Id, bool SomenteProprios);
}
