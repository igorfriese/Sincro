using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sincro.Application.DTOs;
using Sincro.Application.Exceptions;
using Sincro.Application.Interfaces;
using Sincro.Domain;
using System.Security.Claims;

namespace Sincro.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PedidosController : ControllerBase
    {
        private readonly IPedidoService _pedidoService;

        public PedidosController(IPedidoService pedidoService)
        {
            _pedidoService = pedidoService;
        }

        // O .NET troca o claim "sub" do JWT por ClaimTypes.NameIdentifier ao ler o token
        private UsuarioAtual UsuarioAtual
        {
            get
            {
                var id = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? throw new NaoAutorizadoException();

                var somenteProprios = User.IsInRole(Roles.Vendedor)
                    && !User.IsInRole(Roles.Administrador)
                    && !User.IsInRole(Roles.Gestor);

                return new UsuarioAtual(id, somenteProprios);
            }
        }

        [HttpGet]
        public async Task<ActionResult<List<PedidoDto>>> ListarTodos()
            => Ok(await _pedidoService.ListarAsync(UsuarioAtual));

        [HttpGet("{id:int}")]
        public async Task<ActionResult<PedidoDto>> ObterPorId(int id)
            => Ok(await _pedidoService.ObterAsync(id, UsuarioAtual));

        [HttpPost]
        public async Task<ActionResult<PedidoDto>> Criar(CriarPedidoDto dto)
        {
            var criado = await _pedidoService.CriarAsync(dto, UsuarioAtual);
            return CreatedAtAction(nameof(ObterPorId), new { id = criado.Id }, criado);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Atualizar(int id, AtualizarPedidoDto dto)
        {
            await _pedidoService.AtualizarAsync(id, dto, UsuarioAtual);
            return NoContent();
        }

        // Única forma de mudar a coluna: valida a regra do Kanban e registra o evento
        [HttpPut("{id:int}/etapa")]
        public async Task<ActionResult> AlterarEtapa(int id, AlterarEtapaDto dto)
        {
            await _pedidoService.AlterarEtapaAsync(id, dto, UsuarioAtual);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = Roles.Administrador)]
        public async Task<ActionResult> Remover(int id)
        {
            await _pedidoService.RemoverAsync(id);
            return NoContent();
        }

        // Portal do cliente: rota pública, a segurança é a posse do token
        [HttpGet("publico/cliente/{token}")]
        [AllowAnonymous]
        public async Task<ActionResult<PortalClienteDto>> AcompanharClientePublico(string token)
            => Ok(await _pedidoService.AcompanharPorTokenAsync(token));
    }
}
