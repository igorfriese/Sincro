using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sincro.Application.DTOs;
using Sincro.Application.Interfaces;
using Sincro.Domain;

namespace Sincro.Presentation.Controllers
{
    [ApiController]
    [Route("api/produtos")]
    [Authorize]
    public class ProdutosController : ControllerBase
    {
        private readonly IProdutoService _produtoService;

        public ProdutosController(IProdutoService produtoService)
        {
            _produtoService = produtoService;
        }

        /// <summary>Listar produtos (todos autenticados)</summary>
        [HttpGet]
        public async Task<IActionResult> ListarProdutos([FromQuery] int pagina = 1, [FromQuery] int tamanho = 10)
            => Ok(await _produtoService.ListarAsync(pagina, tamanho));

        /// <summary>Obter produto (todos autenticados)</summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObterProduto(int id)
            => Ok(await _produtoService.ObterAsync(id));

        /// <summary>Criar produto (Admin ou Gestor)</summary>
        [HttpPost]
        [Authorize(Roles = Roles.AdministradorOuGestor)]
        public async Task<IActionResult> CriarProduto([FromBody] CriarProdutoDto dto)
        {
            var criado = await _produtoService.CriarAsync(dto);
            return CreatedAtAction(nameof(ObterProduto), new { id = criado.Id }, criado);
        }

        /// <summary>Atualizar produto (Admin ou Gestor)</summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = Roles.AdministradorOuGestor)]
        public async Task<IActionResult> AtualizarProduto(int id, [FromBody] AtualizarProdutoDto dto)
        {
            await _produtoService.AtualizarAsync(id, dto);
            return Ok(new MensagemDto("Produto atualizado com sucesso"));
        }

        /// <summary>Deletar produto (apenas Admin)</summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = Roles.Administrador)]
        public async Task<IActionResult> DeletarProduto(int id)
        {
            await _produtoService.DeletarAsync(id);
            return Ok(new MensagemDto("Produto deletado com sucesso"));
        }
    }
}