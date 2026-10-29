using TallerMecanicoAPI.Helpers;
using TallerMecanicoAPI.DTOs;
using TallerMecanicoAPI.Models;

namespace TallerMecanicoAPI.Services
{
    public interface IRepuestoService
    {
        Task<List<Repuesto>> GetAll();
        Task<ResultadoPaginado<Repuesto>> GetPaginado(ConsultaPaginada consulta);
        Task<Repuesto?> GetById(int id);
        Task Add(RepuestoDTOCreate dto);
        Task Update(int id, RepuestoDTOUpdate dto);
        Task EntradaStock(int id, RepuestoDTOAjusteStock dto);
        Task Delete(int id);
    }
}
