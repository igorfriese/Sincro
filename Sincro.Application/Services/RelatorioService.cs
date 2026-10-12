using Sincro.Application.DTOs;
using Sincro.Application.Exceptions;
using Sincro.Application.Interfaces;
using Sincro.Domain.Interfaces;

namespace Sincro.Application.Services
{
    public class RelatorioService : IRelatorioService
    {
        private readonly IPedidoRepository _pedidoRepository;
        private readonly IEventoPedidoRepository _eventoRepository;
        private readonly IEtapaRepository _etapaRepository;

        public RelatorioService(
            IPedidoRepository pedidoRepository,
            IEventoPedidoRepository eventoRepository,
            IEtapaRepository etapaRepository)
        {
            _pedidoRepository = pedidoRepository;
            _eventoRepository = eventoRepository;
            _etapaRepository = etapaRepository;
        }

        public async Task<ProducaoDto> ProducaoAsync(DateTime? dataInicio, DateTime? dataFim)
        {
            var pedidos = await ListarFinalizadosAsync(dataInicio, dataFim);

            var pontuais = pedidos.Count(p => p.Pontual);

            var comInicio = pedidos.Where(p => p.IniciadoEm.HasValue).ToList();
            int? tempoMedioDias = comInicio.Count == 0
                ? null
                : (int)Math.Round(comInicio.Average(p => (p.ConcluidoEm - p.IniciadoEm!.Value).TotalDays));

            return new ProducaoDto(
                dataInicio?.Date,
                dataFim?.Date,
                pedidos.Count,
                pedidos.Sum(p => p.Quantidade),
                pontuais,
                pedidos.Count - pontuais,
                tempoMedioDias,
                pedidos);
        }

        public async Task<PontualidadeDto> PontualidadeAsync(DateTime? dataInicio, DateTime? dataFim)
        {
            var pedidos = await ListarFinalizadosAsync(dataInicio, dataFim);
            var pontuais = pedidos.Count(p => p.Pontual);

            return new PontualidadeDto(pontuais, pedidos.Count - pontuais);
        }

        // ---------- helpers ----------

        // Pedidos na última etapa (maior Ordem), filtrados pela data em que chegaram nela
        private async Task<List<PedidoProduzidoDto>> ListarFinalizadosAsync(DateTime? dataInicio, DateTime? dataFim)
        {
            if (dataInicio.HasValue && dataFim.HasValue && dataFim.Value.Date < dataInicio.Value.Date)
                throw new RegraDeNegocioException("dataFim não pode ser anterior a dataInicio");

            var etapas = (await _etapaRepository.ListarTodosAsync()).OrderBy(e => e.Ordem).ToList();
            if (etapas.Count == 0)
                return new List<PedidoProduzidoDto>();

            var etapaInicial = etapas.First();
            var etapaFinal = etapas.Last();

            // 3 consultas no total, independente da quantidade de pedidos
            var pedidos = await _pedidoRepository.ListarTodosAsync();
            var eventosFinal = await _eventoRepository.ListarPorEtapaAsync(etapaFinal.Chave);
            var eventosInicio = await _eventoRepository.ListarPorEtapaAsync(etapaInicial.Chave);

            // Conclusão = evento mais recente na etapa final; início = mais antigo na etapa inicial
            var conclusaoPorPedido = eventosFinal
                .GroupBy(e => e.PedidoId)
                .ToDictionary(g => g.Key, g => g.Max(e => e.DataHora));

            var inicioPorPedido = eventosInicio
                .GroupBy(e => e.PedidoId)
                .ToDictionary(g => g.Key, g => g.Min(e => e.DataHora));

            return pedidos
                .Where(p => p.Coluna == etapaFinal.Chave && conclusaoPorPedido.ContainsKey(p.Id))
                .Select(p =>
                {
                    var concluidoEm = conclusaoPorPedido[p.Id];

                    return new PedidoProduzidoDto(
                        p.Id,
                        p.Cliente?.Nome ?? $"Cliente #{p.ClienteId}",
                        p.Produto?.Nome ?? $"Produto #{p.ProdutoId}",
                        p.Quantidade,
                        p.Prazo,
                        concluidoEm,
                        inicioPorPedido.TryGetValue(p.Id, out var inicio) ? inicio : null,
                        // Entregue no próprio dia do prazo ainda conta como pontual
                        concluidoEm.Date <= p.Prazo.Date);
                })
                // Compara só a data, então o último dia do período entra inteiro
                .Where(p => !dataInicio.HasValue || p.ConcluidoEm.Date >= dataInicio.Value.Date)
                .Where(p => !dataFim.HasValue || p.ConcluidoEm.Date <= dataFim.Value.Date)
                .OrderByDescending(p => p.ConcluidoEm)
                .ToList();
        }
    }
}
