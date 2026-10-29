using TallerMecanicoAPI.Helpers;
using TallerMecanicoAPI.Models;

namespace TallerMecanicoAPI.Repositories
{
    public interface IFacturaRepository
    {
        Task<List<Factura>> GetAll();
        Task<ResultadoPaginado<Factura>> GetPaginado(ConsultaPaginada consulta);
        Task<Factura?> GetById(int id);
        Task<Factura?> GetByOrden(int idOrden);
        Task Add(Factura factura);
    }
}
