using TallerMecanicoAPI.Models;

namespace TallerMecanicoAPI.Repositories
{
    public interface IReparacionRepuestoRepository
    {
        Task<List<ReparacionRepuesto>> GetByReparacion(int idReparacion);
        Task<ReparacionRepuesto?> GetById(int id);
        Task Add(ReparacionRepuesto detalle);
        Task Delete(int id);
    }
}
