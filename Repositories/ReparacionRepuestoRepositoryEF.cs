using Microsoft.EntityFrameworkCore;
using TallerMecanicoAPI.Data;
using TallerMecanicoAPI.Models;

namespace TallerMecanicoAPI.Repositories
{
    public class ReparacionRepuestoRepositoryEF : IReparacionRepuestoRepository
    {
        private readonly AppDbContext _context;
        public ReparacionRepuestoRepositoryEF(AppDbContext context) => _context = context;

        public async Task<List<ReparacionRepuesto>> GetByReparacion(int idReparacion) =>
            await _context.ReparacionRepuestos.Include(x => x.Repuesto)
                .Where(x => x.IdReparacion == idReparacion).ToListAsync();

        public async Task<ReparacionRepuesto?> GetById(int id) =>
            await _context.ReparacionRepuestos.Include(x => x.Repuesto)
                .FirstOrDefaultAsync(x => x.IdReparacionRepuesto == id);

        public async Task Add(ReparacionRepuesto detalle)
        {
            _context.ReparacionRepuestos.Add(detalle);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var detalle = await _context.ReparacionRepuestos.FindAsync(id);
            if (detalle != null)
            {
                _context.ReparacionRepuestos.Remove(detalle);
                await _context.SaveChangesAsync();
            }
        }
    }
}
