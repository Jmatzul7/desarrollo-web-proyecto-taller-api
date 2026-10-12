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
        public DiagnosticosController(IDiagnosticoService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> Get() => Ok(await _service.GetAll());

        [HttpGet("orden/{idOrden}")]
        public async Task<IActionResult> GetByOrden(int idOrden) => Ok(await _service.GetByOrden(idOrden));

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var data = await _service.GetById(id);
            return data == null ? NotFound() : Ok(data);
        }

        [HttpPost]
        [RequierePermiso("diagnosticos.crear", Roles = "Administrador,Mecanico")]
        public async Task<IActionResult> Create(DiagnosticoDTOCreate dto)
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
