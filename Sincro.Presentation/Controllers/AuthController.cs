using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sincro.Application.Services;
using Sincro.Domain.Entities;
using Sincro.Presentation.Dtos;

namespace Sincro.Presentation.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly TokenService _tokenService;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            TokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto dto)
        {
            var usuario = await _userManager.FindByEmailAsync(dto.Email);
            if (usuario is null)
                return Unauthorized(new ErroDto("Credenciais inválidas"));

            var resultado = await _signInManager.CheckPasswordSignInAsync(usuario, dto.Senha, lockoutOnFailure: false);
            if (!resultado.Succeeded)
                return Unauthorized(new ErroDto("Credenciais inválidas"));

            var roles = await _userManager.GetRolesAsync(usuario);
            var token = _tokenService.GerarToken(usuario, roles);
            var refreshToken = _tokenService.GerarRefreshToken();

            usuario.RefreshToken = refreshToken;
            usuario.RefreshTokenExpiraEm = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(usuario);

            var usuarioDto = new UsuarioDto(usuario.Id, usuario.Nome, usuario.Email!, roles.FirstOrDefault() ?? "");
            return Ok(new LoginResponseDto(token, refreshToken, usuarioDto));
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(RefreshTokenRequestDto dto)
        {
            var usuario = _userManager.Users.FirstOrDefault(u => u.RefreshToken == dto.RefreshToken);
            if (usuario is null || usuario.RefreshTokenExpiraEm < DateTime.UtcNow)
                return Unauthorized(new ErroDto("Refresh token inválido ou expirado"));

            var roles = await _userManager.GetRolesAsync(usuario);
            var novoToken = _tokenService.GerarToken(usuario, roles);
            var novoRefreshToken = _tokenService.GerarRefreshToken();

            usuario.RefreshToken = novoRefreshToken;
            usuario.RefreshTokenExpiraEm = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(usuario);

            var usuarioDto = new UsuarioDto(usuario.Id, usuario.Nome, usuario.Email!, roles.FirstOrDefault() ?? "");
            return Ok(new LoginResponseDto(novoToken, novoRefreshToken, usuarioDto));
        }
    }
}
