using TallerMecanicoAPI.DTOs;
using TallerMecanicoAPI.Models;

namespace TallerMecanicoAPI.Services
{
    public interface IReparacionService
    {
        Task<List<Reparacion>> GetAll();
        Task<Reparacion?> GetById(int id);
        Task Add(ReparacionDTOCreate dto);
        Task Autorizar(int id, int idUsuarioAutoriza, ReparacionDTOAutorizar dto);
        Task CambiarEstado(int id, ReparacionDTOEstado dto);
        Task Delete(int id);
    }
}
