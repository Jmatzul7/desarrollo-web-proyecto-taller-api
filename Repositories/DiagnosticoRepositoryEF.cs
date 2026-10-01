using Microsoft.EntityFrameworkCore;
using TallerMecanicoAPI.Helpers;
using TallerMecanicoAPI.Data;
using TallerMecanicoAPI.Models;

namespace TallerMecanicoAPI.Repositories
{
    public class DiagnosticoRepositoryEF : IDiagnosticoRepository
    {
        private readonly AppDbContext _context;
        public DiagnosticoRepositoryEF(AppDbContext context) => _context = context;

        private IQueryable<Diagnostico> Consulta() =>
            _context.Diagnosticos
                .Include(d => d.Orden)
                    .ThenInclude(o => o!.Vehiculo)
                        .ThenInclude(v => v!.Cliente)
                .Include(d => d.Orden)
                    .ThenInclude(o => o!.Estado)
                .Include(d => d.Mecanico)
                    .ThenInclude(m => m!.Rol);

        public async Task<List<Diagnostico>> GetAll() => await Consulta().ToListAsync();

        public async Task<List<Diagnostico>> GetByOrden(int idOrden) =>
            await Consulta().Where(d => d.IdOrden == idOrden).ToListAsync();

        public async Task<Diagnostico?> GetById(int id) =>
            await Consulta().FirstOrDefaultAsync(d => d.IdDiagnostico == id);

        public async Task Add(Diagnostico diagnostico)
        {
            _context.Diagnosticos.Add(diagnostico);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var diagnostico = await _context.Diagnosticos.FindAsync(id);
            if (diagnostico == null) return;

            _context.Diagnosticos.Remove(diagnostico);
            await DbErrorHandler.GuardarOManejarConflicto(() => _context.SaveChangesAsync(), "diagnóstico");
        }
    }
}
