using Microsoft.EntityFrameworkCore;
using TallerMecanicoAPI.Data;

namespace TallerMecanicoAPI.Repositories
{
    public class CatalogoRepositoryEF : ICatalogoRepository
    {
        private readonly AppDbContext _context;
        public CatalogoRepositoryEF(AppDbContext context) => _context = context;

        public async Task<bool> RolExiste(int idRol) =>
            await _context.Roles.AnyAsync(r => r.IdRol == idRol);

        public async Task<bool> EstadoOrdenExiste(int idEstadoOrden) =>
            await _context.EstadosOrden.AnyAsync(e => e.IdEstadoOrden == idEstadoOrden);

        public async Task<bool> EstadoReparacionExiste(int idEstadoReparacion) =>
            await _context.EstadosReparacion.AnyAsync(e => e.IdEstadoReparacion == idEstadoReparacion);
    }
}
