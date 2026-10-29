using Microsoft.EntityFrameworkCore;
using TallerMecanicoAPI.Helpers;
using TallerMecanicoAPI.Data;
using TallerMecanicoAPI.Models;

namespace TallerMecanicoAPI.Repositories
{
    public class VehiculoRepositoryEF : IVehiculoRepository
    {
        private readonly AppDbContext _context;
        public VehiculoRepositoryEF(AppDbContext context) => _context = context;

        public async Task<List<Vehiculo>> GetAll() =>
            await _context.Vehiculos.Include(v => v.Cliente).ToListAsync();

        public async Task<ResultadoPaginado<Vehiculo>> GetPaginado(ConsultaPaginada consulta)
        {
            var q = _context.Vehiculos.Include(v => v.Cliente).AsQueryable();
            var b = consulta.TextoBusqueda;
            if (b != null)
                q = q.Where(v => v.Placa.Contains(b) || v.Marca.Contains(b) || v.Modelo.Contains(b) ||
                                 (v.Cliente != null && v.Cliente.Nombre.Contains(b)));
            if (consulta.Activo.HasValue) q = q.Where(v => v.Activo == consulta.Activo.Value);
            return await ResultadoPaginado<Vehiculo>.CrearAsync(q.OrderBy(v => v.Placa).ThenBy(v => v.IdVehiculo), consulta);
        }

        public async Task<Vehiculo?> GetById(int id) =>
            await _context.Vehiculos.Include(v => v.Cliente).FirstOrDefaultAsync(v => v.IdVehiculo == id);

        public async Task<Vehiculo?> GetByPlaca(string placa) =>
            await _context.Vehiculos.FirstOrDefaultAsync(v => v.Placa == placa);

        public async Task Add(Vehiculo vehiculo)
        {
            _context.Vehiculos.Add(vehiculo);
            await _context.SaveChangesAsync();
        }

        public async Task Update(Vehiculo vehiculo)
        {
            _context.Entry(vehiculo).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var vehiculo = await _context.Vehiculos.FindAsync(id);
            if (vehiculo == null) return;

            _context.Vehiculos.Remove(vehiculo);
            await DbErrorHandler.GuardarOManejarConflicto(() => _context.SaveChangesAsync(), "vehículo");
        }
    }
}
