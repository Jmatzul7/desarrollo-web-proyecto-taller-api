using TallerMecanicoAPI.Models;

namespace TallerMecanicoAPI.Repositories
{
    public interface IVehiculoRepository
    {
        Task<List<Vehiculo>> GetAll();
        Task<Vehiculo?> GetById(int id);
        Task<Vehiculo?> GetByPlaca(string placa);
        Task Add(Vehiculo vehiculo);
        Task Update(Vehiculo vehiculo);
        Task Delete(int id);
    }
}
