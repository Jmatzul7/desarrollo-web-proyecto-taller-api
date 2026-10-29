using TallerMecanicoAPI.Helpers;
using TallerMecanicoAPI.DTOs;
using TallerMecanicoAPI.Models;
using TallerMecanicoAPI.Repositories;

namespace TallerMecanicoAPI.Services
{
    public class VehiculoService : IVehiculoService
    {
        private readonly IVehiculoRepository _repo;
        private readonly IClienteRepository _clienteRepo;

        public VehiculoService(IVehiculoRepository repo, IClienteRepository clienteRepo)
        {
            _repo = repo;
            _clienteRepo = clienteRepo;
        }

        public async Task<List<Vehiculo>> GetAll() => await _repo.GetAll();

        public async Task<ResultadoPaginado<Vehiculo>> GetPaginado(ConsultaPaginada consulta) => await _repo.GetPaginado(consulta);

        public async Task<Vehiculo?> GetById(int id) => await _repo.GetById(id);

        public async Task Add(VehiculoDTOCreate dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Placa))
                throw new ArgumentException("La placa es obligatoria.");

            if (string.IsNullOrWhiteSpace(dto.Marca) || string.IsNullOrWhiteSpace(dto.Modelo))
                throw new ArgumentException("La marca y el modelo son obligatorios.");

            if (await _clienteRepo.GetById(dto.IdCliente) == null)
                throw new ArgumentException("El cliente indicado no existe.");

            if (await _repo.GetByPlaca(dto.Placa) != null)
                throw new ArgumentException("Ya existe un vehículo registrado con esa placa.");

            var vehiculo = new Vehiculo
            {
                Placa = dto.Placa,
                Marca = dto.Marca,
                Modelo = dto.Modelo,
                Anio = dto.Anio,
                Color = dto.Color,
                IdCliente = dto.IdCliente,
                Activo = true
            };

            await _repo.Add(vehiculo);
        }

        public async Task Update(int id, VehiculoDTOUpdate dto)
        {
            var vehiculo = await _repo.GetById(id)
                ?? throw new ArgumentException("El vehículo no fue encontrado.");

            if (string.IsNullOrWhiteSpace(dto.Marca) || string.IsNullOrWhiteSpace(dto.Modelo))
                throw new ArgumentException("La marca y el modelo son obligatorios.");

            vehiculo.Marca = dto.Marca;
            vehiculo.Modelo = dto.Modelo;
            vehiculo.Anio = dto.Anio;
            vehiculo.Color = dto.Color;
            vehiculo.Activo = dto.Activo;

            await _repo.Update(vehiculo);
        }

        // "Eliminar" un vehículo es desactivarlo (soft delete): conserva su historial
        // de órdenes de trabajo, pero deja de aparecer como vehículo activo.
        public async Task Delete(int id)
        {
            var vehiculo = await _repo.GetById(id)
                ?? throw new ArgumentException("El vehículo no fue encontrado.");

            if (!vehiculo.Activo)
                throw new InvalidOperationException("El vehículo ya está inactivo.");

            vehiculo.Activo = false;
            await _repo.Update(vehiculo);
        }
    }
}
