using Microsoft.EntityFrameworkCore;
using TallerMecanicoAPI.Data;
using TallerMecanicoAPI.Models;

namespace TallerMecanicoAPI.Repositories
{
    public class PermisoRepositoryEF : IPermisoRepository
    {
        private readonly AppDbContext _context;
        public PermisoRepositoryEF(AppDbContext context) => _context = context;

        public async Task<List<Permiso>> GetAll() =>
            await _context.Permisos.ToListAsync();

        public async Task<bool> Existe(int idPermiso) =>
            await _context.Permisos.AnyAsync(p => p.IdPermiso == idPermiso);

        public async Task<List<string>> GetNombresDeUsuario(int idUsuario) =>
            await _context.UsuarioPermisos
                .Where(up => up.IdUsuario == idUsuario)
                .Join(_context.Permisos, up => up.IdPermiso, p => p.IdPermiso, (up, p) => p.Nombre)
                .ToListAsync();

        public async Task<List<Permiso>> GetDeUsuario(int idUsuario) =>
            await _context.UsuarioPermisos
                .Where(up => up.IdUsuario == idUsuario)
                .Join(_context.Permisos, up => up.IdPermiso, p => p.IdPermiso, (up, p) => p)
                .ToListAsync();

        public async Task<bool> UsuarioTienePermiso(int idUsuario, int idPermiso) =>
            await _context.UsuarioPermisos
                .AnyAsync(up => up.IdUsuario == idUsuario && up.IdPermiso == idPermiso);

        public async Task Asignar(int idUsuario, int idPermiso)
        {
            _context.UsuarioPermisos.Add(new UsuarioPermiso { IdUsuario = idUsuario, IdPermiso = idPermiso });
            await _context.SaveChangesAsync();
        }

        public async Task Quitar(int idUsuario, int idPermiso)
        {
            var registro = await _context.UsuarioPermisos
                .FirstOrDefaultAsync(up => up.IdUsuario == idUsuario && up.IdPermiso == idPermiso);

            if (registro != null)
            {
                _context.UsuarioPermisos.Remove(registro);
                await _context.SaveChangesAsync();
            }
        }
    }
}
