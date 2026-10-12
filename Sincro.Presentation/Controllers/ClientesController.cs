using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sincro.Application.DTOs;
using Sincro.Application.Interfaces;
using Sincro.Domain;
using Sincro.Presentation.Extensions;

namespace Sincro.Presentation.Controllers
{
    [ApiController]
    [Route("api/clientes")]
    [Authorize]
    public class ClientesController : ControllerBase
    {
        private readonly IClienteService _clienteService;

        public ClientesController(IClienteService clienteService)
        {
            _clienteService = clienteService;
        }

        // Leitura liberada para todos (o Vendedor também cria pedidos e precisa escolher o cliente),
        // mas o token do portal só vai para Administrador e Gestor
        private bool PodeVerToken => !User.ParaUsuarioAtual().SomenteProprios;

        [HttpGet]
        public async Task<ActionResult<ListaClientesDto>> ListarClientes([FromQuery] int pagina = 1, [FromQuery] int tamanho = 10)
            => Ok(await _clienteService.ListarAsync(pagina, tamanho, PodeVerToken));

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ClienteDto>> ObterCliente(int id)
            => Ok(await _clienteService.ObterAsync(id, PodeVerToken));

        [HttpPost]
        [Authorize(Roles = Roles.AdministradorOuGestor)]
        public async Task<ActionResult<ClienteDto>> CriarCliente([FromBody] CriarClienteDto dto)
        {
            var criado = await _clienteService.CriarAsync(dto);
            return CreatedAtAction(nameof(ObterCliente), new { id = criado.Id }, criado);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = Roles.AdministradorOuGestor)]
        public async Task<ActionResult<ClienteDto>> AtualizarCliente(int id, [FromBody] AtualizarClienteDto dto)
            => Ok(await _clienteService.AtualizarAsync(id, dto));

        [HttpDelete("{id:int}")]
        [Authorize(Roles = Roles.Administrador)]
        public async Task<IActionResult> DeletarCliente(int id)
        {
            await _clienteService.DeletarAsync(id);
            return Ok(new MensagemDto("Cliente deletado com sucesso"));
        }
    }
}
