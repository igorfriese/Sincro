using Microsoft.AspNetCore.Mvc;
using Sincro.Domain.Interfaces;
using Sincro.Presentation.Dtos;

namespace Sincro.Presentation.Controllers
{
    [ApiController]
    [Route("api/acompanhar")]
    public class AcompanharController : ControllerBase
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IPedidoRepository _pedidoRepository;

        public AcompanharController(IClienteRepository clienteRepository, IPedidoRepository pedidoRepository)
        {
            _clienteRepository = clienteRepository;
            _pedidoRepository = pedidoRepository;
        }

        [HttpGet("{token}")]
        public async Task<IActionResult> ObterPorToken(string token)
        {
            var clientes = await _clienteRepository.ListarTodosAsync();
            var cliente = clientes.FirstOrDefault(c => c.TokenAcompanhamento == token);

            if (cliente is null)
            {
                return NotFound(new { erro = "Link inválido ou cliente não encontrado" });
            }

            var pedidos = await _pedidoRepository.ListarPorClienteIdAsync(cliente.Id);

            var pedidosdto = pedidos.Select(p => new PedidoPublicoDto(
                p.Id, p.Produto!.Nome, p.Quantidade, p.Prazo, p.Coluna, p.Urgente    
                )).ToList();

            return Ok(new AcompanharResponseDto(new ClientePublicoDto(cliente.Nome), pedidosdto));
        }
    }
}