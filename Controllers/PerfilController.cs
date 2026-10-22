using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TallerMecanicoAPI.DTOs;
using TallerMecanicoAPI.Services;

namespace TallerMecanicoAPI.Controllers
{
    // Perfil del usuario autenticado. Cualquier usuario puede ver y editar SU PROPIA cuenta;
    // el id siempre sale del token (nunca del cliente), así que nadie puede tocar otra cuenta.
    // No se puede cambiar aquí el rol ni el estado: eso sigue siendo exclusivo de Administrador.
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PerfilController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;
        private readonly IAuthService _authService;

        public PerfilController(IUsuarioService usuarioService, IAuthService authService)
        {
            _usuarioService = usuarioService;
            _authService = authService;
        }

        private bool TryGetIdUsuario(out int idUsuario) =>
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out idUsuario);

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            if (!TryGetIdUsuario(out var idUsuario))
                return Unauthorized(new { mensaje = "Token inválido." });

            var perfil = await _usuarioService.GetPerfil(idUsuario);
            return perfil == null ? NotFound(new { mensaje = "El usuario no fue encontrado." }) : Ok(perfil);
        }

        // Para validar en tiempo real mientras el usuario escribe. El propio nombre de usuario
        // actual cuenta como disponible.
        [HttpGet("nombre-usuario-disponible")]
        public async Task<IActionResult> NombreUsuarioDisponible([FromQuery] string nombreUsuario)
        {
            if (!TryGetIdUsuario(out var idUsuario))
                return Unauthorized(new { mensaje = "Token inválido." });

            return Ok(new { disponible = await _usuarioService.NombreUsuarioDisponible(nombreUsuario, idUsuario) });
        }

        // Actualiza nombre de usuario, nombre completo y correo. Devuelve una sesión nueva
        // (token) porque el nombre de usuario y el nombre completo viajan dentro del JWT.
        [HttpPut]
        public async Task<IActionResult> Actualizar(PerfilDTOUpdate dto)
        {
            if (!TryGetIdUsuario(out var idUsuario))
                return Unauthorized(new { mensaje = "Token inválido." });

            try
            {
                await _usuarioService.ActualizarPerfil(idUsuario, dto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }

            var sesion = await _authService.Refrescar(idUsuario);
            return sesion == null ? Unauthorized(new { mensaje = "La cuenta ya no está disponible." }) : Ok(sesion);
        }

        // Cambia SOLO la contraseña: es un endpoint aparte para que no intervenga ninguna
        // validación del nombre de usuario.
        [HttpPut("password")]
        public async Task<IActionResult> CambiarPassword(CambiarPasswordDTO dto)
        {
            if (!TryGetIdUsuario(out var idUsuario))
                return Unauthorized(new { mensaje = "Token inválido." });

            try
            {
                await _usuarioService.CambiarPassword(idUsuario, dto);
                return Ok(new { mensaje = "Contraseña actualizada correctamente." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }
    }
}
