using TallerMecanicoAPI.Helpers;
using TallerMecanicoAPI.Models;

namespace TallerMecanicoAPI.Repositories
{
    public interface IRepuestoRepository
    {
        Task<List<Repuesto>> GetAll();
        Task<ResultadoPaginado<Repuesto>> GetPaginado(ConsultaPaginada consulta);
        Task<Repuesto?> GetById(int id);
        Task<Repuesto?> GetByCodigo(string codigo);
        Task Add(Repuesto repuesto);
        Task Update(Repuesto repuesto);
        Task Delete(int id);
    }
}
