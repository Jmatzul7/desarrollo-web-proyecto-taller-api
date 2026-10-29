using TallerMecanicoAPI.Helpers;
using TallerMecanicoAPI.DTOs;
using TallerMecanicoAPI.Models;
using TallerMecanicoAPI.Repositories;

namespace TallerMecanicoAPI.Services
{
    public class RepuestoService : IRepuestoService
    {
        private readonly IRepuestoRepository _repo;
        public RepuestoService(IRepuestoRepository repo) => _repo = repo;

        public async Task<List<Repuesto>> GetAll() => await _repo.GetAll();

        public async Task<ResultadoPaginado<Repuesto>> GetPaginado(ConsultaPaginada consulta) => await _repo.GetPaginado(consulta);

        public async Task<Repuesto?> GetById(int id) => await _repo.GetById(id);

        public async Task Add(RepuestoDTOCreate dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Codigo) || string.IsNullOrWhiteSpace(dto.Nombre))
                throw new ArgumentException("El código y el nombre del repuesto son obligatorios.");

            if (dto.Existencia < 0)
                throw new ArgumentException("La existencia no puede ser negativa.");

            if (dto.PrecioUnitario <= 0)
                throw new ArgumentException("El precio unitario debe ser mayor a cero.");

            if (await _repo.GetByCodigo(dto.Codigo) != null)
                throw new ArgumentException("Ya existe un repuesto con ese código.");

            var repuesto = new Repuesto
            {
                Codigo = dto.Codigo,
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                Existencia = dto.Existencia,
                PrecioUnitario = dto.PrecioUnitario,
                Activo = true
            };

            await _repo.Add(repuesto);
        }

        public async Task Update(int id, RepuestoDTOUpdate dto)
        {
            var repuesto = await _repo.GetById(id)
                ?? throw new ArgumentException("El repuesto no fue encontrado.");

            if (string.IsNullOrWhiteSpace(dto.Nombre))
                throw new ArgumentException("El nombre del repuesto es obligatorio.");

            if (dto.PrecioUnitario <= 0)
                throw new ArgumentException("El precio unitario debe ser mayor a cero.");

            repuesto.Nombre = dto.Nombre;
            repuesto.Descripcion = dto.Descripcion;
            repuesto.PrecioUnitario = dto.PrecioUnitario;
            repuesto.Activo = dto.Activo;

            await _repo.Update(repuesto);
        }

        public async Task EntradaStock(int id, RepuestoDTOAjusteStock dto)
        {
            if (dto.Cantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor a cero.");

            var repuesto = await _repo.GetById(id)
                ?? throw new ArgumentException("El repuesto no fue encontrado.");

            repuesto.Existencia += dto.Cantidad;
            await _repo.Update(repuesto);
        }

        // "Eliminar" un repuesto es desactivarlo (soft delete): conserva el historial
        // de uso en reparaciones pasadas, pero deja de estar disponible para nuevas.
        public async Task Delete(int id)
        {
            var repuesto = await _repo.GetById(id)
                ?? throw new ArgumentException("El repuesto no fue encontrado.");

            if (!repuesto.Activo)
                throw new InvalidOperationException("El repuesto ya está inactivo.");

            repuesto.Activo = false;
            await _repo.Update(repuesto);
        }
    }
}
