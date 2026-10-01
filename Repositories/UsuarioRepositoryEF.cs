using Microsoft.EntityFrameworkCore;
using TallerMecanicoAPI.Helpers;
using TallerMecanicoAPI.Data;
using TallerMecanicoAPI.Models;

namespace TallerMecanicoAPI.Repositories
{
    public class UsuarioRepositoryEF : IUsuarioRepository
    {
        private readonly AppDbContext _context;
        public UsuarioRepositoryEF(AppDbContext context) => _context = context;

        public async Task<List<Usuario>> GetAll() =>
            await _context.Usuarios.Include(u => u.Rol).ToListAsync();

        public async Task<Usuario?> GetById(int id) =>
            await _context.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(u => u.IdUsuario == id);

        public async Task<Usuario?> GetByNombreUsuario(string nombreUsuario) =>
            await _context.Usuarios.Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario);

        public async Task Add(Usuario usuario)
        {
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
        }

        public async Task Update(Usuario usuario)
        {
            _context.Entry(usuario).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return;

            _context.Usuarios.Remove(usuario);
            await DbErrorHandler.GuardarOManejarConflicto(() => _context.SaveChangesAsync(), "usuario");
        }
    }
}
