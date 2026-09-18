using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Sincro.Domain.Entities;
using Sincro.Domain.Interfaces;
using Sincro.Application.Services;
using Sincro.Infrastructure.Data;

namespace Sincro.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PedidosController : ControllerBase
    {
        private readonly IPedidoRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEtapaRepository _etapaRepository;
        private readonly EtapaService _etapaService;
        private readonly ApplicationDbContext _context;


        public PedidosController(
            IPedidoRepository repository,
            IUnitOfWork unitOfWork,
            IEtapaRepository etapaRepository,
            EtapaService etapaService,
            ApplicationDbContext context)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _etapaRepository = etapaRepository;
            _etapaService = etapaService;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<Pedido>>> ListarTodos()
        {
            var pedidos = await _repository.ListarTodosAsync();

            return Ok(pedidos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<List<Pedido>>> ObterPorId(int id)
        {
            var pedido = await _repository.ObterPorIdAsync(id);

            if (pedido == null)
            {
                return NotFound(new { mensagem = "Pedido não encontrado." });
            }

            return Ok(pedido);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador,Gestor")]
        public async Task<ActionResult<Pedido>> Criar(Pedido pedido)
        {
            await _repository.AdicionarAsync(pedido);
            await _unitOfWork.SalvarAsync();

            return CreatedAtAction(
                nameof(ObterPorId),
                new { id = pedido.Id },
                pedido
            );
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador,Gestor")]
        public async Task<ActionResult> Atualizar(int id, Pedido pedido)
        {
            if (id != pedido.Id)
            {
                return BadRequest(new { mensagem = "O ID da URL não corresponde ao ID do pedido" });
            }

            var pedidoExistente = await _repository.ObterPorIdAsync(id);

            if (pedidoExistente == null)
            {
                return NotFound(new { mensagem = "Pedido não encontrado." });
            }

            pedidoExistente.ClienteId = pedido.ClienteId;
            pedidoExistente.ProdutoId = pedido.ProdutoId;
            pedidoExistente.ResponsavelId = pedido.ResponsavelId;
            pedidoExistente.Quantidade = pedido.Quantidade;
            pedidoExistente.Prazo = pedido.Prazo;
            pedidoExistente.Urgente = pedido.Urgente;
            pedidoExistente.Coluna = pedido.Coluna;

            _repository.Atualizar(pedidoExistente);
            await _unitOfWork.SalvarAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult> Remover(int id)
        {
            var pedido = await _repository.ObterPorIdAsync(id);


            if (pedido == null)
            {
                return NotFound(new { mensagem = "Pedido não encontrado." });
            }

            _repository.Remover(pedido);
            await _unitOfWork.SalvarAsync();

            return NoContent();
        }

        [HttpPut("{id}/etapa")]
        [Authorize(Roles = "Administrador,Gestor,Vendedor")]
        public async Task<ActionResult> AlterarEtapa(int id, [FromBody] string novaEtapa)
        {
            var pedido = await _repository.ObterPorIdAsync(id);

            if (pedido == null)
            {
                return NotFound(new { mensagem = "Pedido não encontrado." });
            }

            var etapas = await _etapaRepository.ListarTodosAsync();

            var resultado = _etapaService.ValidarTransicao(
                etapas,
                pedido.Coluna,
                novaEtapa
            );

            if (!resultado.Permitido)
            {
                return BadRequest(new { mensagem = resultado.Motivo });
            }

            var etapaDestino = etapas.FirstOrDefault(e => e.Chave == novaEtapa);

            if (etapaDestino == null)
            {
                return BadRequest(new { mensagem = "Etapa de destino não encontrada." });
            }

            var etapaAnterior = pedido.Coluna;
            pedido.Coluna = novaEtapa;
            _repository.Atualizar(pedido);

            var evento = new EventoPedido
            {
                PedidoId = pedido.Id,
                Etapa = novaEtapa,
                DataHora = DateTime.Now,
                Observacao = $"Pedido movido de {etapaAnterior} para {etapaDestino.Nome}.",
                RegistradoPorId = User.FindFirst("sub")?.Value
            };

            await _context.EventoPedidos.AddAsync(evento);
            await _unitOfWork.SalvarAsync();

            return NoContent();
        }
    }
}