using Microsoft.AspNetCore.Identity;
using Sincro.Domain;
using Sincro.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace Sincro.Infrastructure.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var db = services.GetRequiredService<ApplicationDbContext>();

            foreach (var role in Roles.Todas)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            const string emailAdmin = "admin@sincro.com";
            if (await userManager.FindByEmailAsync(emailAdmin) is null)
            {
                var admin = new ApplicationUser
                {
                    UserName = emailAdmin,
                    Email = emailAdmin,
                    Nome = "Administrador Sincro",
                    EmailConfirmed = true,
                };
                const string senhaInicial = "Sincro@123";
                var resultado = await userManager.CreateAsync(admin, senhaInicial);
                if (resultado.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, Roles.Administrador);
                    Console.WriteLine($"[Seed] Admin criado: {emailAdmin} / {senhaInicial}");
                }
                else
                {
                    foreach (var erro in resultado.Errors)
                        Console.WriteLine($"[Seed] Erro ao criar admin: {erro.Description}");
                }
            }

            if (!db.Etapas.Any())
            {
                db.Etapas.AddRange(
                    new Etapa { Chave = "corte", Nome = "Corte recebido", Cor = "#3C3489", Ordem = 1 },
                    new Etapa { Chave = "costura", Nome = "Em costura", Cor = "#5B4FC9", Ordem = 2 },
                    new Etapa { Chave = "acabamento", Nome = "Acabamento", Cor = "#8577E0", Ordem = 3 },
                    new Etapa { Chave = "revisao", Nome = "Revisão", Cor = "#B06A12", Ordem = 4 },
                    new Etapa { Chave = "entregue", Nome = "Entregue", Cor = "#2C7A9E", Ordem = 5 },
                    new Etapa { Chave = "finalizado", Nome = "Finalizado", Cor = "#1D9E75", Ordem = 6 }
                );
                await db.SaveChangesAsync();
                Console.WriteLine("[Seed] Etapas padrão do Kanban criadas.");
            }
        }
    }
}
