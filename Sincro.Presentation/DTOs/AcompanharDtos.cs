namespace Sincro.Presentation.Dtos
{
    public record PedidoPublicoDto(int Id, string Modelo, int Quantidade, DateTime Prazo, string Coluna, bool Urgente);

    public record ClientePublicoDto(string Nome);

    public record AcompanharResponseDto(ClientePublicoDto Cliente, List<PedidoPublicoDto> Pedidos);
}
