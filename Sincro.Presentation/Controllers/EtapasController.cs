using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sincro.Domain.Entities;
using Sincro.Domain.Interfaces;

namespace Sincro.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EtapasController : ControllerBase
    {
        private readonly IEtapaRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public EtapasController(
            IEtapaRepository repository,
            IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<ActionResult<List<Etapa>>> ListarTodos()
        {
            var etapas = await _repository.ListarTodosAsync();

            return Ok(etapas);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Etapa>> ObterPorId(int id)
        {
            var etapa = await _repository.ObterPorIdAsync(id);

            if (etapa == null)
                return NotFound(new { mensagem = "Etapa não encontrada." });

            return Ok(etapa);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador,Gestor")]
        public async Task<ActionResult<Etapa>> Criar(Etapa etapa)
        {
            await _repository.AdicionarAsync(etapa);
            await _unitOfWork.SalvarAsync();

            return CreatedAtAction(
                nameof(ObterPorId),
                new { id = etapa.Id },
                etapa
            );
        }


        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador,Gestor")]
        public async Task<ActionResult> Atualizar(int id, Etapa etapa)
        {
            if (id != etapa.Id)
                return BadRequest(new { mensagem = "O ID da URL não corresponde ao ID da etapa." });

            var etapaExistente = await _repository.ObterPorIdAsync(id);

            if (etapaExistente == null)
                return NotFound(new { mensagem = "Etapa não encontrada." });

            etapaExistente.Chave = etapa.Chave;
            etapaExistente.Nome = etapa.Nome;
            etapaExistente.Cor = etapa.Cor;
            etapaExistente.Ordem = etapa.Ordem;

            _repository.Atualizar(etapaExistente);
            await _unitOfWork.SalvarAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult> Remover(int id)
        {
            var etapa = await _repository.ObterPorIdAsync(id);

            if (etapa == null)
            {
                return NotFound(new { mensagem = "Etapa não encontrada." });
            }

                _repository.Remover(etapa);
                await _unitOfWork.SalvarAsync();

                return NoContent();
        }
    }
}
