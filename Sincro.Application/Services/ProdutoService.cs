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

        public ProdutoService(IProdutoRepository repository)
        {
            _repository = repository;
        }

        public async Task<ListaProdutosDto> ListarAsync(int pagina, int tamanho)
        {
            pagina = Math.Max(pagina, 1);
            tamanho = Math.Clamp(tamanho, 1, 100);

            var produtos = await _repository.ListarTodosAsync();
            var itens = produtos
                .OrderByDescending(p => p.Id)
                .Skip((pagina - 1) * tamanho)
                .Take(tamanho)
                .Select(ParaLista)
                .ToList();

            return new ListaProdutosDto(produtos.Count, pagina, tamanho, itens);
        }

        public async Task<ProdutoDetailDto> ObterAsync(int id)
        {
            var p = await ObterOuFalharAsync(id);

            return new ProdutoDetailDto(p.Id, p.Nome, p.Descricao ?? "", p.PrecoBase, p.Estoque, true, DateTime.UtcNow);
        }

        public async Task<ProdutoListaDto> CriarAsync(CriarProdutoDto dto)
        {
            var existentes = await _repository.ListarTodosAsync();

            var produto = new Produto
            {
                Codigo = CodigoSequencial.Proximo("PRD", existentes.Select(p => p.Codigo)),
                Nome = dto.Nome,
                Descricao = dto.Descricao,
                PrecoBase = dto.Preco,
                Estoque = dto.Estoque
            };

            await _repository.AdicionarAsync(produto);
            await _repository.SalvarAsync();

            return ParaLista(produto);
        }

        public async Task AtualizarAsync(int id, AtualizarProdutoDto dto)
        {
            var produto = await ObterOuFalharAsync(id);

            produto.Nome = dto.Nome;
            produto.Descricao = dto.Descricao;
            produto.PrecoBase = dto.Preco;
            produto.Estoque = dto.Estoque;

            _repository.Atualizar(produto);
            await _repository.SalvarAsync();
        }

        public async Task DeletarAsync(int id)
        {
            var produto = await ObterOuFalharAsync(id);

            try
            {
                _repository.Remover(produto);
                await _repository.SalvarAsync();
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

        private static ProdutoListaDto ParaLista(Produto p)
            => new(p.Id, p.Nome, p.PrecoBase, p.Estoque, true);
    }
}