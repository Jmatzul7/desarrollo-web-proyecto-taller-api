using TallerMecanicoAPI.Helpers;
using TallerMecanicoAPI.DTOs;
using TallerMecanicoAPI.Models;

namespace TallerMecanicoAPI.Services
{
    public interface IVehiculoService
    {
        Task<List<Vehiculo>> GetAll();
        Task<ResultadoPaginado<Vehiculo>> GetPaginado(ConsultaPaginada consulta);
        Task<Vehiculo?> GetById(int id);
        Task Add(VehiculoDTOCreate dto);
        Task Update(int id, VehiculoDTOUpdate dto);
        Task Delete(int id);
    }
}
