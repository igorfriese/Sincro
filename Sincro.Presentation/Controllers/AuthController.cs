using Microsoft.AspNetCore.Mvc;
using Sincro.Application.DTOs;
using Sincro.Application.Interfaces;

namespace Sincro.Presentation.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto dto)
            => Ok(await _authService.LoginAsync(dto));

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(RefreshTokenRequestDto dto)
            => Ok(await _authService.RenovarTokenAsync(dto));
    }
}