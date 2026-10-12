using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sincro.Application.DTOs;
using Sincro.Application.Interfaces;
using Sincro.Domain;

namespace Sincro.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EtapasController : ControllerBase
    {
        private readonly IEtapaService _etapaService;

        public EtapasController(IEtapaService etapaService)
        {
            _etapaService = etapaService;
        }

        // Ler as colunas: qualquer usuário logado (o Kanban precisa delas)
        [HttpGet]
        public async Task<ActionResult<List<EtapaDto>>> ListarTodos()
            => Ok(await _etapaService.ListarAsync());

        [HttpGet("{id:int}")]
        public async Task<ActionResult<EtapaDto>> ObterPorId(int id)
            => Ok(await _etapaService.ObterAsync(id));

        // Configurar as colunas: só Administrador
        [HttpPost]
        [Authorize(Roles = Roles.Administrador)]
        public async Task<ActionResult<EtapaDto>> Criar(CriarEtapaDto dto)
        {
            var criada = await _etapaService.CriarAsync(dto);
            return CreatedAtAction(nameof(ObterPorId), new { id = criada.Id }, criada);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = Roles.Administrador)]
        public async Task<ActionResult> Atualizar(int id, AtualizarEtapaDto dto)
        {
            await _etapaService.AtualizarAsync(id, dto);
            return NoContent();
        }

        // Nova ordem de todas as colunas de uma vez
        [HttpPut("reordenar")]
        [Authorize(Roles = Roles.Administrador)]
        public async Task<ActionResult> Reordenar(ReordenarEtapasDto dto)
        {
            await _etapaService.ReordenarAsync(dto);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = Roles.Administrador)]
        public async Task<ActionResult> Remover(int id)
        {
            await _etapaService.RemoverAsync(id);
            return NoContent();
        }
    }
}
