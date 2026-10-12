using Microsoft.EntityFrameworkCore;
using Sincro.Application.DTOs;
using Sincro.Application.Exceptions;
using Sincro.Application.Interfaces;
using Sincro.Domain.Entities;
using Sincro.Domain.Interfaces;

namespace Sincro.Application.Services
{
    public class ProdutoService : IProdutoService
    {
        private readonly IProdutoRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public ProdutoService(IProdutoRepository repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ListaProdutosDto> ListarAsync(int pagina, int tamanho)
        {
            pagina = Math.Max(pagina, 1);
            tamanho = Math.Clamp(tamanho, 1, 100);

            var produtos = await _repository.ListarTodosAsync();
            var itens = produtos
                .OrderBy(p => p.Nome)
                .Skip((pagina - 1) * tamanho)
                .Take(tamanho)
                .Select(ParaDto)
                .ToList();

            return new ListaProdutosDto(produtos.Count, pagina, tamanho, itens);
        }

        public async Task<ProdutoDto> ObterAsync(int id)
            => ParaDto(await ObterOuFalharAsync(id));

        public async Task<ProdutoDto> CriarAsync(CriarProdutoDto dto)
        {
            var existentes = await _repository.ListarTodosAsync();

            if (existentes.Any(p => p.Nome.Equals(dto.Nome.Trim(), StringComparison.OrdinalIgnoreCase)))
                throw new RegraDeNegocioException("Já existe um produto com este nome");

            var produto = new Produto
            {
                Codigo = CodigoSequencial.Proximo("PRD", existentes.Select(p => p.Codigo)),
                Nome = dto.Nome.Trim(),
                Descricao = string.IsNullOrWhiteSpace(dto.Descricao) ? null : dto.Descricao.Trim(),
                PrecoBase = dto.Preco,
                Estoque = dto.Estoque
            };

            await _repository.AdicionarAsync(produto);
            await _unitOfWork.SalvarAsync();

            return ParaDto(produto);
        }

        public async Task<ProdutoDto> AtualizarAsync(int id, AtualizarProdutoDto dto)
        {
            var produto = await ObterOuFalharAsync(id);

            var existentes = await _repository.ListarTodosAsync();
            if (existentes.Any(p => p.Id != id && p.Nome.Equals(dto.Nome.Trim(), StringComparison.OrdinalIgnoreCase)))
                throw new RegraDeNegocioException("Já existe outro produto com este nome");

            produto.Nome = dto.Nome.Trim();
            produto.Descricao = string.IsNullOrWhiteSpace(dto.Descricao) ? null : dto.Descricao.Trim();
            produto.PrecoBase = dto.Preco;
            produto.Estoque = dto.Estoque;

            await _unitOfWork.SalvarAsync();

            return ParaDto(produto);
        }

        public async Task DeletarAsync(int id)
        {
            var produto = await ObterOuFalharAsync(id);

            try
            {
                _repository.Remover(produto);
                await _unitOfWork.SalvarAsync();
            }
            catch (DbUpdateException)
            {
                throw new RegraDeNegocioException("Produto possui pedidos e não pode ser deletado");
            }
        }

        // ---------- helpers ----------

        private async Task<Produto> ObterOuFalharAsync(int id)
            => await _repository.ObterPorIdAsync(id)
               ?? throw new NaoEncontradoException("Produto não encontrado");

        private static ProdutoDto ParaDto(Produto p)
            => new(p.Id, p.Codigo, p.Nome, p.Descricao, p.PrecoBase, p.Estoque);
    }
}
