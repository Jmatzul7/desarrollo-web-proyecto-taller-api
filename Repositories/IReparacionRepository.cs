using TallerMecanicoAPI.Models;

namespace TallerMecanicoAPI.Repositories
{
    public interface IReparacionRepository
    {
        Task<List<Reparacion>> GetAll();
        Task<List<Reparacion>> GetByDiagnosticoIds(List<int> idsDiagnostico);
        Task<Reparacion?> GetById(int id);
        Task Add(Reparacion reparacion);
        Task Update(Reparacion reparacion);
        Task Delete(int id);
    }
}
