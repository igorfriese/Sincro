using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sincro.Application.DTOs;
using Sincro.Application.Exceptions;
using Sincro.Application.Interfaces;
using Sincro.Domain.Entities;
using Sincro.Domain.Interfaces;

namespace Sincro.Application.Services
{
    public class EventoService : IEventoService
    {
        private readonly IEventoPedidoRepository _eventoRepository;
        private readonly IPedidoRepository _pedidoRepository;
        private readonly IClienteRepository _clienteRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public EventoService(
            IEventoPedidoRepository eventoRepository,
            IPedidoRepository pedidoRepository,
            IClienteRepository clienteRepository,
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager)
        {
            _eventoRepository = eventoRepository;
            _pedidoRepository = pedidoRepository;
            _clienteRepository = clienteRepository;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<List<EventoDto>> ListarPorPedidoAsync(int pedidoId, UsuarioAtual usuario)
        {
            await ObterPedidoComPermissaoAsync(pedidoId, usuario);

            var eventos = await _eventoRepository.ListarPorPedidoIdAsync(pedidoId);
            var nomes = await NomesDosUsuariosAsync(eventos.Select(e => e.RegistradoPorId));

            return eventos.Select(e => ParaDto(e, nomes)).ToList();
        }

        public async Task<EventoDto> ObterAsync(int id, UsuarioAtual usuario)
        {
            var evento = await _eventoRepository.ObterPorIdAsync(id)
                ?? throw new NaoEncontradoException("Evento não encontrado");

            await ObterPedidoComPermissaoAsync(evento.PedidoId, usuario);

            var nomes = await NomesDosUsuariosAsync(new[] { evento.RegistradoPorId });
            return ParaDto(evento, nomes);
        }

        public async Task<EventoDto> CriarAnotacaoAsync(CriarAnotacaoDto dto, UsuarioAtual usuario)
        {
            var pedido = await ObterPedidoComPermissaoAsync(dto.PedidoId, usuario);

            var observacao = dto.Observacao.Trim();
            if (observacao.Length == 0)
                throw new RegraDeNegocioException("Observação é obrigatória");

            var evento = new EventoPedido
            {
                PedidoId = pedido.Id,
                Etapa = pedido.Coluna, // anotação fica na etapa em que o pedido está
                DataHora = DateTime.Now,
                Observacao = observacao,
                RegistradoPorId = usuario.Id,
            };

            await _eventoRepository.AdicionarAsync(evento);
            await _unitOfWork.SalvarAsync();

            var nomes = await NomesDosUsuariosAsync(new[] { evento.RegistradoPorId });
            return ParaDto(evento, nomes);
        }

        public async Task<List<EventoPublicoDto>> ListarPublicoAsync(string token, int pedidoId)
        {
            var cliente = await _clienteRepository.ObterPorTokenAsync(token)
                ?? throw new NaoEncontradoException("Link inválido ou cliente não encontrado");

            var pedido = await _pedidoRepository.ObterPorIdAsync(pedidoId);
            if (pedido is null || pedido.ClienteId != cliente.Id)
                throw new NaoEncontradoException("Pedido não encontrado para este cliente");

            var eventos = await _eventoRepository.ListarPorPedidoIdAsync(pedidoId);

            return eventos
                .Select(e => new EventoPublicoDto(e.Id, e.Etapa, e.DataHora, e.Observacao))
                .ToList();
        }

        // ---------- helpers ----------

        // Mesma regra dos pedidos: Vendedor só mexe no que é dele
        private async Task<Pedido> ObterPedidoComPermissaoAsync(int pedidoId, UsuarioAtual usuario)
        {
            var pedido = await _pedidoRepository.ObterPorIdAsync(pedidoId)
                ?? throw new NaoEncontradoException("Pedido não encontrado");

            if (usuario.SomenteProprios && pedido.ResponsavelId != usuario.Id)
                throw new AcessoNegadoException("Você só pode acessar os pedidos sob sua responsabilidade");

            return pedido;
        }

        // Uma consulta só para os nomes de todos que registraram eventos
        private async Task<Dictionary<string, string>> NomesDosUsuariosAsync(IEnumerable<string?> ids)
        {
            var idsValidos = ids.Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList();
            if (idsValidos.Count == 0)
                return new Dictionary<string, string>();

            return await _userManager.Users
                .Where(u => idsValidos.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.Nome);
        }

        private static EventoDto ParaDto(EventoPedido e, Dictionary<string, string> nomes)
            => new(
                e.Id,
                e.PedidoId,
                e.Etapa,
                e.DataHora,
                e.Observacao,
                e.RegistradoPorId,
                e.RegistradoPorId is not null && nomes.TryGetValue(e.RegistradoPorId, out var nome) ? nome : null);
    }
}
