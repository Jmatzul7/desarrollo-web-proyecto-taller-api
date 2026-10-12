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
    }
}
