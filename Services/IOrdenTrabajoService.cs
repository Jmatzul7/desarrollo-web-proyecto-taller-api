using TallerMecanicoAPI.DTOs;
using TallerMecanicoAPI.Models;

namespace TallerMecanicoAPI.Services
{
    public interface IOrdenTrabajoService
    {
        Task<List<OrdenTrabajo>> GetAll();
        Task<OrdenTrabajo?> GetById(int id);
        Task Add(OrdenTrabajoDTOCreate dto);
        Task CambiarEstado(int id, OrdenTrabajoDTOEstado dto);
        Task Delete(int id);
    }
}
