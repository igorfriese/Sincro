using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sincro.Application.DTOs;
using Sincro.Application.Exceptions;
using Sincro.Application.Interfaces;
using Sincro.Domain;
using Sincro.Domain.Entities;

namespace Sincro.Application.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UsuarioService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<ListaUsuariosDto> ListarAsync(int pagina, int tamanho)
        {
            pagina = Math.Max(pagina, 1);
            tamanho = Math.Clamp(tamanho, 1, 100);

            var total = await _userManager.Users.CountAsync();
            var usuarios = await _userManager.Users
                .OrderByDescending(u => u.CreatedDate)
                .Skip((pagina - 1) * tamanho)
                .Take(tamanho)
                .ToListAsync();

            var dtos = new List<UsuarioListaDto>();
            foreach (var usuario in usuarios)
            {
                var roles = await _userManager.GetRolesAsync(usuario);
                dtos.Add(new UsuarioListaDto(usuario.Id, usuario.Email!, usuario.Nome,
                    roles.FirstOrDefault() ?? "Sem papel", usuario.CreatedDate));
            }

            return new ListaUsuariosDto(total, dtos);
        }

        public async Task<UsuarioListaDto> ObterAsync(string id, string? usuarioLogadoId, bool usuarioLogadoEhAdmin)
        {
            // Apenas Admin ou o próprio usuário pode ver os dados
            if (usuarioLogadoId != id && !usuarioLogadoEhAdmin)
                throw new AcessoNegadoException();

            var usuario = await ObterOuFalharAsync(id);
            var roles = await _userManager.GetRolesAsync(usuario);

            return new UsuarioListaDto(usuario.Id, usuario.Email!, usuario.Nome,
                roles.FirstOrDefault() ?? "Sem papel", usuario.CreatedDate);
        }

        public async Task<UsuarioListaDto> CriarAsync(CriarUsuarioDto dto)
        {
            if (await _userManager.FindByEmailAsync(dto.Email) is not null)
                throw new RegraDeNegocioException("Email já cadastrado");

            if (!Roles.Todas.Contains(dto.Role))
                throw new RegraDeNegocioException("Role inválido. Use: Administrador, Gestor ou Vendedor");

            var novoUsuario = new ApplicationUser
            {
                Email = dto.Email,
                UserName = dto.Email,
                Nome = dto.Nome,
                CreatedDate = DateTime.UtcNow
            };

            var resultado = await _userManager.CreateAsync(novoUsuario, dto.Senha);
            if (!resultado.Succeeded)
                throw new RegraDeNegocioException($"Erro ao criar usuário: {JuntarErros(resultado)}");

            var resultadoRole = await _userManager.AddToRoleAsync(novoUsuario, dto.Role);
            if (!resultadoRole.Succeeded)
            {
                await _userManager.DeleteAsync(novoUsuario); // não deixa usuário sem papel
                throw new RegraDeNegocioException($"Erro ao atribuir role: {JuntarErros(resultadoRole)}");
            }

            return new UsuarioListaDto(novoUsuario.Id, novoUsuario.Email!, novoUsuario.Nome, dto.Role, novoUsuario.CreatedDate);
        }

        public async Task AtualizarAsync(string id, AtualizarUsuarioDto dto)
        {
            var usuario = await ObterOuFalharAsync(id);
            usuario.Nome = dto.Nome;

            var resultado = await _userManager.UpdateAsync(usuario);
            if (!resultado.Succeeded)
                throw new RegraDeNegocioException($"Erro ao atualizar usuário: {JuntarErros(resultado)}");
        }

        public async Task DeletarAsync(string id, string? usuarioLogadoId)
        {
            if (id == usuarioLogadoId)
                throw new RegraDeNegocioException("Você não pode deletar o seu próprio usuário");

            var usuario = await ObterOuFalharAsync(id);

            var resultado = await _userManager.DeleteAsync(usuario);
            if (!resultado.Succeeded)
                throw new RegraDeNegocioException($"Erro ao deletar usuário: {JuntarErros(resultado)}");
        }

        public async Task<PerfilDto> ObterPerfilAsync(string? usuarioLogadoId)
        {
            var usuario = await ObterLogadoAsync(usuarioLogadoId);
            var roles = await _userManager.GetRolesAsync(usuario);

            return new PerfilDto(usuario.Id, usuario.Email, usuario.Nome, roles.FirstOrDefault());
        }

        public async Task AtualizarPerfilAsync(string? usuarioLogadoId, AtualizarMeuPerfilDto dto)
        {
            var usuario = await ObterLogadoAsync(usuarioLogadoId);
            usuario.Nome = dto.Nome;
            usuario.PhoneNumber = dto.Telefone;

            var resultado = await _userManager.UpdateAsync(usuario);
            if (!resultado.Succeeded)
                throw new RegraDeNegocioException(JuntarErros(resultado));
        }

        public async Task AlterarSenhaAsync(string? usuarioLogadoId, AlterarSenhaDto dto)
        {
            var usuario = await ObterLogadoAsync(usuarioLogadoId);

            var resultado = await _userManager.ChangePasswordAsync(usuario, dto.SenhaAnterior, dto.NovaSenha);
            if (!resultado.Succeeded)
                throw new RegraDeNegocioException(JuntarErros(resultado));
        }

        // ---------- helpers ----------

        private async Task<ApplicationUser> ObterOuFalharAsync(string id)
            => await _userManager.FindByIdAsync(id)
               ?? throw new NaoEncontradoException("Usuário não encontrado");

        private async Task<ApplicationUser> ObterLogadoAsync(string? usuarioLogadoId)
        {
            if (string.IsNullOrEmpty(usuarioLogadoId))
                throw new NaoAutorizadoException();

            return await ObterOuFalharAsync(usuarioLogadoId);
        }

        private static string JuntarErros(IdentityResult resultado)
            => string.Join(", ", resultado.Errors.Select(e => e.Description));
    }
}