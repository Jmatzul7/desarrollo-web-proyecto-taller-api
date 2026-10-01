using TallerMecanicoAPI.DTOs;
using TallerMecanicoAPI.Models;

namespace TallerMecanicoAPI.Services
{
    public interface IDiagnosticoService
    {
        Task<List<Diagnostico>> GetAll();
        Task<List<Diagnostico>> GetByOrden(int idOrden);
        Task<Diagnostico?> GetById(int id);
        Task Add(DiagnosticoDTOCreate dto);
        Task Delete(int id);
    }
}
