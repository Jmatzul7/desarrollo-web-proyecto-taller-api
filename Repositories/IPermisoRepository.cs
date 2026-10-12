using TallerMecanicoAPI.Models;

namespace TallerMecanicoAPI.Repositories
{
    public interface IPermisoRepository
    {
        Task<List<Permiso>> GetAll();
        Task<bool> Existe(int idPermiso);

        // Permisos asignados a un usuario (nombre tal cual se guarda en el claim del JWT)
        Task<List<string>> GetNombresDeUsuario(int idUsuario);
        Task<List<Permiso>> GetDeUsuario(int idUsuario);

        Task<bool> UsuarioTienePermiso(int idUsuario, int idPermiso);
        Task Asignar(int idUsuario, int idPermiso);
        Task Quitar(int idUsuario, int idPermiso);
    }
}
