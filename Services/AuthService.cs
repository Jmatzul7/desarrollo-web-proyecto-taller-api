using TallerMecanicoAPI.DTOs;
using TallerMecanicoAPI.Helpers;
using TallerMecanicoAPI.Models;
using TallerMecanicoAPI.Repositories;

namespace TallerMecanicoAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUsuarioRepository _usuarioRepo;
        private readonly IPermisoRepository _permisoRepo;
        private readonly JwtTokenGenerator _jwtGenerator;

        public AuthService(IUsuarioRepository usuarioRepo, IPermisoRepository permisoRepo, JwtTokenGenerator jwtGenerator)
        {
            _usuarioRepo = usuarioRepo;
            _permisoRepo = permisoRepo;
            _jwtGenerator = jwtGenerator;
        }

        public async Task<LoginResponseDTO?> Login(LoginDTO dto)
        {
            var usuario = await _usuarioRepo.GetByNombreUsuario(dto.NombreUsuario);

            if (usuario == null || !usuario.Activo)
                return null;

            if (!PasswordHasher.Verify(dto.Password, usuario.PasswordHash))
                return null;

            return await EmitirSesion(usuario);
        }

        // Reemite el JWT con el rol y los permisos que el usuario tiene AHORA en la base
        // de datos. El frontend lo llama periódicamente para que un cambio de permisos
        // (o de rol) se refleje sin obligar al usuario a cerrar sesión e ingresar de nuevo.
        // Si la cuenta ya no existe o fue desactivada devuelve null (el controlador responde 401).
        public async Task<LoginResponseDTO?> Refrescar(int idUsuario)
        {
            var usuario = await _usuarioRepo.GetById(idUsuario);

            if (usuario == null || !usuario.Activo)
                return null;

            return await EmitirSesion(usuario);
        }

        private async Task<LoginResponseDTO> EmitirSesion(Usuario usuario)
        {
            var nombreRol = usuario.Rol?.Nombre ?? "Sin Rol";
            var permisos = await _permisoRepo.GetNombresDeUsuario(usuario.IdUsuario);
            var (token, expira) = _jwtGenerator.GenerarToken(usuario, nombreRol, permisos);

            return new LoginResponseDTO
            {
                Token = token,
                Expira = expira,
                IdUsuario = usuario.IdUsuario,
                NombreUsuario = usuario.NombreUsuario,
                NombreCompleto = usuario.NombreCompleto,
                Rol = nombreRol
            };
        }
    }
}
