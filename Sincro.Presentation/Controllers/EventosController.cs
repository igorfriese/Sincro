using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sincro.Domain.Entities;
using Sincro.Infrastructure.Data;

namespace Sincro.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EventosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EventosController (ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("pedido/{pedidoId}")]
        public async Task<ActionResult<List<EventoPedido>>> ListarPorPedido(int pedidoId)
        {
            var pedidoExiste = await _context.Pedidos
                .AnyAsync(p => p.Id == pedidoId);

            if (!pedidoExiste)
            {
                return NotFound(new { mensagem = "Pedido não encontrado." });
            }

            var eventos = await _context.EventoPedidos
                .Where(e => e.PedidoId == pedidoId)
                .OrderBy(e => e.DataHora)
                .ToListAsync();

            return Ok(eventos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EventoPedido>> ObterPorId(int id)
        {
            var evento = await _context.EventoPedidos
                .FirstOrDefaultAsync(e => e.Id == id);
            
            if (evento == null)
            {
                return NotFound(new { mensagem = "Evento não encontrado." });
            }

            return Ok(evento);
        }

        // =========================================================
        // 1. EVENTOS PÚBLICOS DO CLIENTE
        // =========================================================

        [HttpGet("publico/cliente/{token}/pedido/{pedidoId}")]
        [AllowAnonymous]
        public async Task<ActionResult> ListarEventosPublicos(string token, int pedidoId)
        {
            var cliente = await _context.Clientes
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.TokenAcompanhamento == token);
        
            if (cliente == null)
            {
                return NotFound(new { mensagem = "Link inválido ou cliente não encontrado." });
            }

            var pedidoPertenceAoCliente = await _context.Pedidos
                .AsNoTracking()
                .AnyAsync(p => p.Id == pedidoId && p.ClienteId == cliente.Id);

            if (!pedidoPertenceAoCliente)
            {
                return NotFound(new { mensagem = "Pedido não encontrado para este cliente." });
            }

            var eventos = await _context.EventoPedidos
                .AsNoTracking()
                .Where(e => e.PedidoId == pedidoId)
                .OrderBy(e => e.DataHora)
                .Select(e => new
                {
                    id = e.Id,
                    etapa = e.Etapa,
                    dataHota = e.DataHora,
                    observacao = e.Observacao,
                })
                .ToListAsync();

            return Ok(eventos);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador,Gestor,Vendedor")]
        public async Task<ActionResult<EventoPedido>> Criar(EventoPedido evento)
        {
            var pedidoExite = await _context.Pedidos
                .AnyAsync(p => p.Id == evento.PedidoId);

            if (!pedidoExite)
            {
                return NotFound(new { mensagem = "Pedido não encontrado." });
            }

            evento.Id = 0;
            evento.DataHora = evento.DataHora == default
                ? DateTime.Now : evento.DataHora;

            evento.RegistradoPorId = User.FindFirst("sub")?.Value;

            await _context.EventoPedidos.AddAsync(evento);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(ObterPorId),
                new { id = evento.Id },
                evento);
        }
    }
}
