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
