using TallerMecanicoAPI.DTOs;
using TallerMecanicoAPI.Models;
using TallerMecanicoAPI.Repositories;

namespace TallerMecanicoAPI.Services
{
    public class ClienteService : IClienteService
    {
        private readonly IClienteRepository _repo;
        public ClienteService(IClienteRepository repo) => _repo = repo;

        public async Task<List<Cliente>> GetAll() => await _repo.GetAll();

        public async Task<Cliente?> GetById(int id) => await _repo.GetById(id);

        public async Task Add(ClienteDTOCreate dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre))
                throw new ArgumentException("El nombre del cliente es obligatorio.");

            var cliente = new Cliente
            {
                Nombre = dto.Nombre,
                Telefono = dto.Telefono,
                Email = dto.Email,
                Direccion = dto.Direccion,
                Activo = true
            };

            await _repo.Add(cliente);
        }

        public async Task Update(int id, ClienteDTOUpdate dto)
        {
            var cliente = await _repo.GetById(id)
                ?? throw new ArgumentException("El cliente no fue encontrado.");

            cliente.Nombre = dto.Nombre;
            cliente.Telefono = dto.Telefono;
            cliente.Email = dto.Email;
            cliente.Direccion = dto.Direccion;
            cliente.Activo = dto.Activo;

            await _repo.Update(cliente);
        }

        // "Eliminar" un cliente es desactivarlo (soft delete): conserva su historial
        // de vehículos y órdenes, pero deja de aparecer como cliente activo.
        public async Task Delete(int id)
        {
            var cliente = await _repo.GetById(id)
                ?? throw new ArgumentException("El cliente no fue encontrado.");

            if (!cliente.Activo)
                throw new InvalidOperationException("El cliente ya está inactivo.");

            cliente.Activo = false;
            await _repo.Update(cliente);
        }
    }
}
