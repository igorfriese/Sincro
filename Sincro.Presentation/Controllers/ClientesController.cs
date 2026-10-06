using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sincro.Application.DTOs;
using Sincro.Application.Interfaces;
using Sincro.Domain;

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

        [HttpGet]
        [Authorize(Roles = Roles.AdministradorOuGestor)]
        public async Task<IActionResult> ListarClientes([FromQuery] int pagina = 1, [FromQuery] int tamanho = 10)
            => Ok(await _clienteService.ListarAsync(pagina, tamanho));

        [HttpGet("{id:int}")]
        [Authorize(Roles = Roles.AdministradorOuGestor)]
        public async Task<IActionResult> ObterCliente(int id)
            => Ok(await _clienteService.ObterAsync(id));

        [HttpPost]
        [Authorize(Roles = Roles.AdministradorOuGestor)]
        public async Task<IActionResult> CriarCliente([FromBody] CriarClienteDto dto)
        {
            var criado = await _clienteService.CriarAsync(dto);
            return CreatedAtAction(nameof(ObterCliente), new { id = criado.Id }, criado);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = Roles.AdministradorOuGestor)]
        public async Task<IActionResult> AtualizarCliente(int id, [FromBody] AtualizarClienteDto dto)
        {
            await _clienteService.AtualizarAsync(id, dto);
            return Ok(new MensagemDto("Cliente atualizado com sucesso"));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = Roles.Administrador)]
        public async Task<IActionResult> DeletarCliente(int id)
        {
            await _clienteService.DeletarAsync(id);
            return Ok(new MensagemDto("Cliente deletado com sucesso"));
        }
    }
}