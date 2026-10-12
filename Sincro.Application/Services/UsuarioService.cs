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

        public async Task<List<ResponsavelDto>> ListarResponsaveisAsync()
        {
            var usuarios = await _userManager.Users.OrderBy(u => u.Nome).ToListAsync();

            var responsaveis = new List<ResponsavelDto>();
            foreach (var usuario in usuarios)
            {
                var roles = await _userManager.GetRolesAsync(usuario);
                responsaveis.Add(new ResponsavelDto(usuario.Id, usuario.Nome, roles.FirstOrDefault() ?? "Sem papel"));
            }

            return responsaveis;
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

        public async Task<UsuarioListaDto> AtualizarAsync(string id, AtualizarUsuarioDto dto)
        {
            var usuario = await ObterOuFalharAsync(id);

            if (!Roles.Todas.Contains(dto.Role))
                throw new RegraDeNegocioException("Perfil inválido. Use: Administrador, Gestor ou Vendedor");

            var rolesAtuais = await _userManager.GetRolesAsync(usuario);
            var eraAdmin = rolesAtuais.Contains(Roles.Administrador);

            if (eraAdmin && dto.Role != Roles.Administrador && await EhUltimoAdministradorAsync())
                throw new RegraDeNegocioException("Não é possível rebaixar o último Administrador do sistema");

            usuario.Nome = dto.Nome.Trim();
            await TrocarEmailAsync(usuario, dto.Email);

            var resultado = await _userManager.UpdateAsync(usuario);
            if (!resultado.Succeeded)
                throw new RegraDeNegocioException($"Erro ao atualizar usuário: {JuntarErros(resultado)}");

            // Troca de perfil: sai dos papéis antigos e entra no novo
            if (!(rolesAtuais.Count == 1 && rolesAtuais[0] == dto.Role))
            {
                await _userManager.RemoveFromRolesAsync(usuario, rolesAtuais);
                await _userManager.AddToRoleAsync(usuario, dto.Role);
            }

            // Senha definida pelo Administrador (não exige a senha antiga)
            if (!string.IsNullOrWhiteSpace(dto.NovaSenha))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
                var resultadoSenha = await _userManager.ResetPasswordAsync(usuario, token, dto.NovaSenha);
                if (!resultadoSenha.Succeeded)
                    throw new RegraDeNegocioException(JuntarErros(resultadoSenha));
            }

            return new UsuarioListaDto(usuario.Id, usuario.Email!, usuario.Nome, dto.Role, usuario.CreatedDate);
        }

        public async Task DeletarAsync(string id, string? usuarioLogadoId)
        {
            if (id == usuarioLogadoId)
                throw new RegraDeNegocioException("Você não pode deletar o seu próprio usuário");

            var usuario = await ObterOuFalharAsync(id);

            if (await _userManager.IsInRoleAsync(usuario, Roles.Administrador) && await EhUltimoAdministradorAsync())
                throw new RegraDeNegocioException("Não é possível excluir o último Administrador do sistema");

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

        public async Task<PerfilDto> AtualizarPerfilAsync(string? usuarioLogadoId, AtualizarMeuPerfilDto dto)
        {
            var usuario = await ObterLogadoAsync(usuarioLogadoId);
            usuario.Nome = dto.Nome.Trim();
            if (dto.Telefone is not null)
                usuario.PhoneNumber = string.IsNullOrWhiteSpace(dto.Telefone) ? null : dto.Telefone.Trim();
            await TrocarEmailAsync(usuario, dto.Email);

            var resultado = await _userManager.UpdateAsync(usuario);
            if (!resultado.Succeeded)
                throw new RegraDeNegocioException(JuntarErros(resultado));

            var roles = await _userManager.GetRolesAsync(usuario);
            return new PerfilDto(usuario.Id, usuario.Email, usuario.Nome, roles.FirstOrDefault());
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

        private async Task<bool> EhUltimoAdministradorAsync()
            => (await _userManager.GetUsersInRoleAsync(Roles.Administrador)).Count <= 1;

        // O email também é o login (UserName), então os dois mudam juntos
        private async Task TrocarEmailAsync(ApplicationUser usuario, string novoEmail)
        {
            novoEmail = novoEmail.Trim();
            if (string.Equals(usuario.Email, novoEmail, StringComparison.OrdinalIgnoreCase))
                return;

            var dono = await _userManager.FindByEmailAsync(novoEmail);
            if (dono is not null && dono.Id != usuario.Id)
                throw new RegraDeNegocioException("Email já cadastrado");

            usuario.Email = novoEmail;
            usuario.UserName = novoEmail;
        }

        private static string JuntarErros(IdentityResult resultado)
            => string.Join(", ", resultado.Errors.Select(e => e.Description));
    }
}