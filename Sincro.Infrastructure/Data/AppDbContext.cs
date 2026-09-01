using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Sincro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sincro.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<Cliente> Clientes => Set<Cliente>();
        public DbSet<Produto> Produtos => Set<Produto>();
        public DbSet<Pedido> Pedidos => Set<Pedido>();
        public DbSet<Etapa> Etapas => Set<Etapa>();
        public DbSet<EventoPedido> EventoPedidos => Set<EventoPedido>();
    }
}
