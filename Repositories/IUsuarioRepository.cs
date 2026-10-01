using TallerMecanicoAPI.Models;

namespace TallerMecanicoAPI.Repositories
{
    public interface IUsuarioRepository
    {
        Task<List<Usuario>> GetAll();
        Task<Usuario?> GetById(int id);
        Task<Usuario?> GetByNombreUsuario(string nombreUsuario);
        Task Add(Usuario usuario);
        Task Update(Usuario usuario);
        Task Delete(int id);
    }
}
