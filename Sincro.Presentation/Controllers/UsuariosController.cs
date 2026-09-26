using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sincro.Domain.Entities;
using Sincro.Presentation.Dtos;
using Sincro.Presentation.DTOs;
using System.Security.Claims;

namespace Sincro.Presentation.Controllers
{
    [ApiController]
    [Route("api/usuarios")]
    [Authorize]
    public class UsuariosController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UsuariosController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        /// <summary>
        /// Listar todos os usuários (apenas Admin)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ListarUsuarios([FromQuery] int pagina = 1, [FromQuery] int tamanho = 10)
        {
            try
            {
                var usuarios = _userManager.Users
                    .OrderByDescending(u => u.CreatedDate)
                    .Skip((pagina - 1) * tamanho)
                    .Take(tamanho)
                    .ToList();

                var dtos = new List<UsuarioListaDto>();
                foreach (var usuario in usuarios)
                {
                    var roles = await _userManager.GetRolesAsync(usuario);
                    dtos.Add(new UsuarioListaDto(
                        usuario.Id,
                        usuario.Email!,
                        usuario.Nome,
                        roles.FirstOrDefault() ?? "Sem papel",
                        usuario.CreatedDate
                    ));
                }

                return Ok(new { total = _userManager.Users.Count(), usuarios = dtos });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErroDto($"Erro ao listar usuários: {ex.Message}"));
            }
        }

        /// <summary>
        /// Obter detalhes de um usuário (apenas Admin ou o próprio usuário)
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> ObterUsuario(string id)
        {
            try
            {
                var usuarioLogado = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var isAdmin = User.IsInRole("Administrador");

                // Apenas Admin ou o próprio usuário pode ver os dados
                if (usuarioLogado != id && !isAdmin)
                    return Forbid();

                var usuario = await _userManager.FindByIdAsync(id);
                if (usuario == null)
                    return NotFound(new ErroDto("Usuário não encontrado"));

                var roles = await _userManager.GetRolesAsync(usuario);

                return Ok(new UsuarioListaDto(
                    usuario.Id,
                    usuario.Email!,
                    usuario.Nome,
                    roles.FirstOrDefault() ?? "Sem papel",
                    usuario.CreatedDate
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErroDto($"Erro ao obter usuário: {ex.Message}"));
            }
        }

        /// <summary>
        /// Criar novo usuário (apenas Admin)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> CriarUsuario([FromBody] CriarUsuarioDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Verificar se email já existe
                var usuarioExistente = await _userManager.FindByEmailAsync(dto.Email);
                if (usuarioExistente != null)
                    return BadRequest(new ErroDto("Email já cadastrado"));

                // Validar role
                var rolesValidas = new[] { "Administrador", "Gestor", "Vendedor" };
                if (!rolesValidas.Contains(dto.Role))
                    return BadRequest(new ErroDto("Role inválido. Use: Administrador, Gestor ou Vendedor"));

                var novoUsuario = new ApplicationUser
                {
                    Email = dto.Email,
                    UserName = dto.Email,
                    Nome = dto.Nome,
                    CreatedDate = DateTime.UtcNow
                };

                var resultado = await _userManager.CreateAsync(novoUsuario, dto.Senha);
                if (!resultado.Succeeded)
                    return BadRequest(new ErroDto($"Erro ao criar usuário: {string.Join(", ", resultado.Errors.Select(e => e.Description))}"));

                // Atribuir role
                await _userManager.AddToRoleAsync(novoUsuario, dto.Role);

                return CreatedAtAction(nameof(ObterUsuario), new { id = novoUsuario.Id },
                    new UsuarioListaDto(novoUsuario.Id, novoUsuario.Email!, novoUsuario.Nome, dto.Role, novoUsuario.CreatedDate));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErroDto($"Erro ao criar usuário: {ex.Message}"));
            }
        }

        /// <summary>
        /// Atualizar usuário (apenas Admin)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> AtualizarUsuario(string id, [FromBody] AtualizarUsuarioDto dto)
        {
            try
            {
                var usuario = await _userManager.FindByIdAsync(id);
                if (usuario == null)
                    return NotFound(new ErroDto("Usuário não encontrado"));

                usuario.Nome = dto.Nome;

                var resultado = await _userManager.UpdateAsync(usuario);
                if (!resultado.Succeeded)
                    return BadRequest(new ErroDto("Erro ao atualizar usuário"));

                return Ok(new { mensagem = "Usuário atualizado com sucesso" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErroDto($"Erro ao atualizar usuário: {ex.Message}"));
            }
        }

        /// <summary>
        /// Deletar usuário (soft delete — apenas Admin)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeletarUsuario(string id)
        {
            try
            {
                var usuario = await _userManager.FindByIdAsync(id);
                if (usuario == null)
                    return NotFound(new ErroDto("Usuário não encontrado"));

                var resultado = await _userManager.DeleteAsync(usuario);
                if (!resultado.Succeeded)
                    return BadRequest(new ErroDto("Erro ao deletar usuário"));

                return Ok(new { mensagem = "Usuário deletado com sucesso" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErroDto($"Erro ao deletar usuário: {ex.Message}"));
            }
        }

        /// <summary>
        /// Obter dados do usuário logado
        /// </summary>
        [HttpGet("me/dados")]
        public async Task<IActionResult> ObterMeuPerfil()
        {
            try
            {
                var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(usuarioId))
                    return Unauthorized();

                var usuario = await _userManager.FindByIdAsync(usuarioId);
                if (usuario == null)
                    return NotFound();

                var roles = await _userManager.GetRolesAsync(usuario);

                return Ok(new
                {
                    id = usuario.Id,
                    email = usuario.Email,
                    nome = usuario.Nome,
                    role = roles.FirstOrDefault()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErroDto($"Erro: {ex.Message}"));
            }
        }

        /// <summary>
        /// Atualizar dados do próprio perfil (usuário logado)
        /// </summary>
        [HttpPut("me")]
        public async Task<IActionResult> AtualizarMeuPerfil([FromBody] AtualizarMeuPerfilDto dto)
        {
            try
            {
                var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(usuarioId))
                    return Unauthorized();

                var usuario = await _userManager.FindByIdAsync(usuarioId);
                if (usuario == null)
                    return NotFound();

                usuario.Nome = dto.Nome;
                usuario.PhoneNumber = dto.Telefone;

                var resultado = await _userManager.UpdateAsync(usuario);
                if (!resultado.Succeeded)
                    return BadRequest(new ErroDto(string.Join(", ", resultado.Errors.Select(e => e.Description))));

                return Ok(new { mensagem = "Perfil atualizado com sucesso" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErroDto($"Erro: {ex.Message}"));
            }
        }


        /// <summary>
        /// Alterar senha do usuário logado
        /// </summary>
        [HttpPut("me/senha")]
        public async Task<IActionResult> AlterarSenha([FromBody] AlterarSenhaDto dto)
        {
            try
            {
                var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(usuarioId))
                    return Unauthorized();

                var usuario = await _userManager.FindByIdAsync(usuarioId);
                if (usuario == null)
                    return NotFound();

                var resultado = await _userManager.ChangePasswordAsync(usuario, dto.SenhaAnterior, dto.NovaSenha);
                if (!resultado.Succeeded)
                    return BadRequest(new ErroDto(string.Join(", ", resultado.Errors.Select(e => e.Description))));

                return Ok(new { mensagem = "Senha alterada com sucesso" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErroDto($"Erro: {ex.Message}"));
            }
        }
    }
}