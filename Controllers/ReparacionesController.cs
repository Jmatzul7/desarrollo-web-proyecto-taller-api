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
        private readonly IOrdenTrabajoService _ordenes;

        public ReparacionesController(
            IReparacionService service,
            IReparacionRepuestoService repuestoService,
            IOrdenTrabajoService ordenes)
        {
            _service = service;
            _repuestoService = repuestoService;
            _ordenes = ordenes;
        }

        private static readonly object NoEncontrada = new { mensaje = "La reparación no fue encontrada." };

        // Un usuario sin "ordenes.ver_todas" solo accede a las reparaciones de sus órdenes asignadas.
        private async Task<bool> PuedeAccederReparacion(int idReparacion)
        {
            if (AlcanceOrdenes.IdMecanicoRestringido(User) == null) return true;
            var idOrden = await _service.GetIdOrdenDeReparacion(idReparacion);
            return idOrden == null || await AlcanceOrdenes.PuedeAccederAsync(User, _ordenes, idOrden.Value);
        }

        private int UsuarioActualId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var restringido = AlcanceOrdenes.IdMecanicoRestringido(User);
            return Ok(restringido.HasValue ? await _service.GetPorMecanico(restringido.Value) : await _service.GetAll());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var data = await _service.GetById(id);
            if (data == null) return NotFound();
            if (!await PuedeAccederReparacion(id)) return NotFound();
            return Ok(data);
        }

        [HttpPost]
        [RequierePermiso("reparaciones.crear", Roles = "Administrador,Mecanico")]
        public async Task<IActionResult> Create(ReparacionDTOCreate dto)
        {
            if (AlcanceOrdenes.IdMecanicoRestringido(User) != null)
            {
                var idOrden = await _service.GetIdOrdenDeDiagnostico(dto.IdDiagnostico);
                if (idOrden != null && !await AlcanceOrdenes.PuedeAccederAsync(User, _ordenes, idOrden.Value))
                    return NotFound(new { mensaje = "El diagnóstico indicado no existe." });
            }

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
            if (!await PuedeAccederReparacion(id)) return NotFound(NoEncontrada);

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
        public async Task<IActionResult> GetRepuestos(int id)
        {
            if (!await PuedeAccederReparacion(id)) return NotFound(NoEncontrada);
            return Ok(await _repuestoService.GetByReparacion(id));
        }

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
