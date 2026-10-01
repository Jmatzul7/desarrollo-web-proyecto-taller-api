using TallerMecanicoAPI.DTOs;
using TallerMecanicoAPI.Models;
using TallerMecanicoAPI.Repositories;

namespace TallerMecanicoAPI.Services
{
    // Controla la salida de repuestos de bodega hacia una reparación.
    // Exige: reparación autorizada, existencia suficiente y justificación.
    public class ReparacionRepuestoService : IReparacionRepuestoService
    {
        private readonly IReparacionRepuestoRepository _repo;
        private readonly IReparacionRepository _reparacionRepo;
        private readonly IRepuestoRepository _repuestoRepo;

        public ReparacionRepuestoService(
            IReparacionRepuestoRepository repo,
            IReparacionRepository reparacionRepo,
            IRepuestoRepository repuestoRepo)
        {
            _repo = repo;
            _reparacionRepo = reparacionRepo;
            _repuestoRepo = repuestoRepo;
        }

        public async Task<List<ReparacionRepuesto>> GetByReparacion(int idReparacion) =>
            await _repo.GetByReparacion(idReparacion);

        public async Task Add(int idReparacion, int idUsuarioAutoriza, ReparacionRepuestoDTOCreate dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Justificacion))
                throw new ArgumentException("Debe indicar una justificación para el uso del repuesto.");

            if (dto.Cantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor a cero.");

            var reparacion = await _reparacionRepo.GetById(idReparacion)
                ?? throw new ArgumentException("La reparación indicada no existe.");

            // Estado 2 = Autorizada, 4 = En Proceso
            if (reparacion.IdEstadoReparacion != 2 && reparacion.IdEstadoReparacion != 4)
                throw new InvalidOperationException(
                    "Solo se pueden asignar repuestos a reparaciones autorizadas o en proceso.");

            var repuesto = await _repuestoRepo.GetById(dto.IdRepuesto)
                ?? throw new ArgumentException("El repuesto indicado no existe.");

            if (repuesto.Existencia < dto.Cantidad)
                throw new InvalidOperationException(
                    $"Existencia insuficiente. Disponible: {repuesto.Existencia}, solicitado: {dto.Cantidad}.");

            var detalle = new ReparacionRepuesto
            {
                IdReparacion = idReparacion,
                IdRepuesto = dto.IdRepuesto,
                Cantidad = dto.Cantidad,
                PrecioUnitario = repuesto.PrecioUnitario,
                Justificacion = dto.Justificacion,
                IdAutorizadoPor = idUsuarioAutoriza
            };

            await _repo.Add(detalle);

            // Descuenta la existencia en bodega
            repuesto.Existencia -= dto.Cantidad;
            await _repuestoRepo.Update(repuesto);
        }

        public async Task Delete(int id)
        {
            var detalle = await _repo.GetById(id)
                ?? throw new ArgumentException("El detalle de repuesto indicado no fue encontrado.");

            // Al eliminar el uso, se restituye la existencia en bodega
            var repuesto = await _repuestoRepo.GetById(detalle.IdRepuesto);
            if (repuesto != null)
            {
                repuesto.Existencia += detalle.Cantidad;
                await _repuestoRepo.Update(repuesto);
            }

            await _repo.Delete(id);
        }
    }
}
