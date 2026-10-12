using System.ComponentModel.DataAnnotations;

namespace Sincro.Application.DTOs
{
    // ---------- Eventos (timeline) ----------
    // POST /api/eventos é só anotação: a etapa é a atual do pedido e a data é a do servidor.
    // Mudança de etapa é pelo PUT /api/pedidos/{id}/etapa, que também registra o evento.
    public record CriarAnotacaoDto(
        [Range(1, int.MaxValue, ErrorMessage = "Pedido é obrigatório")] int PedidoId,
        [Required(ErrorMessage = "Observação é obrigatória")]
        [MaxLength(500, ErrorMessage = "Observação deve ter no máximo 500 caracteres")]
        string Observacao);

    public record EventoDto(
        int Id,
        int PedidoId,
        string Etapa,
        DateTime DataHora,
        string? Observacao,
        string? RegistradoPorId,
        string? RegistradoPorNome);

    // Portal público: sem dados de quem registrou
    public record EventoPublicoDto(int Id, string Etapa, DateTime DataHora, string? Observacao);
}
