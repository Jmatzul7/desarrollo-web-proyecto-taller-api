using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TallerMecanicoAPI.Data;

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

        [HttpGet("estados-reparacion")]
        public async Task<IActionResult> GetEstadosReparacion() => Ok(await _context.EstadosReparacion.ToListAsync());
    }
}
