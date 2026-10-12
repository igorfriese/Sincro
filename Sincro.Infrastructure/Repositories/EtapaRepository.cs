using Sincro.Domain.Entities;
using Sincro.Domain.Interfaces;
using Sincro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sincro.Infrastructure.Repositories
{
    public class EtapaRepository : IEtapaRepository
    {
        private readonly ApplicationDbContext _context;

        public EtapaRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Etapa>> ListarTodosAsync()
        {
            return await _context.Etapas.ToListAsync();
        }

        public async Task<Etapa?> ObterPorIdAsync(int id)
        {
            return await _context.Etapas.FindAsync(id);
        }

        public async Task AdicionarAsync(Etapa etapa)
        {
            await _context.Etapas.AddAsync(etapa);
        }

        public void Atualizar(Etapa etapa)
        {
            _context.Etapas.Update(etapa);
        }

        public void Remover(Etapa etapa)
        {
            _context.Etapas.Remove(etapa);
        }
    }
}
