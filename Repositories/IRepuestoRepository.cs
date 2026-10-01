using TallerMecanicoAPI.Models;

namespace TallerMecanicoAPI.Repositories
{
    public interface IRepuestoRepository
    {
        Task<List<Repuesto>> GetAll();
        Task<Repuesto?> GetById(int id);
        Task<Repuesto?> GetByCodigo(string codigo);
        Task Add(Repuesto repuesto);
        Task Update(Repuesto repuesto);
        Task Delete(int id);
    }
}
