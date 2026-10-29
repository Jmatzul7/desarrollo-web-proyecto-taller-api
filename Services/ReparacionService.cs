using TallerMecanicoAPI.DTOs;
using TallerMecanicoAPI.Helpers;
using TallerMecanicoAPI.Models;
using TallerMecanicoAPI.Repositories;

namespace TallerMecanicoAPI.Services
{
    // Estados de reparación (según seed de EstadoReparacion):
    // 1 = Pendiente, 2 = Autorizada, 3 = Rechazada, 4 = En Proceso, 5 = Finalizada
    public class ReparacionService : IReparacionService
    {
        private readonly IReparacionRepository _repo;
        private readonly IDiagnosticoRepository _diagnosticoRepo;
        private readonly ICatalogoRepository _catalogoRepo;
        private readonly IOrdenTrabajoRepository _ordenRepo;

        public ReparacionService(
            IReparacionRepository repo,
            IDiagnosticoRepository diagnosticoRepo,
            ICatalogoRepository catalogoRepo,
            IOrdenTrabajoRepository ordenRepo)
        {
            _repo = repo;
            _diagnosticoRepo = diagnosticoRepo;
            _catalogoRepo = catalogoRepo;
            _ordenRepo = ordenRepo;
        }

        // Orden a la que pertenece una reparación (reparación -> diagnóstico -> orden).
        private async Task<OrdenTrabajo> ObtenerOrdenDeDiagnostico(int idDiagnostico)
        {
            var diagnostico = await _diagnosticoRepo.GetById(idDiagnostico)
                ?? throw new ArgumentException("El diagnóstico indicado no existe.");
            return await _ordenRepo.GetById(diagnostico.IdOrden)
                ?? throw new ArgumentException("La orden de trabajo de la reparación no existe.");
        }

        // Recalcula el estado de la orden a partir del estado de todas sus reparaciones.
        private async Task SincronizarEstadoOrden(int idOrden)
        {
            var orden = await _ordenRepo.GetById(idOrden);
            if (orden == null) return;

            var diagnosticos = await _diagnosticoRepo.GetByOrden(idOrden);
            var reparaciones = await _repo.GetByDiagnosticoIds(diagnosticos.Select(d => d.IdDiagnostico).ToList());

            var nuevo = FlujoTrabajo.EstadoOrdenCalculado(
                orden.IdEstadoOrden, reparaciones.Select(r => r.IdEstadoReparacion));
            if (nuevo == null) return;

            orden.IdEstadoOrden = nuevo.Value;
            orden.FechaSalida = nuevo.Value == FlujoTrabajo.OrdenFinalizada ? (orden.FechaSalida ?? DateTime.Now) : null;
            await _ordenRepo.Update(orden);
        }

        public async Task<List<Reparacion>> GetAll() => await _repo.GetAll();

        public async Task<Reparacion?> GetById(int id) => await _repo.GetById(id);

        public async Task<List<Reparacion>> GetPorMecanico(int idMecanico)
        {
            var ordenes = await _ordenRepo.GetPorMecanico(idMecanico);
            var idsDiagnostico = new List<int>();
            foreach (var orden in ordenes)
                idsDiagnostico.AddRange((await _diagnosticoRepo.GetByOrden(orden.IdOrden)).Select(d => d.IdDiagnostico));

            return await _repo.GetByDiagnosticoIds(idsDiagnostico);
        }

        public async Task<int?> GetIdOrdenDeDiagnostico(int idDiagnostico) =>
            (await _diagnosticoRepo.GetById(idDiagnostico))?.IdOrden;

        public async Task<int?> GetIdOrdenDeReparacion(int idReparacion)
        {
            var reparacion = await _repo.GetById(idReparacion);
            return reparacion == null ? null : await GetIdOrdenDeDiagnostico(reparacion.IdDiagnostico);
        }

        public async Task Add(ReparacionDTOCreate dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Descripcion))
                throw new ArgumentException("La descripción de la reparación es obligatoria.");

            var orden = await ObtenerOrdenDeDiagnostico(dto.IdDiagnostico);
            FlujoTrabajo.ExigirOrdenAbierta(orden, "agregar una reparación");

            if (dto.IdReparacionPrevia.HasValue &&
                await _repo.GetById(dto.IdReparacionPrevia.Value) == null)
                throw new ArgumentException("La reparación previa indicada no existe.");

