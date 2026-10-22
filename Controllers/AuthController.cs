using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using TallerMecanicoAPI.DTOs;
using TallerMecanicoAPI.Services;

namespace TallerMecanicoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUsuarioService _usuarioService;

        public AuthController(IAuthService authService, IUsuarioService usuarioService)
        {
            _authService = authService;
            _usuarioService = usuarioService;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO dto)
        {
            var respuesta = await _authService.Login(dto);

            if (respuesta == null)
                return Unauthorized(new { mensaje = "Usuario o contraseña incorrectos." });

            return Ok(respuesta);
        }

        // Reemite el token con el rol y los permisos vigentes en la base de datos.
        // Así, cuando un Administrador asigna o quita un permiso, el usuario afectado
        // lo recibe en su siguiente refresco sin tener que volver a iniciar sesión.
        [Authorize]
        [HttpPost("refrescar")]
        public async Task<IActionResult> Refrescar()
        {
            var claimId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(claimId, out var idUsuario))
                return Unauthorized(new { mensaje = "Token inválido." });

            var respuesta = await _authService.Refrescar(idUsuario);

            if (respuesta == null)
                return Unauthorized(new { mensaje = "La cuenta ya no está disponible." });

            return Ok(respuesta);
        }

        // Autoregistro: cualquiera puede crear su cuenta, pero queda inactiva
        // (Activo = false) hasta que un Administrador la active. No permite
        // autoasignarse el rol Administrador.
        [AllowAnonymous]
        [HttpPost("registro")]
        public async Task<IActionResult> Registro(UsuarioDTOCreate dto)
        {
            try
            {
                await _usuarioService.RegistrarPublico(dto);
                return Ok(new
                {
                    mensaje = "Registro exitoso. Un administrador debe activar tu cuenta antes de que puedas iniciar sesión."
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }
    }
}
