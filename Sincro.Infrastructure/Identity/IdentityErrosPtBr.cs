using Microsoft.AspNetCore.Identity;

namespace Sincro.Infrastructure.Identity
{
    // Mensagens do ASP.NET Identity em português (o padrão vem em inglês e chega até a tela)
    public class IdentityErrosPtBr : IdentityErrorDescriber
    {
        public override IdentityError PasswordTooShort(int length)
            => new() { Code = nameof(PasswordTooShort), Description = $"A senha deve ter no mínimo {length} caracteres" };

        public override IdentityError PasswordRequiresDigit()
            => new() { Code = nameof(PasswordRequiresDigit), Description = "A senha deve ter ao menos um número" };

        public override IdentityError PasswordRequiresLower()
            => new() { Code = nameof(PasswordRequiresLower), Description = "A senha deve ter ao menos uma letra minúscula" };

        public override IdentityError PasswordRequiresUpper()
            => new() { Code = nameof(PasswordRequiresUpper), Description = "A senha deve ter ao menos uma letra maiúscula" };

        public override IdentityError PasswordRequiresNonAlphanumeric()
            => new() { Code = nameof(PasswordRequiresNonAlphanumeric), Description = "A senha deve ter ao menos um símbolo" };

        public override IdentityError PasswordMismatch()
            => new() { Code = nameof(PasswordMismatch), Description = "Senha atual incorreta" };

        public override IdentityError DuplicateEmail(string email)
            => new() { Code = nameof(DuplicateEmail), Description = $"O email {email} já está em uso" };

        public override IdentityError DuplicateUserName(string userName)
            => new() { Code = nameof(DuplicateUserName), Description = $"O email {userName} já está em uso" };

        public override IdentityError InvalidEmail(string? email)
            => new() { Code = nameof(InvalidEmail), Description = $"Email inválido: {email}" };

        public override IdentityError InvalidUserName(string? userName)
            => new() { Code = nameof(InvalidUserName), Description = $"Login inválido: {userName}" };

        public override IdentityError InvalidToken()
            => new() { Code = nameof(InvalidToken), Description = "Token inválido ou expirado" };
    }
}
