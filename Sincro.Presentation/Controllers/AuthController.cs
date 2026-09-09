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

            var usuarioDto = new UsuarioDto(usuario.Id, usuario.Nome, usuario.Email!, roles.FirstOrDefault() ?? "");
            return Ok(new LoginResponseDto(token, usuarioDto));
        }
    }
}
