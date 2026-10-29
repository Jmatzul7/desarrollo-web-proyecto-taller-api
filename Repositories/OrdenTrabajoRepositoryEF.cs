using Microsoft.EntityFrameworkCore;
using TallerMecanicoAPI.Helpers;
using TallerMecanicoAPI.Data;
using TallerMecanicoAPI.Models;

namespace TallerMecanicoAPI.Repositories
{
    public class OrdenTrabajoRepositoryEF : IOrdenTrabajoRepository
    {
        private readonly AppDbContext _context;
        public OrdenTrabajoRepositoryEF(AppDbContext context) => _context = context;

        private IQueryable<OrdenTrabajo> Consulta() =>
            _context.OrdenesTrabajo
                .Include(o => o.Vehiculo)
                    .ThenInclude(v => v!.Cliente)
                .Include(o => o.Mecanico)
                    .ThenInclude(m => m!.Rol)
                .Include(o => o.Estado);

        public async Task<List<OrdenTrabajo>> GetAll() => await Consulta().ToListAsync();

        public async Task<List<OrdenTrabajo>> GetPorMecanico(int idMecanico) =>
            await Consulta().Where(o => o.IdMecanico == idMecanico).ToListAsync();

        public async Task<ResultadoPaginado<OrdenTrabajo>> GetPaginado(ConsultaPaginada consulta)
        {
            var q = Consulta();
            var b = consulta.TextoBusqueda;
            if (b != null)
            {
                var tieneId = int.TryParse(b.TrimStart('#'), out var id);
                q = q.Where(o => (tieneId && o.IdOrden == id) ||
                                 (o.Observaciones != null && o.Observaciones.Contains(b)) ||
                                 (o.Vehiculo != null && (o.Vehiculo.Placa.Contains(b) ||
                                     (o.Vehiculo.Cliente != null && o.Vehiculo.Cliente.Nombre.Contains(b)))) ||
                                 (o.Mecanico != null && o.Mecanico.NombreCompleto.Contains(b)));
            }
            if (consulta.Estado.HasValue) q = q.Where(o => o.IdEstadoOrden == consulta.Estado.Value);
            if (consulta.IdMecanico.HasValue) q = q.Where(o => o.IdMecanico == consulta.IdMecanico.Value);
            return await ResultadoPaginado<OrdenTrabajo>.CrearAsync(q.OrderByDescending(o => o.IdOrden), consulta);
        }

        public async Task<OrdenTrabajo?> GetById(int id) =>
            await Consulta().FirstOrDefaultAsync(o => o.IdOrden == id);

        public async Task Add(OrdenTrabajo orden)
        {
            _context.OrdenesTrabajo.Add(orden);
            await _context.SaveChangesAsync();
        }

        public async Task Update(OrdenTrabajo orden)
        {
            _context.Entry(orden).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var orden = await _context.OrdenesTrabajo.FindAsync(id);
            if (orden == null) return;

            _context.OrdenesTrabajo.Remove(orden);
            await DbErrorHandler.GuardarOManejarConflicto(() => _context.SaveChangesAsync(), "orden de trabajo");
        }
    }
}
