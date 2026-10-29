using TallerMecanicoAPI.Helpers;
using TallerMecanicoAPI.DTOs;

namespace TallerMecanicoAPI.Services
{
    public interface IUsuarioService
    {
        Task<List<UsuarioDTOResponse>> GetAll();
        Task<ResultadoPaginado<UsuarioDTOResponse>> GetPaginado(ConsultaPaginada consulta);
        Task<UsuarioDTOResponse?> GetById(int id);
        Task Add(UsuarioDTOCreate dto);

        // Autoregistro público: no permite rol Administrador y crea la cuenta inactiva
        // (un Administrador debe activarla vía Update antes de poder iniciar sesión).
        Task RegistrarPublico(UsuarioDTOCreate dto);

        Task Update(int id, UsuarioDTOUpdate dto);

        // ---- Perfil propio (el usuario autenticado sobre su propia cuenta) ----
        Task<UsuarioDTOResponse?> GetPerfil(int idUsuario);

        // true si nadie más (aparte del propio usuario) usa ese nombre de usuario.
        Task<bool> NombreUsuarioDisponible(string nombreUsuario, int idUsuarioActual);

        // Actualiza nombre de usuario, nombre completo y correo. Valida que el nombre de
        // usuario no esté en uso por OTRA cuenta.
        Task ActualizarPerfil(int idUsuario, PerfilDTOUpdate dto);

        // Cambia solo la contraseña. NO valida ni toca el nombre de usuario.
        Task CambiarPassword(int idUsuario, CambiarPasswordDTO dto);
        Task Delete(int id);
    }
}
