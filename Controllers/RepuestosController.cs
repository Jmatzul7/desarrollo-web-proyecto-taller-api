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
    public class RepuestosController : ControllerBase
    {
        private readonly IRepuestoService _service;
        public RepuestosController(IRepuestoService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> Get() => Ok(await _service.GetAll());

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var data = await _service.GetById(id);
            return data == null ? NotFound() : Ok(data);
        }

        [HttpPost]
        [RequierePermiso("repuestos.crear", Roles = "Administrador,Bodeguero")]
        public async Task<IActionResult> Create(RepuestoDTOCreate dto)
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

        [HttpPut("{id}")]
        [RequierePermiso("repuestos.editar", Roles = "Administrador,Bodeguero")]
        public async Task<IActionResult> Update(int id, RepuestoDTOUpdate dto)
        {
            try
            {
                await _service.Update(id, dto);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        // Entrada de mercadería a bodega
        [HttpPatch("{id}/entrada")]
        [RequierePermiso("repuestos.entrada_stock", Roles = "Administrador,Bodeguero")]
        public async Task<IActionResult> EntradaStock(int id, RepuestoDTOAjusteStock dto)
        {
            try
            {
                await _service.EntradaStock(id, dto);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [RequierePermiso("repuestos.eliminar", Roles = "Administrador")]
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
