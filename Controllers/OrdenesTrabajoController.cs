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
    public class OrdenesTrabajoController : ControllerBase
    {
        private readonly IOrdenTrabajoService _service;
        public OrdenesTrabajoController(IOrdenTrabajoService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] ConsultaPaginada consulta)
        {
            // Sin "ordenes.ver_todas" (p. ej. un Mecánico) solo se listan las órdenes asignadas al usuario.
            var restringido = AlcanceOrdenes.IdMecanicoRestringido(User);
            if (restringido.HasValue) consulta.IdMecanico = restringido;

            if (consulta.Pagina.HasValue) return Ok(await _service.GetPaginado(consulta));
            return Ok(restringido.HasValue ? await _service.GetPorMecanico(restringido.Value) : await _service.GetAll());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var data = await _service.GetById(id);
            if (data == null) return NotFound();

            // Una orden ajena se responde como "no encontrada" para no revelar que existe.
            var restringido = AlcanceOrdenes.IdMecanicoRestringido(User);
            if (restringido.HasValue && data.IdMecanico != restringido) return NotFound();

            return Ok(data);
        }

        [HttpPost]
        [RequierePermiso("ordenes.crear", Roles = "Administrador,Recepcionista")]
        public async Task<IActionResult> Create(OrdenTrabajoDTOCreate dto)
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

        [HttpPatch("{id}/estado")]
        [RequierePermiso("ordenes.cambiar_estado", Roles = "Administrador,Recepcionista,Mecanico")]
        public async Task<IActionResult> CambiarEstado(int id, OrdenTrabajoDTOEstado dto)
        {
            if (!await AlcanceOrdenes.PuedeAccederAsync(User, _service, id))
                return NotFound(new { mensaje = "La orden de trabajo no fue encontrada." });

            try
            {
                await _service.CambiarEstado(id, dto);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        // Cambia el mecánico asignado (p. ej. por ausencia o carga de trabajo). La orden
        // deja de ser visible para el mecánico anterior si este no tiene "ordenes.ver_todas".
        [HttpPatch("{id}/mecanico")]
        [RequierePermiso("ordenes.reasignar", Roles = "Administrador,Recepcionista")]
        public async Task<IActionResult> Reasignar(int id, OrdenTrabajoDTOReasignar dto)
        {
            if (!await AlcanceOrdenes.PuedeAccederAsync(User, _service, id))
                return NotFound(new { mensaje = "La orden de trabajo no fue encontrada." });

            try
            {
                await _service.Reasignar(id, dto);
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
        [RequierePermiso("ordenes.eliminar", Roles = "Administrador")]
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
    }
}
