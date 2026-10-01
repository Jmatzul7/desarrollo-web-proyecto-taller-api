using TallerMecanicoAPI.DTOs;
using TallerMecanicoAPI.Models;
using TallerMecanicoAPI.Repositories;

namespace TallerMecanicoAPI.Services
{
    public class DiagnosticoService : IDiagnosticoService
    {
        private readonly IDiagnosticoRepository _repo;
        private readonly IOrdenTrabajoRepository _ordenRepo;
        private readonly IUsuarioRepository _usuarioRepo;

        public DiagnosticoService(IDiagnosticoRepository repo, IOrdenTrabajoRepository ordenRepo, IUsuarioRepository usuarioRepo)
        {
            _repo = repo;
            _ordenRepo = ordenRepo;
            _usuarioRepo = usuarioRepo;
        }

        public async Task<List<Diagnostico>> GetAll() => await _repo.GetAll();

        public async Task<List<Diagnostico>> GetByOrden(int idOrden) => await _repo.GetByOrden(idOrden);

        public async Task<Diagnostico?> GetById(int id) => await _repo.GetById(id);

        public async Task Add(DiagnosticoDTOCreate dto)
        {
            if (string.IsNullOrWhiteSpace(dto.DescripcionFalla))
                throw new ArgumentException("La descripción de la falla es obligatoria.");

            var orden = await _ordenRepo.GetById(dto.IdOrden)
                ?? throw new ArgumentException("La orden de trabajo indicada no existe.");

            var mecanico = await _usuarioRepo.GetById(dto.IdMecanico)
                ?? throw new ArgumentException("El mecánico indicado no existe.");

            if (mecanico.Rol?.Nombre != "Mecanico")
                throw new ArgumentException("El usuario indicado no tiene el rol de Mecánico.");

            var diagnostico = new Diagnostico
            {
                IdOrden = dto.IdOrden,
                IdMecanico = dto.IdMecanico,
                DescripcionFalla = dto.DescripcionFalla
            };

            await _repo.Add(diagnostico);

            // Al registrar un diagnóstico, la orden pasa a estado "En Diagnostico"
            orden.IdEstadoOrden = 2;
            await _ordenRepo.Update(orden);
        }

        public async Task Delete(int id)
        {
            if (await _repo.GetById(id) == null)
                throw new ArgumentException("El diagnóstico no fue encontrado.");

            await _repo.Delete(id);
        }
    }
}
