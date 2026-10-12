using Sincro.Application.DTOs;
using Sincro.Application.Exceptions;
using Sincro.Application.Interfaces;
using Sincro.Domain.Entities;
using Sincro.Domain.Interfaces;

namespace Sincro.Application.Services
{
    // Colunas do Kanban. A última (maior Ordem) é a etapa final: pedido nela é terminal.
    public class EtapaService : IEtapaService
    {
        private readonly IEtapaRepository _etapaRepository;
        private readonly IPedidoRepository _pedidoRepository;
        private readonly IUnitOfWork _unitOfWork;

        public EtapaService(
            IEtapaRepository etapaRepository,
            IPedidoRepository pedidoRepository,
            IUnitOfWork unitOfWork)
        {
            _etapaRepository = etapaRepository;
            _pedidoRepository = pedidoRepository;
            _unitOfWork = unitOfWork;
        }

        public (bool Permitido, string? Motivo) ValidarTransicao(
            List<Etapa> etapas,
            string chaveAtual,
            string chaveDestino)
        {
            if (chaveAtual == chaveDestino)
            {
                return (true, null);
            }

            var etapasOrdenadas = Ordenar(etapas);

            var indiceAtual = etapasOrdenadas
                .FindIndex(e => e.Chave == chaveAtual);

            var indiceDestino = etapasOrdenadas
                .FindIndex(e => e.Chave == chaveDestino);

            var indiceUltima = etapasOrdenadas.Count - 1;

            if (indiceAtual == -1 || indiceDestino == -1)
            {
                return (false, "Etapa inválida.");
            }

            if (indiceAtual == indiceUltima)
            {
                return (false, $"Este pedido já está \"{etapasOrdenadas[indiceUltima].Nome}\" e não pode ser mais movimentado.");
            }

            return (true, null);
        }

        public async Task<List<EtapaDto>> ListarAsync()
            => Ordenar(await _etapaRepository.ListarTodosAsync()).Select(ParaDto).ToList();

        public async Task<EtapaDto> ObterAsync(int id)
            => ParaDto(await ObterOuFalharAsync(id));

        public async Task<EtapaDto> CriarAsync(CriarEtapaDto dto)
        {
            var etapas = Ordenar(await _etapaRepository.ListarTodosAsync());

            if (etapas.Any(e => e.Chave == dto.Chave))
                throw new RegraDeNegocioException($"Já existe uma etapa com a chave \"{dto.Chave}\"");

            var nova = new Etapa
            {
                Chave = dto.Chave,
                Nome = dto.Nome.Trim(),
                Cor = dto.Cor,
            };

            if (etapas.Count == 0)
            {
                nova.Ordem = 1;
            }
            else
            {
                // Entra antes da última, para a etapa final continuar sendo a final
                var final = etapas[^1];
                nova.Ordem = final.Ordem;
                final.Ordem++;
            }

            await _etapaRepository.AdicionarAsync(nova);
            await _unitOfWork.SalvarAsync();

            return ParaDto(nova);
        }

        public async Task AtualizarAsync(int id, AtualizarEtapaDto dto)
        {
            var etapa = await ObterOuFalharAsync(id);

            etapa.Nome = dto.Nome.Trim();
            etapa.Cor = dto.Cor;

            await _unitOfWork.SalvarAsync();
        }

        public async Task ReordenarAsync(ReordenarEtapasDto dto)
        {
            var etapas = Ordenar(await _etapaRepository.ListarTodosAsync());

            var mesmasEtapas = dto.Ids.Count == etapas.Count
                && dto.Ids.Distinct().Count() == etapas.Count
                && etapas.All(e => dto.Ids.Contains(e.Id));

            if (!mesmasEtapas)
                throw new RegraDeNegocioException("Envie os ids de todas as etapas, cada um uma vez, na nova ordem");

            // Trocar a etapa final mudaria o que é "terminado": só com as duas vazias
            var finalAtual = etapas[^1];
            var novaFinal = etapas.First(e => e.Id == dto.Ids[^1]);

            if (novaFinal.Id != finalAtual.Id)
            {
                var pedidosNaFinalAtual = await _pedidoRepository.ContarPorColunaAsync(finalAtual.Chave);
                var pedidosNaNovaFinal = await _pedidoRepository.ContarPorColunaAsync(novaFinal.Chave);

                if (pedidosNaFinalAtual > 0 || pedidosNaNovaFinal > 0)
                    throw new RegraDeNegocioException(
                        $"\"{finalAtual.Nome}\" precisa continuar sendo a última etapa enquanto houver pedidos nela ou em \"{novaFinal.Nome}\"");
            }

            for (var i = 0; i < dto.Ids.Count; i++)
                etapas.First(e => e.Id == dto.Ids[i]).Ordem = i + 1;

            await _unitOfWork.SalvarAsync();
        }

        public async Task RemoverAsync(int id)
        {
            var etapa = await ObterOuFalharAsync(id);
            var etapas = Ordenar(await _etapaRepository.ListarTodosAsync());

            if (etapas[^1].Id == etapa.Id)
                throw new RegraDeNegocioException($"\"{etapa.Nome}\" é a etapa final e não pode ser excluída");

            if (etapas.Count <= 2)
                throw new RegraDeNegocioException("O Kanban precisa de pelo menos 2 etapas");

            var qtdPedidos = await _pedidoRepository.ContarPorColunaAsync(etapa.Chave);
            if (qtdPedidos > 0)
                throw new RegraDeNegocioException(
                    $"Não é possível excluir \"{etapa.Nome}\": existem {qtdPedidos} pedido(s) nesta etapa. Mova-os antes de excluir.");

            _etapaRepository.Remover(etapa);
            await _unitOfWork.SalvarAsync();
        }

        // ---------- helpers ----------

        // Desempate pelo Id para a ordem ser sempre a mesma, mesmo com Ordem repetida
        private static List<Etapa> Ordenar(IEnumerable<Etapa> etapas)
            => etapas.OrderBy(e => e.Ordem).ThenBy(e => e.Id).ToList();

        private async Task<Etapa> ObterOuFalharAsync(int id)
            => await _etapaRepository.ObterPorIdAsync(id)
               ?? throw new NaoEncontradoException("Etapa não encontrada");

        private static EtapaDto ParaDto(Etapa e) => new(e.Id, e.Chave, e.Nome, e.Cor, e.Ordem);
    }
}
