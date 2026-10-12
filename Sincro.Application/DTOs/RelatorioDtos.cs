namespace Sincro.Application.DTOs
{
    // ---------- Relatórios ----------
    public record PedidoProduzidoDto(
        int Id,
        string Cliente,
        string Modelo,
        int Quantidade,
        DateTime Prazo,
        DateTime ConcluidoEm,
        DateTime? IniciadoEm,
        bool Pontual);

    public record ProducaoDto(
        DateTime? DataInicio,
        DateTime? DataFim,
        int TotalPedidos,
        int TotalPecas,
        int Pontuais,
        int Atrasados,
        int? TempoMedioDias,
        IReadOnlyList<PedidoProduzidoDto> Pedidos);

    public record PontualidadeDto(int Pontuais, int Atrasados);
}
