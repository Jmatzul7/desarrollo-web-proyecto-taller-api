using TallerMecanicoAPI.Models;

namespace TallerMecanicoAPI.Repositories
{
    public interface IOrdenTrabajoRepository
    {
        Task<List<OrdenTrabajo>> GetAll();
        Task<OrdenTrabajo?> GetById(int id);
        Task Add(OrdenTrabajo orden);
        Task Update(OrdenTrabajo orden);
        Task Delete(int id);
    }
}
