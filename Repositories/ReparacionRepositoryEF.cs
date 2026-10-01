using Microsoft.EntityFrameworkCore;
using TallerMecanicoAPI.Helpers;
using TallerMecanicoAPI.Data;
using TallerMecanicoAPI.Models;

namespace TallerMecanicoAPI.Repositories
{
    public class ReparacionRepositoryEF : IReparacionRepository
    {
        private readonly AppDbContext _context;
        public ReparacionRepositoryEF(AppDbContext context) => _context = context;

        private IQueryable<Reparacion> Consulta() =>
            _context.Reparaciones
                .Include(r => r.Diagnostico)
                    .ThenInclude(d => d!.Orden)
                        .ThenInclude(o => o!.Vehiculo)
                            .ThenInclude(v => v!.Cliente)
                .Include(r => r.Diagnostico)
                    .ThenInclude(d => d!.Orden)
                        .ThenInclude(o => o!.Estado)
                .Include(r => r.Diagnostico)
                    .ThenInclude(d => d!.Mecanico)
                        .ThenInclude(m => m!.Rol)
                .Include(r => r.Estado)
                .Include(r => r.AutorizadoPor)
                    .ThenInclude(u => u!.Rol);

        public async Task<List<Reparacion>> GetAll() => await Consulta().ToListAsync();

        // Consulta liviana (sin includes profundos): se usa internamente para calcular
        // costos de factura, no para exponerse directamente en un endpoint.
        public async Task<List<Reparacion>> GetByDiagnosticoIds(List<int> idsDiagnostico) =>
            await _context.Reparaciones
                .Where(r => idsDiagnostico.Contains(r.IdDiagnostico))
                .ToListAsync();

        public async Task<Reparacion?> GetById(int id) =>
            await Consulta().FirstOrDefaultAsync(r => r.IdReparacion == id);

        public async Task Add(Reparacion reparacion)
        {
            _context.Reparaciones.Add(reparacion);
            await _context.SaveChangesAsync();
        }

        public async Task Update(Reparacion reparacion)
        {
            _context.Entry(reparacion).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var reparacion = await _context.Reparaciones.FindAsync(id);
            if (reparacion == null) return;

            _context.Reparaciones.Remove(reparacion);
            await DbErrorHandler.GuardarOManejarConflicto(() => _context.SaveChangesAsync(), "reparación");
        }
    }
}
