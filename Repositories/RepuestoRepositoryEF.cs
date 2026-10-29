using Microsoft.EntityFrameworkCore;
using TallerMecanicoAPI.Helpers;
using TallerMecanicoAPI.Data;
using TallerMecanicoAPI.Models;

namespace TallerMecanicoAPI.Repositories
{
    public class RepuestoRepositoryEF : IRepuestoRepository
    {
        private readonly AppDbContext _context;
        public RepuestoRepositoryEF(AppDbContext context) => _context = context;

        public async Task<List<Repuesto>> GetAll() => await _context.Repuestos.ToListAsync();

        public async Task<ResultadoPaginado<Repuesto>> GetPaginado(ConsultaPaginada consulta)
        {
            var q = _context.Repuestos.AsQueryable();
            var b = consulta.TextoBusqueda;
            if (b != null) q = q.Where(r => r.Codigo.Contains(b) || r.Nombre.Contains(b));
            if (consulta.Activo.HasValue) q = q.Where(r => r.Activo == consulta.Activo.Value);
            if (consulta.StockBajo == true) q = q.Where(r => r.Activo && r.Existencia <= 5);
            return await ResultadoPaginado<Repuesto>.CrearAsync(q.OrderBy(r => r.Nombre).ThenBy(r => r.IdRepuesto), consulta);
        }

        public async Task<Repuesto?> GetById(int id) => await _context.Repuestos.FindAsync(id);

        public async Task<Repuesto?> GetByCodigo(string codigo) =>
            await _context.Repuestos.FirstOrDefaultAsync(r => r.Codigo == codigo);

        public async Task Add(Repuesto repuesto)
        {
            _context.Repuestos.Add(repuesto);
            await _context.SaveChangesAsync();
        }

        public async Task Update(Repuesto repuesto)
        {
            _context.Entry(repuesto).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var repuesto = await _context.Repuestos.FindAsync(id);
            if (repuesto == null) return;

            _context.Repuestos.Remove(repuesto);
            await DbErrorHandler.GuardarOManejarConflicto(() => _context.SaveChangesAsync(), "repuesto");
        }
    }
}
