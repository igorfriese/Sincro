using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sincro.Domain;
using Sincro.Domain.Entities;

namespace Sincro.Infrastructure.Data
{
    // Dados de exemplo para desenvolvimento e para a apresentação.
    // Só roda em Development (ver Program.cs) e só quando ainda não há clientes nem pedidos,
    // então nunca mistura com dados reais. Datas são relativas a hoje, para o Kanban,
    // os alertas e os relatórios sempre terem o que mostrar.
    public static class DemoSeeder
    {
        private const string SenhaDemo = "Sincro@123";

        // Etapa: índice na lista de etapas ordenadas (0 = primeira); -1 = última (finalizado).
        // InicioDiasAtras: quando entrou na primeira etapa. PassoDias: tempo em cada etapa.
        // PrazoDias: prazo relativo a hoje (negativo = já passou).
        private record PedidoDemo(
            int Cliente, int Produto, int Quantidade, bool Urgente, string? Responsavel,
            int Etapa, int InicioDiasAtras, int PassoDias, int PrazoDias);

        public static async Task SeedAsync(IServiceProvider services)
        {
            var db = services.GetRequiredService<ApplicationDbContext>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            if (await db.Clientes.AnyAsync() || await db.Pedidos.AnyAsync())
                return;

            var etapas = await db.Etapas.OrderBy(e => e.Ordem).ToListAsync();
            if (etapas.Count < 2)
                return;

            // ---------- Usuários ----------
            var admin = await userManager.FindByEmailAsync("admin@sincro.com");
            var gestor = await CriarUsuarioAsync(userManager, "gestor@sincro.com", "Carlos Mendes", Roles.Gestor);
            var vendedora = await CriarUsuarioAsync(userManager, "vendedor@sincro.com", "Júlia Rocha", Roles.Vendedor);
            var vendedor2 = await CriarUsuarioAsync(userManager, "vendedor2@sincro.com", "Marcos Lima", Roles.Vendedor);

            var responsaveis = new Dictionary<string, string?>
            {
                ["julia"] = vendedora?.Id,
                ["marcos"] = vendedor2?.Id,
            };
            var quemRegistra = gestor?.Id ?? admin?.Id;

            // ---------- Clientes ----------
            var clientes = new List<Cliente>
            {
                NovoCliente(1, "Confecção Aurora", "11.111.111/0001-11", "(47) 3333-1001", "compras@aurora.exemplo", "Rua das Palmeiras, 120 - Blumenau"),
                NovoCliente(2, "Moda Vale Azul", "22.222.222/0001-22", "(47) 3333-1002", "pcp@valeazul.exemplo", "Av. Central, 845 - Brusque"),
                NovoCliente(3, "Malhas Serra Verde", "33.333.333/0001-33", "(47) 3333-1003", "contato@serraverde.exemplo", "Rua XV de Novembro, 50 - Gaspar"),
                NovoCliente(4, "Ateliê Ponto Firme", "44.444.444/0001-44", "(47) 3333-1004", "atelie@pontofirme.exemplo", "Rua Itajaí, 300 - Indaial"),
            };
            db.Clientes.AddRange(clientes);

            // ---------- Produtos ----------
            var produtos = new List<Produto>
            {
                NovoProduto(1, "Camiseta básica", "Malha 30.1 penteada", 12.50m),
                NovoProduto(2, "Moletom canguru", "Moletom flanelado com capuz", 38.00m),
                NovoProduto(3, "Calça jogger", "Moletinho com punho", 29.90m),
                NovoProduto(4, "Vestido midi", "Viscose estampada", 45.00m),
                NovoProduto(5, "Polo piquet", "Piquet com gola e punho", 24.00m),
            };
            db.Produtos.AddRange(produtos);

            await db.SaveChangesAsync(); // gera os Ids de clientes e produtos

            // ---------- Pedidos ----------
            var pedidosDemo = new List<PedidoDemo>
            {
                // Em andamento — espalhados pelo Kanban, com prazos folgados, apertados e vencidos
                new(0, 0, 500, false, "julia",  0, 1, 1, 20),
                new(1, 2, 120, true,  null,     0, 0, 1, 2),
                new(2, 1, 200, false, "julia",  1, 4, 2, 10),
                new(3, 3, 80,  true,  null,     1, 6, 3, -1),
                new(0, 4, 300, false, "marcos", 1, 3, 1, 15),
                new(1, 0, 650, false, "julia",  2, 8, 3, 3),
                new(2, 2, 150, false, null,     3, 10, 2, 5),
                new(3, 1, 90,  false, "marcos", 3, 12, 3, -2),
                new(0, 3, 60,  false, "julia",  4, 9, 2, 1),

                // Finalizados — alimentam Histórico e Relatórios (uns no prazo, outros atrasados)
                new(0, 0, 400, false, null,     -1, 40, 3, -24), // concluído há 25 dias: no prazo
                new(1, 1, 180, false, "julia",  -1, 35, 3, -22), // há 20 dias: atrasado
                new(2, 4, 250, false, null,     -1, 28, 2, -15), // há 18 dias: no prazo
                new(3, 2, 100, false, null,     -1, 20, 2, -10), // há 10 dias, no dia do prazo: no prazo
                new(0, 1, 220, true,  "julia",  -1, 15, 2, -7),  // há 5 dias: atrasado
                new(1, 4, 320, false, "marcos", -1, 9, 1, -1),   // há 4 dias: no prazo
                new(2, 0, 700, false, null,     -1, 60, 4, -45), // há 40 dias: atrasado
            };

            var ultima = etapas.Count - 1;
            var hoje = DateTime.Today;

            foreach (var demo in pedidosDemo)
            {
                // Pedidos em andamento nunca caem na última etapa, mesmo que existam menos colunas
                var indiceEtapa = demo.Etapa < 0 ? ultima : Math.Min(demo.Etapa, ultima - 1);
                var inicio = hoje.AddDays(-demo.InicioDiasAtras).AddHours(8);

                var pedido = new Pedido
                {
                    ClienteId = clientes[demo.Cliente].Id,
                    ProdutoId = produtos[demo.Produto].Id,
                    ResponsavelId = demo.Responsavel is null ? null : responsaveis[demo.Responsavel],
                    Quantidade = demo.Quantidade,
                    Prazo = hoje.AddDays(demo.PrazoDias),
                    Urgente = demo.Urgente,
                    Coluna = etapas[indiceEtapa].Chave,
                };
                db.Pedidos.Add(pedido);

                // Um evento por etapa percorrida, como o Kanban registra
                for (var i = 0; i <= indiceEtapa; i++)
                {
                    db.EventoPedidos.Add(new EventoPedido
                    {
                        Pedido = pedido, // o EF preenche o PedidoId ao salvar
                        Etapa = etapas[i].Chave,
                        DataHora = inicio.AddDays(i * demo.PassoDias).AddHours(i),
                        Observacao = i == 0
                            ? "Pedido criado e corte recebido."
                            : $"Pedido movido para \"{etapas[i].Nome}\".",
                        RegistradoPorId = quemRegistra,
                    });
                }
            }

            await db.SaveChangesAsync();

            Console.WriteLine($"[Seed demo] {clientes.Count} clientes, {produtos.Count} produtos e {pedidosDemo.Count} pedidos de exemplo criados.");
            Console.WriteLine($"[Seed demo] Usuários: gestor@sincro.com, vendedor@sincro.com, vendedor2@sincro.com / {SenhaDemo}");
        }

