using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sincro.Domain.Interfaces;

namespace Sincro.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

    public async Task SalvarAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
