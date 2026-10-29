using TallerMecanicoAPI.Helpers;
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

        public async Task<ResultadoPaginado<Factura>> GetPaginado(ConsultaPaginada consulta)
        {
            var q = Consulta();
            var b = consulta.TextoBusqueda;
            if (b != null)
            {
                var tieneId = int.TryParse(b.TrimStart('#'), out var id);
                q = q.Where(f => (tieneId && (f.IdFactura == id || f.IdOrden == id)) ||
                                 (f.Orden != null && f.Orden.Vehiculo != null && (f.Orden.Vehiculo.Placa.Contains(b) ||
                                     (f.Orden.Vehiculo.Cliente != null && f.Orden.Vehiculo.Cliente.Nombre.Contains(b)))));
            }
            return await ResultadoPaginado<Factura>.CrearAsync(q.OrderByDescending(f => f.FechaEmision).ThenByDescending(f => f.IdFactura), consulta);
        }

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
