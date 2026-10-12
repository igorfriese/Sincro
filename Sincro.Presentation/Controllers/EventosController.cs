using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sincro.Application.DTOs;
using Sincro.Application.Interfaces;
using Sincro.Presentation.Extensions;

namespace Sincro.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EventosController : ControllerBase
    {
        private readonly IEventoService _eventoService;

        public EventosController(IEventoService eventoService)
        {
            _eventoService = eventoService;
        }

        [HttpGet("pedido/{pedidoId:int}")]
        public async Task<ActionResult<List<EventoDto>>> ListarPorPedido(int pedidoId)
            => Ok(await _eventoService.ListarPorPedidoAsync(pedidoId, User.ParaUsuarioAtual()));

        [HttpGet("{id:int}")]
        public async Task<ActionResult<EventoDto>> ObterPorId(int id)
            => Ok(await _eventoService.ObterAsync(id, User.ParaUsuarioAtual()));

        // Anotação na timeline (mudança de etapa é pelo PUT /api/pedidos/{id}/etapa)
        [HttpPost]
        public async Task<ActionResult<EventoDto>> Criar(CriarAnotacaoDto dto)
        {
            var criado = await _eventoService.CriarAnotacaoAsync(dto, User.ParaUsuarioAtual());
            return CreatedAtAction(nameof(ObterPorId), new { id = criado.Id }, criado);
        }

        // Portal do cliente: rota pública, a segurança é a posse do token
        [HttpGet("publico/cliente/{token}/pedido/{pedidoId:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<List<EventoPublicoDto>>> ListarEventosPublicos(string token, int pedidoId)
            => Ok(await _eventoService.ListarPublicoAsync(token, pedidoId));
    }
}
