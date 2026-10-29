using TallerMecanicoAPI.Helpers;
using TallerMecanicoAPI.DTOs;
using TallerMecanicoAPI.Models;

namespace TallerMecanicoAPI.Services
{
    public interface IOrdenTrabajoService
    {
        Task<List<OrdenTrabajo>> GetAll();
        Task<List<OrdenTrabajo>> GetPorMecanico(int idMecanico);
        Task<ResultadoPaginado<OrdenTrabajo>> GetPaginado(ConsultaPaginada consulta);
        Task<OrdenTrabajo?> GetById(int id);
        Task Add(OrdenTrabajoDTOCreate dto);
        Task CambiarEstado(int id, OrdenTrabajoDTOEstado dto);
        Task Reasignar(int id, OrdenTrabajoDTOReasignar dto);
        Task Delete(int id);
    }
}
