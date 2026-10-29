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
    public class DiagnosticosController : ControllerBase
    {
        private readonly IDiagnosticoService _service;
        private readonly IOrdenTrabajoService _ordenes;

        public DiagnosticosController(IDiagnosticoService service, IOrdenTrabajoService ordenes)
        {
            _service = service;
            _ordenes = ordenes;
        }

        private static readonly object OrdenNoEncontrada = new { mensaje = "La orden de trabajo no fue encontrada." };

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var todos = await _service.GetAll();
            var restringido = AlcanceOrdenes.IdMecanicoRestringido(User);
            if (restringido == null) return Ok(todos);

            // Solo los diagnósticos de las órdenes asignadas al usuario.
            var propias = (await _ordenes.GetPorMecanico(restringido.Value)).Select(o => o.IdOrden).ToHashSet();
            return Ok(todos.Where(d => propias.Contains(d.IdOrden)).ToList());
        }

        [HttpGet("orden/{idOrden}")]
        public async Task<IActionResult> GetByOrden(int idOrden)
        {
            if (!await AlcanceOrdenes.PuedeAccederAsync(User, _ordenes, idOrden))
                return NotFound(OrdenNoEncontrada);

            return Ok(await _service.GetByOrden(idOrden));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var data = await _service.GetById(id);
            if (data == null) return NotFound();
            if (!await AlcanceOrdenes.PuedeAccederAsync(User, _ordenes, data.IdOrden)) return NotFound();
            return Ok(data);
        }

        [HttpPost]
        [RequierePermiso("diagnosticos.crear", Roles = "Administrador,Mecanico")]
        public async Task<IActionResult> Create(DiagnosticoDTOCreate dto)
        {
            if (!await AlcanceOrdenes.PuedeAccederAsync(User, _ordenes, dto.IdOrden))
                return NotFound(OrdenNoEncontrada);

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

        [HttpDelete("{id}")]
        [RequierePermiso("diagnosticos.eliminar", Roles = "Administrador")]
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
