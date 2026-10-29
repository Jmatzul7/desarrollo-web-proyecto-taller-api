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

        // Permisos EFECTIVOS del usuario: los suyos propios + los de su rol, sin repetir.
        // Es lo que viaja como claims "permiso" en el JWT, así que todo lo demás
        // (RequierePermiso, frontend) los trata igual venga de donde venga.
        public async Task<List<string>> GetNombresDeUsuario(int idUsuario)
        {
            var propios = await _context.UsuarioPermisos
                .Where(up => up.IdUsuario == idUsuario)
                .Join(_context.Permisos, up => up.IdPermiso, p => p.IdPermiso, (up, p) => p.Nombre)
                .ToListAsync();

            var idRol = await _context.Usuarios
                .Where(u => u.IdUsuario == idUsuario)
                .Select(u => u.IdRol)
                .FirstOrDefaultAsync();

            var delRol = await _context.RolPermisos
                .Where(rp => rp.IdRol == idRol)
                .Join(_context.Permisos, rp => rp.IdPermiso, p => p.IdPermiso, (rp, p) => p.Nombre)
                .ToListAsync();

            return propios.Union(delRol).ToList();
        }

        public async Task<bool> RolExiste(int idRol) =>
            await _context.Roles.AnyAsync(r => r.IdRol == idRol);

        public async Task<string?> GetNombreRol(int idRol) =>
            await _context.Roles.Where(r => r.IdRol == idRol).Select(r => r.Nombre).FirstOrDefaultAsync();

        public async Task<List<Permiso>> GetDeRol(int idRol) =>
            await _context.RolPermisos
                .Where(rp => rp.IdRol == idRol)
                .Join(_context.Permisos, rp => rp.IdPermiso, p => p.IdPermiso, (rp, p) => p)
                .ToListAsync();

        public async Task<bool> RolTienePermiso(int idRol, int idPermiso) =>
            await _context.RolPermisos.AnyAsync(rp => rp.IdRol == idRol && rp.IdPermiso == idPermiso);

        public async Task AsignarARol(int idRol, int idPermiso)
        {
            _context.RolPermisos.Add(new RolPermiso { IdRol = idRol, IdPermiso = idPermiso });
            await _context.SaveChangesAsync();
        }

        public async Task QuitarDeRol(int idRol, int idPermiso)
        {
            var registro = await _context.RolPermisos
                .FirstOrDefaultAsync(rp => rp.IdRol == idRol && rp.IdPermiso == idPermiso);

            if (registro != null)
            {
                _context.RolPermisos.Remove(registro);
                await _context.SaveChangesAsync();
            }
        }

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
