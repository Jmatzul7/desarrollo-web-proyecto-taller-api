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

        // Permisos asignados a un rol (se suman a los que ya trae por defecto)
        Task<bool> RolExiste(int idRol);
        Task<string?> GetNombreRol(int idRol);
        Task<List<Permiso>> GetDeRol(int idRol);
        Task<bool> RolTienePermiso(int idRol, int idPermiso);
        Task AsignarARol(int idRol, int idPermiso);
        Task QuitarDeRol(int idRol, int idPermiso);

        Task<bool> UsuarioTienePermiso(int idUsuario, int idPermiso);
        Task Asignar(int idUsuario, int idPermiso);
        Task Quitar(int idUsuario, int idPermiso);
    }
}
