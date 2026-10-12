using Sincro.Application.DTOs;
using Sincro.Application.Exceptions;
using Sincro.Domain;
using System.Security.Claims;

namespace Sincro.Presentation.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        // O .NET troca o claim "sub" do JWT por ClaimTypes.NameIdentifier ao ler o token.
        // SomenteProprios = Vendedor (sem perfil Administrador/Gestor junto).
        public static UsuarioAtual ParaUsuarioAtual(this ClaimsPrincipal user)
        {
            var id = user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new NaoAutorizadoException();

            var somenteProprios = user.IsInRole(Roles.Vendedor)
                && !user.IsInRole(Roles.Administrador)
                && !user.IsInRole(Roles.Gestor);

            return new UsuarioAtual(id, somenteProprios);
        }
    }
}
