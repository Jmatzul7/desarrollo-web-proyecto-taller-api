using TallerMecanicoAPI.DTOs;
using TallerMecanicoAPI.Helpers;
using TallerMecanicoAPI.Models;
using TallerMecanicoAPI.Repositories;

namespace TallerMecanicoAPI.Services
{
    public class OrdenTrabajoService : IOrdenTrabajoService
    {
        private readonly IOrdenTrabajoRepository _repo;
        private readonly IVehiculoRepository _vehiculoRepo;
        private readonly IUsuarioRepository _usuarioRepo;
        private readonly ICatalogoRepository _catalogoRepo;
        private readonly IDiagnosticoRepository _diagnosticoRepo;
        private readonly IReparacionRepository _reparacionRepo;

        public OrdenTrabajoService(
            IOrdenTrabajoRepository repo,
            IVehiculoRepository vehiculoRepo,
            IUsuarioRepository usuarioRepo,
            ICatalogoRepository catalogoRepo,
            IDiagnosticoRepository diagnosticoRepo,
            IReparacionRepository reparacionRepo)
        {
            _diagnosticoRepo = diagnosticoRepo;
            _reparacionRepo = reparacionRepo;
            _repo = repo;
            _vehiculoRepo = vehiculoRepo;
            _usuarioRepo = usuarioRepo;
            _catalogoRepo = catalogoRepo;
        }

        public async Task<List<OrdenTrabajo>> GetAll() => await _repo.GetAll();

        public async Task<List<OrdenTrabajo>> GetPorMecanico(int idMecanico) => await _repo.GetPorMecanico(idMecanico);

        public async Task<ResultadoPaginado<OrdenTrabajo>> GetPaginado(ConsultaPaginada consulta) => await _repo.GetPaginado(consulta);

        public async Task<OrdenTrabajo?> GetById(int id) => await _repo.GetById(id);

        public async Task Add(OrdenTrabajoDTOCreate dto)
        {
            if (await _vehiculoRepo.GetById(dto.IdVehiculo) == null)
                throw new ArgumentException("El vehículo indicado no existe.");

            var mecanico = await _usuarioRepo.GetById(dto.IdMecanico)
                ?? throw new ArgumentException("El mecánico indicado no existe.");

            if (mecanico.Rol?.Nombre != "Mecanico")
                throw new ArgumentException("El usuario asignado no tiene el rol de Mecánico.");

            var orden = new OrdenTrabajo
            {
                IdVehiculo = dto.IdVehiculo,
                IdMecanico = dto.IdMecanico,
                Observaciones = dto.Observaciones,
                IdEstadoOrden = 1 // Recibido
            };

            await _repo.Add(orden);
        }

        public async Task CambiarEstado(int id, OrdenTrabajoDTOEstado dto)
        {
            var orden = await _repo.GetById(id)
                ?? throw new ArgumentException("La orden de trabajo no fue encontrada.");

            if (!await _catalogoRepo.EstadoOrdenExiste(dto.IdEstadoOrden))
                throw new ArgumentException("El estado de orden indicado no existe.");

            var actual = orden.IdEstadoOrden;
            var nuevo = dto.IdEstadoOrden;

            if (nuevo == actual)
                return;

            FlujoTrabajo.ExigirOrdenAbierta(orden, "cambiar el estado");

            // "Facturada" solo la asigna el sistema al generar la factura.
            if (nuevo == FlujoTrabajo.OrdenFacturada)
                throw new InvalidOperationException(
                    "El estado Facturada se asigna al generar la factura de la orden.");

            if (nuevo == FlujoTrabajo.OrdenCancelada)
            {
                // Cancelar es posible desde cualquier estado abierto.
            }
            else if (nuevo != actual + 1)
            {
                throw new InvalidOperationException(
                    "Cambio de estado no permitido: la orden avanza paso a paso " +
                    "(Recibido → En Diagnóstico → En Reparación → Finalizada) o se cancela.");
            }
            else
            {
                // Avanzar manualmente exige que el trabajo lo respalde.
                var diagnosticos = await _diagnosticoRepo.GetByOrden(id);
                var reparaciones = await _reparacionRepo.GetByDiagnosticoIds(
                    diagnosticos.Select(d => d.IdDiagnostico).ToList());
                var activas = reparaciones.Where(r => r.IdEstadoReparacion != FlujoTrabajo.RepRechazada).ToList();

                if (nuevo == FlujoTrabajo.OrdenEnDiagnostico && diagnosticos.Count == 0)
                    throw new InvalidOperationException("Registra un diagnóstico para pasar la orden a En Diagnóstico.");

                if (nuevo == FlujoTrabajo.OrdenEnReparacion &&
                    !activas.Any(r => r.IdEstadoReparacion != FlujoTrabajo.RepPendiente))
                    throw new InvalidOperationException(
                        "Para pasar a En Reparación debe haber al menos una reparación autorizada o en proceso.");

                if (nuevo == FlujoTrabajo.OrdenFinalizada &&
                    (activas.Count == 0 || activas.Any(r => r.IdEstadoReparacion != FlujoTrabajo.RepFinalizada)))
                    throw new InvalidOperationException(
                        "No se puede finalizar la orden: todas las reparaciones (salvo las rechazadas) deben estar finalizadas.");
            }

            orden.IdEstadoOrden = nuevo;

            if (nuevo == FlujoTrabajo.OrdenFinalizada)
                orden.FechaSalida ??= DateTime.Now;

            await _repo.Update(orden);
        }

        public async Task Reasignar(int id, OrdenTrabajoDTOReasignar dto)
        {
            var orden = await _repo.GetById(id)
                ?? throw new ArgumentException("La orden de trabajo no fue encontrada.");

            FlujoTrabajo.ExigirOrdenAbierta(orden, "reasignar el mecánico");

            var mecanico = await _usuarioRepo.GetById(dto.IdMecanico)
                ?? throw new ArgumentException("El mecánico indicado no existe.");

            if (mecanico.Rol?.Nombre != "Mecanico")
                throw new ArgumentException("El usuario asignado no tiene el rol de Mecánico.");

            if (!mecanico.Activo)
                throw new InvalidOperationException("El mecánico indicado está inactivo.");

            if (orden.IdMecanico == dto.IdMecanico)
                throw new InvalidOperationException("La orden ya está asignada a ese mecánico.");

            orden.IdMecanico = dto.IdMecanico;
            await _repo.Update(orden);
        }

        public async Task Delete(int id)
        {
            if (await _repo.GetById(id) == null)
                throw new ArgumentException("La orden de trabajo no fue encontrada.");

            await _repo.Delete(id);
        }
    }
}
