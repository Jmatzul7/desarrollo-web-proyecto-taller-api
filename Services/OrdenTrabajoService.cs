using TallerMecanicoAPI.DTOs;
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

        public OrdenTrabajoService(
            IOrdenTrabajoRepository repo,
            IVehiculoRepository vehiculoRepo,
            IUsuarioRepository usuarioRepo,
            ICatalogoRepository catalogoRepo)
        {
            _repo = repo;
            _vehiculoRepo = vehiculoRepo;
            _usuarioRepo = usuarioRepo;
            _catalogoRepo = catalogoRepo;
        }

        public async Task<List<OrdenTrabajo>> GetAll() => await _repo.GetAll();

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

            orden.IdEstadoOrden = dto.IdEstadoOrden;

            if (dto.IdEstadoOrden == 4 || dto.IdEstadoOrden == 5) // Finalizada o Facturada
                orden.FechaSalida = DateTime.Now;

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
