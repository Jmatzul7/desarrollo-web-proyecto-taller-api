using TallerMecanicoAPI.DTOs;
using TallerMecanicoAPI.Models;

namespace TallerMecanicoAPI.Services
{
    public interface IClienteService
    {
        Task<List<Cliente>> GetAll();
        Task<Cliente?> GetById(int id);
        Task Add(ClienteDTOCreate dto);
        Task Update(int id, ClienteDTOUpdate dto);
        Task Delete(int id);
    }
}
