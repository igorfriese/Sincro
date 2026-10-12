using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sincro.Application.DTOs;
using Sincro.Application.Exceptions;
using Sincro.Application.Interfaces;
using Sincro.Domain.Entities;

namespace Sincro.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly TokenService _tokenService;

        public AuthService(UserManager<ApplicationUser> userManager, TokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto dto)
        {
            var usuario = await _userManager.FindByEmailAsync(dto.Email);

            // Mesma mensagem para email inexistente e senha errada (não revela se o email existe)
            const string mensagemInvalido = "E-mail ou senha incorretos.";
            const string mensagemBloqueado = "Muitas tentativas erradas. Aguarde 15 minutos e tente de novo.";

            if (usuario is null)
                throw new NaoAutorizadoException(mensagemInvalido);

            if (await _userManager.IsLockedOutAsync(usuario))
                throw new NaoAutorizadoException(mensagemBloqueado);

            if (!await _userManager.CheckPasswordAsync(usuario, dto.Senha))
            {
                await _userManager.AccessFailedAsync(usuario); // conta a falha; bloqueia ao atingir o limite
                throw new NaoAutorizadoException(await _userManager.IsLockedOutAsync(usuario)
                    ? mensagemBloqueado
                    : mensagemInvalido);
            }

            await _userManager.ResetAccessFailedCountAsync(usuario);

            return await GerarRespostaAsync(usuario);
        }

        public async Task<LoginResponseDto> RenovarTokenAsync(RefreshTokenRequestDto dto)
        {
            var usuario = await _userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == dto.RefreshToken);

            if (usuario is null
                || usuario.RefreshTokenExpiraEm is null
                || usuario.RefreshTokenExpiraEm < DateTime.UtcNow
                || await _userManager.IsLockedOutAsync(usuario))
                throw new NaoAutorizadoException("Refresh token inválido ou expirado");

            return await GerarRespostaAsync(usuario);
        }

        private async Task<LoginResponseDto> GerarRespostaAsync(ApplicationUser usuario)
        {
            var roles = await _userManager.GetRolesAsync(usuario);
            var token = _tokenService.GerarToken(usuario, roles);
            var refreshToken = _tokenService.GerarRefreshToken();

            usuario.RefreshToken = refreshToken;
            usuario.RefreshTokenExpiraEm = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(usuario);

            var usuarioDto = new UsuarioDto(usuario.Id, usuario.Nome, usuario.Email!, roles.FirstOrDefault() ?? "");
            return new LoginResponseDto(token, refreshToken, usuarioDto);
        }
    }
}