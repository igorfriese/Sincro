using Microsoft.AspNetCore.Identity;
using Sincro.Application.DTOs;
using Sincro.Application.Exceptions;
using Sincro.Application.Interfaces;
using Sincro.Domain.Entities;
using Sincro.Domain.Interfaces;

namespace Sincro.Application.Services
{
    public class PedidoService : IPedidoService
    {
        private readonly IPedidoRepository _pedidoRepository;
        private readonly IClienteRepository _clienteRepository;
        private readonly IProdutoRepository _produtoRepository;
        private readonly IEtapaRepository _etapaRepository;
        private readonly IEventoPedidoRepository _eventoRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public PedidoService(
            IPedidoRepository pedidoRepository,
            IClienteRepository clienteRepository,
            IProdutoRepository produtoRepository,
            IEtapaRepository etapaRepository,
            IEventoPedidoRepository eventoRepository,
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager)
        {
            _pedidoRepository = pedidoRepository;
            _clienteRepository = clienteRepository;
            _produtoRepository = produtoRepository;
            _etapaRepository = etapaRepository;
            _eventoRepository = eventoRepository;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<List<PedidoDto>> ListarAsync(UsuarioAtual usuario)
        {
            var pedidos = usuario.SomenteProprios
                ? await _pedidoRepository.ListarPorResponsavelAsync(usuario.Id)
                : await _pedidoRepository.ListarTodosAsync();

            return pedidos.OrderBy(p => p.Prazo).Select(ParaDto).ToList();
        }

        public async Task<PedidoDto> ObterAsync(int id, UsuarioAtual usuario)
        {
            var pedido = await ObterComPermissaoAsync(id, usuario);
            return ParaDto(pedido);
        }

        public async Task<PedidoDto> CriarAsync(CriarPedidoDto dto, UsuarioAtual usuario)
        {
            await ValidarClienteEProdutoAsync(dto.ClienteId, dto.ProdutoId);
            ValidarQuantidadeEPrazo(dto.Quantidade, dto.Prazo);

            if (dto.Prazo.Date < DateTime.Today)
                throw new RegraDeNegocioException("O prazo não pode ser anterior a hoje");

            // Vendedor sempre cria o pedido em nome próprio
            var responsavelId = usuario.SomenteProprios
                ? usuario.Id
                : await ValidarResponsavelAsync(dto.ResponsavelId);

            var etapas = await ListarEtapasOrdenadasAsync();
            var etapaInicial = string.IsNullOrWhiteSpace(dto.Coluna)
                ? etapas[0]
                : etapas.FirstOrDefault(e => e.Chave == dto.Coluna)
                  ?? throw new RegraDeNegocioException($"Etapa \"{dto.Coluna}\" não existe");

            if (etapaInicial == etapas[^1])
                throw new RegraDeNegocioException($"O pedido não pode ser criado já em \"{etapaInicial.Nome}\"");

            var pedido = new Pedido
            {
                ClienteId = dto.ClienteId,
                ProdutoId = dto.ProdutoId,
                ResponsavelId = responsavelId,
                Quantidade = dto.Quantidade,
                Prazo = dto.Prazo.Date,
                Urgente = dto.Urgente,
                Coluna = etapaInicial.Chave,
            };
            await _pedidoRepository.AdicionarAsync(pedido);

            // O evento de criação é do servidor: a timeline começa sempre registrada
            await _eventoRepository.AdicionarAsync(new EventoPedido
            {
                Pedido = pedido,
                Etapa = etapaInicial.Chave,
                DataHora = DateTime.Now,
                Observacao = "Ordem de produção criada.",
                RegistradoPorId = usuario.Id,
            });

            await _unitOfWork.SalvarAsync();

            // Recarrega com cliente e produto para devolver o mesmo formato da listagem
            return ParaDto((await _pedidoRepository.ObterPorIdAsync(pedido.Id))!);
        }

        public async Task AtualizarAsync(int id, AtualizarPedidoDto dto, UsuarioAtual usuario)
        {
            var pedido = await ObterComPermissaoAsync(id, usuario);

            await ValidarClienteEProdutoAsync(dto.ClienteId, dto.ProdutoId);
            ValidarQuantidadeEPrazo(dto.Quantidade, dto.Prazo);

            pedido.ClienteId = dto.ClienteId;
            pedido.ProdutoId = dto.ProdutoId;
            pedido.Quantidade = dto.Quantidade;
            pedido.Prazo = dto.Prazo.Date;
            pedido.Urgente = dto.Urgente;

            // Vendedor não troca o responsável (senão o pedido "sumiria" da lista dele)
            if (!usuario.SomenteProprios)
                pedido.ResponsavelId = await ValidarResponsavelAsync(dto.ResponsavelId);

            await _unitOfWork.SalvarAsync();
        }

        public async Task AlterarEtapaAsync(int id, AlterarEtapaDto dto, UsuarioAtual usuario)
        {
            var pedido = await ObterComPermissaoAsync(id, usuario);
            var etapas = await ListarEtapasOrdenadasAsync();

            var (permitido, motivo) = EtapaService.ValidarTransicao(etapas, pedido.Coluna, dto.Etapa);
            if (!permitido)
                throw new RegraDeNegocioException(motivo ?? "Movimento não permitido");

            if (pedido.Coluna == dto.Etapa)
                return; // já está nessa etapa: nada muda, nenhum evento

            var etapaAnterior = etapas.First(e => e.Chave == pedido.Coluna);
            var etapaNova = etapas.First(e => e.Chave == dto.Etapa);

            pedido.Coluna = etapaNova.Chave;

            await _eventoRepository.AdicionarAsync(new EventoPedido
            {
                PedidoId = pedido.Id,
                Etapa = etapaNova.Chave,
                DataHora = DateTime.Now,
                Observacao = string.IsNullOrWhiteSpace(dto.Observacao)
                    ? $"Pedido movido de \"{etapaAnterior.Nome}\" para \"{etapaNova.Nome}\"."
                    : dto.Observacao.Trim(),
                RegistradoPorId = usuario.Id,
            });

            await _unitOfWork.SalvarAsync();
        }

        public async Task RemoverAsync(int id)
        {
            var pedido = await _pedidoRepository.ObterPorIdAsync(id)
                ?? throw new NaoEncontradoException("Pedido não encontrado");

            _pedidoRepository.Remover(pedido); // eventos saem junto (cascade)
            await _unitOfWork.SalvarAsync();
        }

        public async Task<PortalClienteDto> AcompanharPorTokenAsync(string token)
        {
            var cliente = await _clienteRepository.ObterPorTokenAsync(token)
                ?? throw new NaoEncontradoException("Link inválido ou cliente não encontrado");

            var pedidos = await _pedidoRepository.ListarPorClienteIdAsync(cliente.Id);

            var itens = pedidos
                .OrderBy(p => p.Prazo)
                .Select(p => new PedidoPortalDto(
                    p.Id,
                    p.Produto?.Nome ?? $"Produto #{p.ProdutoId}",
                    p.Quantidade,
                    p.Prazo,
                    p.Urgente,
                    p.Coluna))
                .ToList();

            return new PortalClienteDto(new ClientePortalDto(cliente.Id, cliente.Nome), itens);
        }

        // ---------- helpers ----------

        private async Task<Pedido> ObterComPermissaoAsync(int id, UsuarioAtual usuario)
        {
            var pedido = await _pedidoRepository.ObterPorIdAsync(id)
                ?? throw new NaoEncontradoException("Pedido não encontrado");

            if (usuario.SomenteProprios && pedido.ResponsavelId != usuario.Id)
                throw new AcessoNegadoException("Você só pode acessar os pedidos sob sua responsabilidade");

            return pedido;
        }

        private async Task ValidarClienteEProdutoAsync(int clienteId, int produtoId)
        {
            if (await _clienteRepository.ObterPorIdAsync(clienteId) is null)
                throw new RegraDeNegocioException("Cliente não encontrado");

            if (await _produtoRepository.ObterPorIdAsync(produtoId) is null)
                throw new RegraDeNegocioException("Produto não encontrado");
        }

        private static void ValidarQuantidadeEPrazo(int quantidade, DateTime prazo)
        {
            if (quantidade <= 0)
                throw new RegraDeNegocioException("Quantidade deve ser maior que 0");

            if (prazo == default)
                throw new RegraDeNegocioException("Prazo é obrigatório");
        }

        private async Task<string?> ValidarResponsavelAsync(string? responsavelId)
        {
            if (string.IsNullOrWhiteSpace(responsavelId))
                return null;

            if (await _userManager.FindByIdAsync(responsavelId) is null)
                throw new RegraDeNegocioException("Responsável não encontrado");

            return responsavelId;
        }

        private async Task<List<Etapa>> ListarEtapasOrdenadasAsync()
        {
            var etapas = (await _etapaRepository.ListarTodosAsync()).OrderBy(e => e.Ordem).ToList();

            if (etapas.Count == 0)
                throw new RegraDeNegocioException("Nenhuma etapa cadastrada");

            return etapas;
        }

        private static PedidoDto ParaDto(Pedido p)
            => new(
                p.Id,
                p.ClienteId,
                p.Cliente is null ? null : new ClienteResumoDto(p.Cliente.Id, p.Cliente.Nome, p.Cliente.TokenAcompanhamento),
                p.ProdutoId,
                p.Produto is null ? null : new ProdutoResumoDto(p.Produto.Id, p.Produto.Nome),
                p.ResponsavelId,
                p.Quantidade,
                p.Prazo,
                p.Urgente,
                p.Coluna);
    }
}
