using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sincro.Application.DTOs;
using Sincro.Application.Interfaces;
using Sincro.Domain;
using System.Security.Claims;

namespace Sincro.Presentation.Controllers
{
    [ApiController]
    [Route("api/usuarios")]
    [Authorize]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;

        public UsuariosController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        private string? UsuarioLogadoId => User.FindFirstValue(ClaimTypes.NameIdentifier);

        /// <summary>Listar todos os usuários (apenas Admin)</summary>
        [HttpGet]
        [Authorize(Roles = Roles.Administrador)]
        public async Task<IActionResult> ListarUsuarios([FromQuery] int pagina = 1, [FromQuery] int tamanho = 10)
            => Ok(await _usuarioService.ListarAsync(pagina, tamanho));

        /// <summary>Obter detalhes de um usuário (apenas Admin ou o próprio usuário)</summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> ObterUsuario(string id)
            => Ok(await _usuarioService.ObterAsync(id, UsuarioLogadoId, User.IsInRole(Roles.Administrador)));

        /// <summary>Criar novo usuário (apenas Admin)</summary>
        [HttpPost]
        [Authorize(Roles = Roles.Administrador)]
        public async Task<IActionResult> CriarUsuario([FromBody] CriarUsuarioDto dto)
        {
            var criado = await _usuarioService.CriarAsync(dto);
            return CreatedAtAction(nameof(ObterUsuario), new { id = criado.Id }, criado);
        }

        /// <summary>Atualizar usuário (apenas Admin)</summary>
        [HttpPut("{id}")]
        [Authorize(Roles = Roles.Administrador)]
        public async Task<IActionResult> AtualizarUsuario(string id, [FromBody] AtualizarUsuarioDto dto)
        {
            await _usuarioService.AtualizarAsync(id, dto);
            return Ok(new MensagemDto("Usuário atualizado com sucesso"));
        }

        /// <summary>Deletar usuário (apenas Admin)</summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = Roles.Administrador)]
        public async Task<IActionResult> DeletarUsuario(string id)
        {
            await _usuarioService.DeletarAsync(id, UsuarioLogadoId);
            return Ok(new MensagemDto("Usuário deletado com sucesso"));
        }

        /// <summary>Obter dados do usuário logado</summary>
        [HttpGet("me/dados")]
        public async Task<IActionResult> ObterMeuPerfil()
            => Ok(await _usuarioService.ObterPerfilAsync(UsuarioLogadoId));

        /// <summary>Atualizar dados do próprio perfil</summary>
        [HttpPut("me")]
        public async Task<IActionResult> AtualizarMeuPerfil([FromBody] AtualizarMeuPerfilDto dto)
        {
            await _usuarioService.AtualizarPerfilAsync(UsuarioLogadoId, dto);
            return Ok(new MensagemDto("Perfil atualizado com sucesso"));
        }

        /// <summary>Alterar senha do usuário logado</summary>
        [HttpPut("me/senha")]
        public async Task<IActionResult> AlterarSenha([FromBody] AlterarSenhaDto dto)
        {
            await _usuarioService.AlterarSenhaAsync(UsuarioLogadoId, dto);
            return Ok(new MensagemDto("Senha alterada com sucesso"));
        }
    }
}