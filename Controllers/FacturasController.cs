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
    public class FacturasController : ControllerBase
    {
        private readonly IFacturaService _service;
        public FacturasController(IFacturaService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> Get() => Ok(await _service.GetAll());

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var data = await _service.GetById(id);
            return data == null ? NotFound() : Ok(data);
        }

        [HttpPost]
        [RequierePermiso("facturas.generar", Roles = "Administrador,Recepcionista")]
        public async Task<IActionResult> Generar(FacturaDTOCreate dto)
        {
            try
            {
                var factura = await _service.Generar(dto);
                return Ok(factura);
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

        // Descarga el PDF de una factura ya generada
        [HttpGet("{id}/pdf")]
        public async Task<IActionResult> DescargarPdf(int id)
        {
            var detalle = await _service.ObtenerDetalle(id);

            if (detalle == null)
                return NotFound(new { mensaje = "La factura indicada no existe." });

            var pdfBytes = FacturaPdfGenerator.Generar(detalle);
            var nombreArchivo = $"Factura_{detalle.IdFactura:D6}.pdf";

            return File(pdfBytes, "application/pdf", nombreArchivo);
        }
    }
}
