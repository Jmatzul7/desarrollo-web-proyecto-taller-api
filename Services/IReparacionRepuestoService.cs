using TallerMecanicoAPI.DTOs;
using TallerMecanicoAPI.Models;

namespace TallerMecanicoAPI.Services
{
    public interface IReparacionRepuestoService
    {
        Task<List<ReparacionRepuesto>> GetByReparacion(int idReparacion);
        Task Add(int idReparacion, int idUsuarioAutoriza, ReparacionRepuestoDTOCreate dto);
        Task Delete(int id);
    }
}
