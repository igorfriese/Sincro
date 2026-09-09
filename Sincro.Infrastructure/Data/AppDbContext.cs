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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Cliente>().HasIndex(c => c.Codigo).IsUnique();
            builder.Entity<Cliente>().HasIndex(c => c.TokenAcompanhamento).IsUnique();
            builder.Entity<Produto>().Property(p => p.PrecoBase).HasPrecision(10, 2);
            builder.Entity<Etapa>().HasIndex(e => e.Chave).IsUnique();
            builder.Entity<Pedido>()
                .HasOne(p => p.Cliente)
                .WithMany(c => c.Pedidos)
                .HasForeignKey(p => p.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
