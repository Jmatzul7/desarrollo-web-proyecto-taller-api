using TallerMecanicoAPI.DTOs;
using TallerMecanicoAPI.Models;

namespace TallerMecanicoAPI.Services
{
    public interface IPermisoService
    {
        Task<List<Permiso>> GetAll();
        Task<List<Permiso>> GetDeUsuario(int idUsuario);
        Task Asignar(int idUsuario, UsuarioPermisoDTOAsignar dto);
        Task Quitar(int idUsuario, int idPermiso);

        // ---- Permisos de un rol (los heredan todos los usuarios de ese rol) ----
        Task<List<Permiso>> GetDeRol(int idRol);
        Task AsignarARol(int idRol, UsuarioPermisoDTOAsignar dto);
        Task QuitarDeRol(int idRol, int idPermiso);
    }
}
