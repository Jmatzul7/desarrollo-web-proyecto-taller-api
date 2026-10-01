using Microsoft.AspNetCore.Authorization;
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
