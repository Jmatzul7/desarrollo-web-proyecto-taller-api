using TallerMecanicoAPI.Models;

namespace TallerMecanicoAPI.Repositories
{
    public interface IDiagnosticoRepository
    {
        Task<List<Diagnostico>> GetAll();
        Task<List<Diagnostico>> GetByOrden(int idOrden);
        Task<Diagnostico?> GetById(int id);
        Task Add(Diagnostico diagnostico);
        Task Delete(int id);
    }
}
