using TallerMecanicoAPI.Models;

namespace TallerMecanicoAPI.Repositories
{
    public interface IFacturaRepository
    {
        Task<List<Factura>> GetAll();
        Task<Factura?> GetById(int id);
        Task<Factura?> GetByOrden(int idOrden);
        Task Add(Factura factura);
    }
}
