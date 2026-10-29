using TallerMecanicoAPI.Helpers;
using TallerMecanicoAPI.DTOs;
using TallerMecanicoAPI.Models;

namespace TallerMecanicoAPI.Services
{
    public interface IFacturaService
    {
        Task<List<Factura>> GetAll();
        Task<ResultadoPaginado<Factura>> GetPaginado(ConsultaPaginada consulta);
        Task<Factura?> GetById(int id);
        Task<Factura> Generar(FacturaDTOCreate dto);

        // Arma el detalle completo (cliente, vehículo, reparaciones y repuestos)
        // que se usa para generar el PDF de la factura.
        Task<FacturaDetalleDTO?> ObtenerDetalle(int idFactura);
    }
}
