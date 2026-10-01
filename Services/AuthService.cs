using TallerMecanicoAPI.DTOs;
using TallerMecanicoAPI.Helpers;
using TallerMecanicoAPI.Repositories;

namespace TallerMecanicoAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUsuarioRepository _usuarioRepo;
        private readonly JwtTokenGenerator _jwtGenerator;

        public AuthService(IUsuarioRepository usuarioRepo, JwtTokenGenerator jwtGenerator)
        {
            _usuarioRepo = usuarioRepo;
            _jwtGenerator = jwtGenerator;
        }

        public async Task<LoginResponseDTO?> Login(LoginDTO dto)
        {
            var usuario = await _usuarioRepo.GetByNombreUsuario(dto.NombreUsuario);

            if (usuario == null || !usuario.Activo)
                return null;

            if (!PasswordHasher.Verify(dto.Password, usuario.PasswordHash))
                return null;

            var nombreRol = usuario.Rol?.Nombre ?? "Sin Rol";
            var (token, expira) = _jwtGenerator.GenerarToken(usuario, nombreRol);

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
