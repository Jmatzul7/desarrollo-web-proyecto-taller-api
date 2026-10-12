using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TallerMecanicoAPI.DTOs;
using TallerMecanicoAPI.Helpers;
using TallerMecanicoAPI.Services;

namespace TallerMecanicoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReparacionesController : ControllerBase
    {
        private readonly IReparacionService _service;
        private readonly IReparacionRepuestoService _repuestoService;

        public ReparacionesController(IReparacionService service, IReparacionRepuestoService repuestoService)
        {
            _service = service;
            _repuestoService = repuestoService;
        }

        private int UsuarioActualId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        public async Task<IActionResult> Get() => Ok(await _service.GetAll());

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var data = await _service.GetById(id);
            return data == null ? NotFound() : Ok(data);
        }

        [HttpPost]
        [RequierePermiso("reparaciones.crear", Roles = "Administrador,Mecanico")]
        public async Task<IActionResult> Create(ReparacionDTOCreate dto)
        {
            try
            {
                await _service.Add(dto);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        // Autoriza o rechaza una reparación pendiente
        [HttpPatch("{id}/autorizar")]
        [RequierePermiso("reparaciones.autorizar", Roles = "Administrador")]
        public async Task<IActionResult> Autorizar(int id, ReparacionDTOAutorizar dto)
        {
            try
            {
                await _service.Autorizar(id, UsuarioActualId, dto);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        // Cambia el estado (p.ej. iniciar o finalizar), respetando dependencias entre reparaciones
        [HttpPatch("{id}/estado")]
        [RequierePermiso("reparaciones.cambiar_estado", Roles = "Administrador,Mecanico")]
        public async Task<IActionResult> CambiarEstado(int id, ReparacionDTOEstado dto)
        {
            try
            {
                await _service.CambiarEstado(id, dto);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [RequierePermiso("reparaciones.eliminar", Roles = "Administrador")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.Delete(id);
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

        // ---- Repuestos usados en la reparación ----

        [HttpGet("{id}/repuestos")]
        public async Task<IActionResult> GetRepuestos(int id) => Ok(await _repuestoService.GetByReparacion(id));

        [HttpPost("{id}/repuestos")]
        [RequierePermiso("reparaciones.asignar_repuesto", Roles = "Administrador,Bodeguero")]
        public async Task<IActionResult> AddRepuesto(int id, ReparacionRepuestoDTOCreate dto)
        {
            try
            {
                await _repuestoService.Add(id, UsuarioActualId, dto);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpDelete("repuestos/{idDetalle}")]
        [RequierePermiso("reparaciones.quitar_repuesto", Roles = "Administrador,Bodeguero")]
        public async Task<IActionResult> DeleteRepuesto(int idDetalle)
        {
            try
            {
                await _repuestoService.Delete(idDetalle);
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
    }
}
