using TallerMecanicoAPI.Helpers;
using TallerMecanicoAPI.Models;

namespace TallerMecanicoAPI.Repositories
{
    public interface IClienteRepository
    {
        Task<List<Cliente>> GetAll();
        Task<ResultadoPaginado<Cliente>> GetPaginado(ConsultaPaginada consulta);
        Task<Cliente?> GetById(int id);
        Task Add(Cliente cliente);
        Task Update(Cliente cliente);
        Task Delete(int id);
    }
}
