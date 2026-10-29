using TallerMecanicoAPI.Helpers;
using TallerMecanicoAPI.DTOs;
using TallerMecanicoAPI.Models;

namespace TallerMecanicoAPI.Services
{
    public interface IReparacionService
    {
        Task<List<Reparacion>> GetAll();

        // Alcance por orden: reparaciones de las órdenes de un mecánico y a qué orden pertenece una reparación.
        Task<List<Reparacion>> GetPorMecanico(int idMecanico);
        Task<int?> GetIdOrdenDeReparacion(int idReparacion);
        Task<int?> GetIdOrdenDeDiagnostico(int idDiagnostico);
        Task<Reparacion?> GetById(int id);
        Task Add(ReparacionDTOCreate dto);
        Task Autorizar(int id, int idUsuarioAutoriza, ReparacionDTOAutorizar dto);
        Task CambiarEstado(int id, ReparacionDTOEstado dto);
        Task Delete(int id);
    }
}
