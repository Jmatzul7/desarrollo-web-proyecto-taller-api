using TallerMecanicoAPI.DTOs;
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

        public ReparacionService(IReparacionRepository repo, IDiagnosticoRepository diagnosticoRepo, ICatalogoRepository catalogoRepo)
        {
            _repo = repo;
            _diagnosticoRepo = diagnosticoRepo;
            _catalogoRepo = catalogoRepo;
        }

        public async Task<List<Reparacion>> GetAll() => await _repo.GetAll();

        public async Task<Reparacion?> GetById(int id) => await _repo.GetById(id);

        public async Task Add(ReparacionDTOCreate dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Descripcion))
                throw new ArgumentException("La descripción de la reparación es obligatoria.");

            if (await _diagnosticoRepo.GetById(dto.IdDiagnostico) == null)
                throw new ArgumentException("El diagnóstico indicado no existe.");

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
        }

        public async Task Autorizar(int id, int idUsuarioAutoriza, ReparacionDTOAutorizar dto)
        {
            var reparacion = await _repo.GetById(id)
                ?? throw new ArgumentException("La reparación no fue encontrada.");

            if (reparacion.IdEstadoReparacion != 1)
                throw new InvalidOperationException("Solo se pueden autorizar reparaciones en estado Pendiente.");

            reparacion.IdEstadoReparacion = dto.Aprobar ? 2 : 3; // Autorizada / Rechazada
            reparacion.IdAutorizadoPor = idUsuarioAutoriza;
            reparacion.FechaAutorizacion = DateTime.Now;

            await _repo.Update(reparacion);
        }

        public async Task CambiarEstado(int id, ReparacionDTOEstado dto)
        {
            var reparacion = await _repo.GetById(id)
                ?? throw new ArgumentException("La reparación no fue encontrada.");

            if (!await _catalogoRepo.EstadoReparacionExiste(dto.IdEstadoReparacion))
                throw new ArgumentException("El estado de reparación indicado no existe.");

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
        }

        public async Task Delete(int id)
        {
            if (await _repo.GetById(id) == null)
                throw new ArgumentException("La reparación no fue encontrada.");

            await _repo.Delete(id);
        }
    }
}
