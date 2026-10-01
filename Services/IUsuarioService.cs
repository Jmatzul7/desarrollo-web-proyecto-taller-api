using TallerMecanicoAPI.DTOs;

namespace TallerMecanicoAPI.Services
{
    public interface IUsuarioService
    {
        Task<List<UsuarioDTOResponse>> GetAll();
        Task<UsuarioDTOResponse?> GetById(int id);
        Task Add(UsuarioDTOCreate dto);

        // Autoregistro público: no permite rol Administrador y crea la cuenta inactiva
        // (un Administrador debe activarla vía Update antes de poder iniciar sesión).
        Task RegistrarPublico(UsuarioDTOCreate dto);

        Task Update(int id, UsuarioDTOUpdate dto);
        Task Delete(int id);
    }
}
