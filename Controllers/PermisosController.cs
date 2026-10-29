using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using TallerMecanicoAPI.DTOs;
using TallerMecanicoAPI.Helpers;
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

        // ---- Permisos por ROL ----
        // Se SUMAN a lo que el rol ya trae por defecto (la lista fija de roles de cada acción):
        // todos los usuarios de ese rol los heredan, sin asignarlos uno por uno.

        [HttpGet("rol/{idRol}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> GetDeRol(int idRol)
        {
            try
            {
                return Ok(await _service.GetDeRol(idRol));
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
        }

        [HttpPost("rol/{idRol}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> AsignarARol(int idRol, UsuarioPermisoDTOAsignar dto)
        {
            try
            {
                await _service.AsignarARol(idRol, dto);
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

        [HttpDelete("rol/{idRol}/{idPermiso}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> QuitarDeRol(int idRol, int idPermiso)
        {
            try
            {
                await _service.QuitarDeRol(idRol, idPermiso);
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

        // Qué roles tienen cada permiso POR DEFECTO (lo declarado en [RequierePermiso(..., Roles = "...")]
        // de cada acción). Se lee de los propios controladores, así nunca queda desactualizado.
        [HttpGet("por-defecto")]
        [Authorize(Roles = "Administrador")]
        public IActionResult PorDefecto()
        {
            var atributos = typeof(PermisosController).Assembly.GetTypes()
                .Where(t => typeof(ControllerBase).IsAssignableFrom(t))
                .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                .Select(m => m.GetCustomAttribute<RequierePermisoAttribute>())
                .Where(a => a != null)
                .Select(a => a!);

            // "ordenes.ver_todas" no es de un endpoint: define el alcance de la vista de órdenes.
            var resultado = atributos
                .GroupBy(a => a.Permiso)
                .Select(g => new
                {
                    permiso = g.Key,
                    roles = g.SelectMany(a => a.Roles.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                             .Distinct().OrderBy(r => r).ToList()
                })
                .ToList();

            resultado.Add(new
            {
                permiso = AlcanceOrdenes.Permiso,
                roles = AlcanceOrdenes.RolesPorDefecto.Split(',').OrderBy(r => r).ToList()
            });

            return Ok(resultado.OrderBy(x => x.permiso).ToList());
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
