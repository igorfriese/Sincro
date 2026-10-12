using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sincro.Application.DTOs;
using Sincro.Application.Interfaces;
using Sincro.Domain;

namespace Sincro.Presentation.Controllers
{
    [ApiController]
    [Route("api/relatorios")]
    [Authorize(Roles = Roles.AdministradorOuGestor)]
    public class RelatoriosController : ControllerBase
    {
        private readonly IRelatorioService _relatorioService;

        public RelatoriosController(IRelatorioService relatorioService)
        {
            _relatorioService = relatorioService;
        }

        // GET /api/relatorios/producao?dataInicio=2026-10-01&dataFim=2026-10-31 (datas opcionais)
        [HttpGet("producao")]
        public async Task<ActionResult<ProducaoDto>> Producao([FromQuery] DateTime? dataInicio, [FromQuery] DateTime? dataFim)
        {
            return Ok(await _relatorioService.ProducaoAsync(dataInicio, dataFim));
        }

        // GET /api/relatorios/pontualidade?dataInicio=2026-10-01&dataFim=2026-10-31
        [HttpGet("pontualidade")]
        public async Task<ActionResult<PontualidadeDto>> Pontualidade([FromQuery] DateTime? dataInicio, [FromQuery] DateTime? dataFim)
        {
            return Ok(await _relatorioService.PontualidadeAsync(dataInicio, dataFim));
        }
    }
}
