using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sincro.Domain.Entities;
using Sincro.Domain.Interfaces;
using Sincro.Presentation.DTOs;
using Sincro.Presentation.Dtos;

namespace Sincro.Presentation.Controllers
{
    [ApiController]
    [Route("api/clientes")]
    [Authorize]
    public class ClientesController : ControllerBase
    {
        private readonly IClienteRepository _repository;

        public ClientesController(IClienteRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        [Authorize(Roles = "Administrador,Gestor")]
        public async Task<IActionResult> ListarClientes([FromQuery] int pagina = 1, [FromQuery] int tamanho = 10)
        {
            try
            {
                var clientes = await _repository.ListarTodosAsync();
                var clientesPaginados = clientes
                    .OrderByDescending(c => c.Id)
                    .Skip((pagina - 1) * tamanho)
                    .Take(tamanho)
                    .Select(c => new ClienteListaDto(c.Id, c.Nome, c.Email ?? "", c.Telefone ?? "", DateTime.UtcNow))
                    .ToList();

                return Ok(new { total = clientes.Count, pagina, tamanho, clientes = clientesPaginados });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErroDto($"Erro ao listar clientes: {ex.Message}"));
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Administrador,Gestor")]
        public async Task<IActionResult> ObterCliente(int id)
        {
            try
            {
                var cliente = await _repository.ObterPorIdAsync(id);
                if (cliente is null)
                    return NotFound(new ErroDto("Cliente não encontrado"));

                return Ok(new ClienteDetailDto(cliente.Id, cliente.Nome, cliente.Email ?? "", cliente.Telefone ?? "", "", cliente.Documento ?? "", true, DateTime.UtcNow));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErroDto($"Erro ao obter cliente: {ex.Message}"));
            }
        }

        [HttpPost]
        [Authorize(Roles = "Administrador,Gestor")]
        public async Task<IActionResult> CriarCliente([FromBody] CriarClienteDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var novoCliente = new Cliente
                {
                    Codigo = Guid.NewGuid().ToString()[..8],
                    Nome = dto.Nome,
                    Email = dto.Email,
                    Telefone = dto.Telefone,
                    Documento = dto.CNPJ,
                    TokenAcompanhamento = Guid.NewGuid().ToString()
                };

                await _repository.AdicionarAsync(novoCliente);
                await _repository.SalvarAsync(); // ← IMPORTANTE

                return CreatedAtAction(nameof(ObterCliente), new { id = novoCliente.Id },
                    new ClienteListaDto(novoCliente.Id, novoCliente.Nome, novoCliente.Email ?? "", novoCliente.Telefone ?? "", DateTime.UtcNow));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErroDto($"Erro ao criar cliente: {ex.Message}"));
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador,Gestor")]
        public async Task<IActionResult> AtualizarCliente(int id, [FromBody] AtualizarClienteDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var cliente = await _repository.ObterPorIdAsync(id);
                if (cliente is null)
                    return NotFound(new ErroDto("Cliente não encontrado"));

                cliente.Nome = dto.Nome;
                cliente.Email = dto.Email;
                cliente.Telefone = dto.Telefone;

                _repository.Atualizar(cliente);
                await _repository.SalvarAsync(); // ← IMPORTANTE

                return Ok(new { mensagem = "Cliente atualizado com sucesso" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErroDto($"Erro ao atualizar cliente: {ex.Message}"));
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeletarCliente(int id)
        {
            try
            {
                var cliente = await _repository.ObterPorIdAsync(id);
                if (cliente is null)
                    return NotFound(new ErroDto("Cliente não encontrado"));

                _repository.Remover(cliente);
                await _repository.SalvarAsync(); // ← IMPORTANTE

                return Ok(new { mensagem = "Cliente deletado com sucesso" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErroDto($"Erro ao deletar cliente: {ex.Message}"));
            }
        }
    }
}