        // ---------- helpers ----------

        private static async Task<ApplicationUser?> CriarUsuarioAsync(
            UserManager<ApplicationUser> userManager, string email, string nome, string role)
        {
            var existente = await userManager.FindByEmailAsync(email);
            if (existente is not null)
                return existente;

            var usuario = new ApplicationUser
            {
                UserName = email,
                Email = email,
                Nome = nome,
                EmailConfirmed = true,
            };

            var resultado = await userManager.CreateAsync(usuario, SenhaDemo);
            if (!resultado.Succeeded)
            {
                foreach (var erro in resultado.Errors)
                    Console.WriteLine($"[Seed demo] Erro ao criar {email}: {erro.Description}");
                return null;
            }

            await userManager.AddToRoleAsync(usuario, role);
            return usuario;
        }

        private static Cliente NovoCliente(int numero, string nome, string documento, string telefone, string email, string endereco)
            => new()
            {
                Codigo = $"CLI-{numero:D4}",
                Nome = nome,
                Documento = documento,
                Telefone = telefone,
                Email = email,
                Endereco = endereco,
                TokenAcompanhamento = Guid.NewGuid().ToString("N"),
            };

        private static Produto NovoProduto(int numero, string nome, string descricao, decimal preco)
            => new()
            {
                Codigo = $"PRD-{numero:D4}",
                Nome = nome,
                Descricao = descricao,
                PrecoBase = preco,
                Estoque = 0,
            };
    }
}
