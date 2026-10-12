using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TallerMecanicoAPI.DTOs;
using TallerMecanicoAPI.Services;

namespace TallerMecanicoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PermisosController : ControllerBase
    {
        private readonly IPermisoService _service;
        public PermisosController(IPermisoService service) => _service = service;

        // Catálogo completo de permisos disponibles (para armar un selector en el frontend)
        [HttpGet]
        public async Task<IActionResult> Get() => Ok(await _service.GetAll());

        // Gestionar permisos de un usuario es exclusivo de Administrador: asignar permisos
        // es, en sí mismo, una acción sensible (evita que alguien se autoasigne acceso).

        [HttpGet("usuario/{idUsuario}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> GetDeUsuario(int idUsuario)
        {
            try
            {
                return Ok(await _service.GetDeUsuario(idUsuario));
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
        }

        [HttpPost("usuario/{idUsuario}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Asignar(int idUsuario, UsuarioPermisoDTOAsignar dto)
        {
            try
            {
                await _service.Asignar(idUsuario, dto);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
        }

        [HttpDelete("usuario/{idUsuario}/{idPermiso}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Quitar(int idUsuario, int idPermiso)
        {
            try
            {
                await _service.Quitar(idUsuario, idPermiso);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
        }
    }
}
