using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sincro.Domain.Entities;
using Sincro.Domain.Interfaces;
using Sincro.Presentation.DTOs;
using Sincro.Presentation.Dtos;

namespace Sincro.Presentation.Controllers
{
    [ApiController]
    [Route("api/produtos")]
    [Authorize]
    public class ProdutosController : ControllerBase
    {
        private readonly IProdutoRepository _repository;

        public ProdutosController(IProdutoRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Listar todos os produtos (paginado) — Todos autenticados podem ver
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListarProdutos([FromQuery] int pagina = 1, [FromQuery] int tamanho = 10)
        {
            try
            {
                var produtos = await _repository.ListarTodosAsync();
                var produtosPaginados = produtos
                    .OrderByDescending(p => p.Id)
                    .Skip((pagina - 1) * tamanho)
                    .Take(tamanho)
                    .Select(p => new ProdutoListaDto(
                        p.Id,
                        p.Nome,
                        p.PrecoBase,
                        p.Estoque,
                        true // Ativo
                    ))
                    .ToList();

                return Ok(new
                {
                    total = produtos.Count,
                    pagina = pagina,
                    tamanho = tamanho,
                    produtos = produtosPaginados
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErroDto($"Erro ao listar produtos: {ex.Message}"));
            }
        }

        /// <summary>
        /// Obter detalhes de um produto específico — Todos autenticados podem ver
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> ObterProduto(int id)
        {
            try
            {
                var produto = await _repository.ObterPorIdAsync(id);
                if (produto is null)
                    return NotFound(new ErroDto("Produto não encontrado"));

                return Ok(new ProdutoDetailDto(
                    produto.Id,
                    produto.Nome,
                    produto.Descricao ?? "",
                    produto.PrecoBase,
                    produto.Estoque,
                    true, // Ativo
                    DateTime.UtcNow
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErroDto($"Erro ao obter produto: {ex.Message}"));
            }
        }

        /// <summary>
        /// Criar novo produto (Admin ou Gestor)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Administrador,Gestor")]
        public async Task<IActionResult> CriarProduto([FromBody] CriarProdutoDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var novoProduto = new Produto
                {
                    Codigo = Guid.NewGuid().ToString()[..8],
                    Nome = dto.Nome,
                    Descricao = dto.Descricao,
                    PrecoBase = dto.Preco,
                    Estoque = dto.Estoque
                };

                await _repository.AdicionarAsync(novoProduto);

                return CreatedAtAction(nameof(ObterProduto), new { id = novoProduto.Id },
                    new ProdutoListaDto(
                        novoProduto.Id,
                        novoProduto.Nome,
                        novoProduto.PrecoBase,
                        0,
                        true
                    ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErroDto($"Erro ao criar produto: {ex.Message}"));
            }
        }

        /// <summary>
        /// Atualizar dados de um produto (Admin ou Gestor)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador,Gestor")]
        public async Task<IActionResult> AtualizarProduto(int id, [FromBody] AtualizarProdutoDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var produto = await _repository.ObterPorIdAsync(id);
                if (produto is null)
                    return NotFound(new ErroDto("Produto não encontrado"));

                // Atualizar campos
                produto.Nome = dto.Nome;
                produto.Descricao = dto.Descricao;
                produto.PrecoBase = dto.Preco;
                produto.Estoque = dto.Estoque;

                // Descrição e Estoque — atualizar se adicionados à entidade

                _repository.Atualizar(produto);

                return Ok(new { mensagem = "Produto atualizado com sucesso" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErroDto($"Erro ao atualizar produto: {ex.Message}"));
            }
        }

        /// <summary>
        /// Deletar um produto (apenas Admin)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeletarProduto(int id)
        {
            try
            {
                var produto = await _repository.ObterPorIdAsync(id);
                if (produto is null)
                    return NotFound(new ErroDto("Produto não encontrado"));

                _repository.Remover(produto);

                return Ok(new { mensagem = "Produto deletado com sucesso" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErroDto($"Erro ao deletar produto: {ex.Message}"));
            }
        }
    }
}