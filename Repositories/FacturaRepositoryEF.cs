using Microsoft.EntityFrameworkCore;
using TallerMecanicoAPI.Data;
using TallerMecanicoAPI.Models;

namespace TallerMecanicoAPI.Repositories
{
    public class FacturaRepositoryEF : IFacturaRepository
    {
        private readonly AppDbContext _context;
        public FacturaRepositoryEF(AppDbContext context) => _context = context;

        private IQueryable<Factura> Consulta() =>
            _context.Facturas
                .Include(f => f.Orden)
                    .ThenInclude(o => o!.Vehiculo)
                        .ThenInclude(v => v!.Cliente)
                .Include(f => f.Orden)
                    .ThenInclude(o => o!.Mecanico)
                        .ThenInclude(m => m!.Rol)
                .Include(f => f.Orden)
                    .ThenInclude(o => o!.Estado);

        public async Task<List<Factura>> GetAll() => await Consulta().ToListAsync();

        public async Task<Factura?> GetById(int id) =>
            await Consulta().FirstOrDefaultAsync(f => f.IdFactura == id);

        public async Task<Factura?> GetByOrden(int idOrden) =>
            await _context.Facturas.FirstOrDefaultAsync(f => f.IdOrden == idOrden);

        public async Task Add(Factura factura)
        {
            _context.Facturas.Add(factura);
            await _context.SaveChangesAsync();
        }
    }
}