            var reparacion = new Reparacion
            {
                IdDiagnostico = dto.IdDiagnostico,
                Descripcion = dto.Descripcion,
                Orden = dto.Orden,
                IdReparacionPrevia = dto.IdReparacionPrevia,
                CostoManoObra = dto.CostoManoObra,
                IdEstadoReparacion = 1 // Pendiente
            };

            await _repo.Add(reparacion);

            // Si la orden ya estaba Finalizada, la nueva reparación pendiente la devuelve a En Reparación.
            await SincronizarEstadoOrden(orden.IdOrden);
        }

        public async Task Autorizar(int id, int idUsuarioAutoriza, ReparacionDTOAutorizar dto)
        {
            var reparacion = await _repo.GetById(id)
                ?? throw new ArgumentException("La reparación no fue encontrada.");

            if (reparacion.IdEstadoReparacion != FlujoTrabajo.RepPendiente)
                throw new InvalidOperationException("Solo se pueden autorizar reparaciones en estado Pendiente.");

            var orden = await ObtenerOrdenDeDiagnostico(reparacion.IdDiagnostico);
            FlujoTrabajo.ExigirOrdenAbierta(orden, "autorizar o rechazar la reparación");

            reparacion.IdEstadoReparacion = dto.Aprobar ? 2 : 3; // Autorizada / Rechazada
            reparacion.IdAutorizadoPor = idUsuarioAutoriza;
            reparacion.FechaAutorizacion = DateTime.Now;

            await _repo.Update(reparacion);

            // Rechazar la última reparación abierta puede dejar la orden lista (Finalizada).
            await SincronizarEstadoOrden(orden.IdOrden);
        }

        public async Task CambiarEstado(int id, ReparacionDTOEstado dto)
        {
            var reparacion = await _repo.GetById(id)
                ?? throw new ArgumentException("La reparación no fue encontrada.");

            if (!await _catalogoRepo.EstadoReparacionExiste(dto.IdEstadoReparacion))
                throw new ArgumentException("El estado de reparación indicado no existe.");

            var orden = await ObtenerOrdenDeDiagnostico(reparacion.IdDiagnostico);
            FlujoTrabajo.ExigirOrdenAbierta(orden, "cambiar el estado de la reparación");

            // Solo se permite Autorizada -> En Proceso -> Finalizada (autorizar/rechazar es otra acción).
            if (!FlujoTrabajo.TransicionReparacionValida(reparacion.IdEstadoReparacion, dto.IdEstadoReparacion))
                throw new InvalidOperationException(
                    "Cambio de estado no permitido: el orden es Pendiente → Autorizada → En Proceso → Finalizada. " +
                    "Una reparación Rechazada o Finalizada ya no cambia.");

            // No puede avanzar a "En Proceso" si depende de otra reparación no finalizada
            if (dto.IdEstadoReparacion == 4 && reparacion.IdReparacionPrevia.HasValue)
            {
                var previa = await _repo.GetById(reparacion.IdReparacionPrevia.Value);
                if (previa != null && previa.IdEstadoReparacion != 5)
                    throw new InvalidOperationException(
                        "No se puede iniciar esta reparación: la reparación previa aún no ha finalizado.");
            }

            if (dto.IdEstadoReparacion == 4 && reparacion.IdEstadoReparacion != 2)
                throw new InvalidOperationException("La reparación debe estar Autorizada antes de iniciarse.");

            reparacion.IdEstadoReparacion = dto.IdEstadoReparacion;

            if (dto.IdEstadoReparacion == 4 && reparacion.FechaInicio == null)
                reparacion.FechaInicio = DateTime.Now;

            if (dto.IdEstadoReparacion == 5)
                reparacion.FechaFin = DateTime.Now;

            await _repo.Update(reparacion);

            // La orden pasa sola a En Reparación al iniciar, y a Finalizada al terminar la última.
            await SincronizarEstadoOrden(orden.IdOrden);
        }

        public async Task Delete(int id)
        {
            var reparacion = await _repo.GetById(id)
                ?? throw new ArgumentException("La reparación no fue encontrada.");

            var orden = await ObtenerOrdenDeDiagnostico(reparacion.IdDiagnostico);
            FlujoTrabajo.ExigirOrdenAbierta(orden, "eliminar la reparación");

            await _repo.Delete(id);
            await SincronizarEstadoOrden(orden.IdOrden);
        }
    }
}
