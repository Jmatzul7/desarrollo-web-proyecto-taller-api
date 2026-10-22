using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TallerMecanicoAPI.Data;
using TallerMecanicoAPI.Helpers;

namespace TallerMecanicoAPI.Controllers
{
    // Catálogos de solo lectura usados como listas desplegables por el frontend
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CatalogosController : ControllerBase
    {
        private readonly AppDbContext _context;
        public CatalogosController(AppDbContext context) => _context = context;

        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles() => Ok(await _context.Roles.ToListAsync());

        [HttpGet("estados-orden")]
        public async Task<IActionResult> GetEstadosOrden() => Ok(await _context.EstadosOrden.ToListAsync());

        // Lista mínima de mecánicos activos para asignar una orden de trabajo.
        // Se separa de GET /api/usuarios (solo Administrador) para no exponer correos ni
        // datos de otras cuentas: solo id, usuario y nombre.
        [HttpGet("mecanicos")]
        [RequierePermiso("usuarios.ver_mecanicos", Roles = "Administrador,Recepcionista,Mecanico")]
        public async Task<IActionResult> GetMecanicos() => Ok(await _context.Usuarios
            .Where(u => u.Activo && u.Rol != null && u.Rol.Nombre == "Mecanico")
            .OrderBy(u => u.NombreCompleto)
            .Select(u => new { u.IdUsuario, u.NombreUsuario, u.NombreCompleto })
            .ToListAsync());

        [HttpGet("estados-reparacion")]
        public async Task<IActionResult> GetEstadosReparacion() => Ok(await _context.EstadosReparacion.ToListAsync());
    }
}
