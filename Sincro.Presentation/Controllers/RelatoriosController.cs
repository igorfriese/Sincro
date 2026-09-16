using Microsoft.AspNetCore.Mvc;
using Sincro.Domain.Entities;
using Sincro.Domain.Interfaces;
using Sincro.Presentation.Dtos;

namespace Sincro.Presentation.Controllers
{
    [ApiController]
    [Route("api/relatorios")]
    public class RelatoriosController : ControllerBase
    {
        private readonly IPedidoRepository _pedidoRepository;
        private readonly IEventoPedidoRepository _eventoRepository;
        private readonly IEtapaRepository _etapaRepository;

        public RelatoriosController(IPedidoRepository pedidoRepository, IEventoPedidoRepository eventoRepository, IEtapaRepository etapaRepository)
        {
            _pedidoRepository = pedidoRepository;
            _eventoRepository = eventoRepository;
            _etapaRepository = etapaRepository;
        }

        [HttpGet("producao")]
        public async Task<IActionResult> Producao(DateTime dataInicio, DateTime dataFim)
        {
            var etapas = await _etapaRepository.ListarTodosAsync();
            var etapaFinal = etapas.OrderByDescending(e => e.Ordem).First();

            var pedidos = await _pedidoRepository.ListarTodosAsync();
            var pedidosFinalizados = pedidos.Where(p => p.Coluna == etapaFinal.Chave).ToList();

            var pedidosNoPeriodo = new List<Pedido>();

            foreach (var pedido in pedidosFinalizados)
            {
                var eventos = await _eventoRepository.ListarPorPedidoIdAsync(pedido.Id);
                var eventoConclusao = eventos.FirstOrDefault(e => e.Etapa == etapaFinal.Chave);

                if (eventoConclusao is null) continue;

                if (eventoConclusao.DataHora >= dataInicio && eventoConclusao.DataHora <= dataFim)
                {
                    pedidosNoPeriodo.Add(pedido);
                }
            }
            return Ok(pedidosNoPeriodo);
        }
    }
}