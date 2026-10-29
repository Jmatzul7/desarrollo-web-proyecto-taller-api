using TallerMecanicoAPI.DTOs;
using TallerMecanicoAPI.Helpers;
using TallerMecanicoAPI.Models;
using TallerMecanicoAPI.Repositories;

namespace TallerMecanicoAPI.Services
{
    // Genera la factura de una orden: suma la mano de obra de las reparaciones
    // finalizadas y el costo de los repuestos utilizados en cada una.
    public class FacturaService : IFacturaService
    {
        private readonly IFacturaRepository _facturaRepo;
        private readonly IOrdenTrabajoRepository _ordenRepo;
        private readonly IDiagnosticoRepository _diagnosticoRepo;
        private readonly IReparacionRepository _reparacionRepo;
        private readonly IReparacionRepuestoRepository _reparacionRepuestoRepo;

        public FacturaService(
            IFacturaRepository facturaRepo,
            IOrdenTrabajoRepository ordenRepo,
            IDiagnosticoRepository diagnosticoRepo,
            IReparacionRepository reparacionRepo,
            IReparacionRepuestoRepository reparacionRepuestoRepo)
        {
            _facturaRepo = facturaRepo;
            _ordenRepo = ordenRepo;
            _diagnosticoRepo = diagnosticoRepo;
            _reparacionRepo = reparacionRepo;
            _reparacionRepuestoRepo = reparacionRepuestoRepo;
        }

        public async Task<List<Factura>> GetAll() => await _facturaRepo.GetAll();

        public async Task<ResultadoPaginado<Factura>> GetPaginado(ConsultaPaginada consulta) => await _facturaRepo.GetPaginado(consulta);

        public async Task<Factura?> GetById(int id) => await _facturaRepo.GetById(id);

        public async Task<Factura> Generar(FacturaDTOCreate dto)
        {
            var orden = await _ordenRepo.GetById(dto.IdOrden)
                ?? throw new ArgumentException("La orden de trabajo no fue encontrada.");

            if (await _facturaRepo.GetByOrden(dto.IdOrden) != null)
                throw new InvalidOperationException("Esta orden ya tiene una factura generada.");

            var diagnosticos = await _diagnosticoRepo.GetByOrden(dto.IdOrden);
            var idsDiagnostico = diagnosticos.Select(d => d.IdDiagnostico).ToList();
            var reparaciones = await _reparacionRepo.GetByDiagnosticoIds(idsDiagnostico);

            // Regla de flujo: no se factura con reparaciones sin finalizar (pendientes, autorizadas o
            // en proceso), ni una orden ya facturada o cancelada. Las rechazadas no bloquean.
            var motivo = FlujoTrabajo.MotivoNoFacturable(
                orden.IdEstadoOrden, reparaciones.Select(r => r.IdEstadoReparacion).ToList());
            if (motivo != null)
                throw new InvalidOperationException(motivo);

            var reparacionesFinalizadas = reparaciones
                .Where(r => r.IdEstadoReparacion == FlujoTrabajo.RepFinalizada).ToList();

            decimal costoManoObra = reparacionesFinalizadas.Sum(r => r.CostoManoObra ?? 0);
            decimal costoRepuestos = 0;

            foreach (var reparacion in reparacionesFinalizadas)
            {
                var detalles = await _reparacionRepuestoRepo.GetByReparacion(reparacion.IdReparacion);
                costoRepuestos += detalles.Sum(d => d.Cantidad * d.PrecioUnitario);
            }

            var factura = new Factura
            {
                IdOrden = dto.IdOrden,
                CostoManoObra = costoManoObra,
                CostoRepuestos = costoRepuestos,
                Total = costoManoObra + costoRepuestos,
                Estado = "Emitida"
            };

            await _facturaRepo.Add(factura);

            orden.IdEstadoOrden = FlujoTrabajo.OrdenFacturada;
            orden.FechaSalida ??= DateTime.Now;
            await _ordenRepo.Update(orden);

            return factura;
        }

        public async Task<FacturaDetalleDTO?> ObtenerDetalle(int idFactura)
        {
            var factura = await _facturaRepo.GetById(idFactura);
            if (factura?.Orden == null) return null;

            var orden = factura.Orden;

            var diagnosticos = await _diagnosticoRepo.GetByOrden(orden.IdOrden);
            var idsDiagnostico = diagnosticos.Select(d => d.IdDiagnostico).ToList();

            // 5 = Finalizada: solo las reparaciones que efectivamente se facturaron
            var reparaciones = (await _reparacionRepo.GetByDiagnosticoIds(idsDiagnostico))
                .Where(r => r.IdEstadoReparacion == 5)
                .ToList();

            var detalle = new FacturaDetalleDTO
            {
                IdFactura = factura.IdFactura,
                FechaEmision = factura.FechaEmision,
                Estado = factura.Estado,
                CostoManoObra = factura.CostoManoObra,
                CostoRepuestos = factura.CostoRepuestos,
                Total = factura.Total,
                IdOrden = orden.IdOrden,
                FechaIngreso = orden.FechaIngreso,
                ClienteNombre = orden.Vehiculo?.Cliente?.Nombre ?? "N/D",
                ClienteTelefono = orden.Vehiculo?.Cliente?.Telefono,
                ClienteEmail = orden.Vehiculo?.Cliente?.Email,
                ClienteDireccion = orden.Vehiculo?.Cliente?.Direccion,
                VehiculoPlaca = orden.Vehiculo?.Placa ?? "N/D",
                VehiculoMarca = orden.Vehiculo?.Marca ?? "",
                VehiculoModelo = orden.Vehiculo?.Modelo ?? "",
                VehiculoAnio = orden.Vehiculo?.Anio,
                MecanicoNombre = orden.Mecanico?.NombreCompleto ?? "N/D"
            };

            foreach (var reparacion in reparaciones)
            {
                var repuestosUsados = await _reparacionRepuestoRepo.GetByReparacion(reparacion.IdReparacion);

                detalle.Reparaciones.Add(new FacturaDetalleReparacionDTO
                {
                    Descripcion = reparacion.Descripcion,
                    CostoManoObra = reparacion.CostoManoObra ?? 0,
                    Repuestos = repuestosUsados.Select(rr => new FacturaDetalleRepuestoDTO
                    {
                        Nombre = rr.Repuesto?.Nombre ?? "N/D",
                        Cantidad = rr.Cantidad,
                        PrecioUnitario = rr.PrecioUnitario
                    }).ToList()
                });
            }

            return detalle;
        }
    }
}
