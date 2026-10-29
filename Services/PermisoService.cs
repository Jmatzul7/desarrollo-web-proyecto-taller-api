using TallerMecanicoAPI.DTOs;
using TallerMecanicoAPI.Models;
using TallerMecanicoAPI.Repositories;

namespace TallerMecanicoAPI.Services
{
    public class PermisoService : IPermisoService
    {
        private readonly IPermisoRepository _permisoRepo;
        private readonly IUsuarioRepository _usuarioRepo;

        public PermisoService(IPermisoRepository permisoRepo, IUsuarioRepository usuarioRepo)
        {
            _permisoRepo = permisoRepo;
            _usuarioRepo = usuarioRepo;
        }

        public async Task<List<Permiso>> GetAll() => await _permisoRepo.GetAll();

        public async Task<List<Permiso>> GetDeUsuario(int idUsuario)
        {
            if (await _usuarioRepo.GetById(idUsuario) == null)
                throw new ArgumentException("El usuario no fue encontrado.");

            return await _permisoRepo.GetDeUsuario(idUsuario);
        }

        public async Task Asignar(int idUsuario, UsuarioPermisoDTOAsignar dto)
        {
            if (await _usuarioRepo.GetById(idUsuario) == null)
                throw new ArgumentException("El usuario no fue encontrado.");

            if (!await _permisoRepo.Existe(dto.IdPermiso))
                throw new ArgumentException("El permiso indicado no existe.");

            if (await _permisoRepo.UsuarioTienePermiso(idUsuario, dto.IdPermiso))
                throw new InvalidOperationException("El usuario ya tiene asignado ese permiso.");

            await _permisoRepo.Asignar(idUsuario, dto.IdPermiso);
        }

        private async Task ValidarRolEditable(int idRol)
        {
            var nombre = await _permisoRepo.GetNombreRol(idRol)
                ?? throw new ArgumentException("El rol no fue encontrado.");

            // El Administrador ya tiene acceso a todo: asignarle permisos no tendría efecto.
            if (nombre == "Administrador")
                throw new InvalidOperationException("El rol Administrador ya tiene todos los permisos; no se edita.");
        }

        public async Task<List<Permiso>> GetDeRol(int idRol)
        {
            if (!await _permisoRepo.RolExiste(idRol))
                throw new ArgumentException("El rol no fue encontrado.");

            return await _permisoRepo.GetDeRol(idRol);
        }

        public async Task AsignarARol(int idRol, UsuarioPermisoDTOAsignar dto)
        {
            await ValidarRolEditable(idRol);

            if (!await _permisoRepo.Existe(dto.IdPermiso))
                throw new ArgumentException("El permiso indicado no existe.");

            if (await _permisoRepo.RolTienePermiso(idRol, dto.IdPermiso))
                throw new InvalidOperationException("El rol ya tiene asignado ese permiso.");

            await _permisoRepo.AsignarARol(idRol, dto.IdPermiso);
        }

        public async Task QuitarDeRol(int idRol, int idPermiso)
        {
            await ValidarRolEditable(idRol);

            if (!await _permisoRepo.RolTienePermiso(idRol, idPermiso))
                throw new ArgumentException("El rol no tiene asignado ese permiso.");

            await _permisoRepo.QuitarDeRol(idRol, idPermiso);
        }

        public async Task Quitar(int idUsuario, int idPermiso)
        {
            if (await _usuarioRepo.GetById(idUsuario) == null)
                throw new ArgumentException("El usuario no fue encontrado.");

            if (!await _permisoRepo.UsuarioTienePermiso(idUsuario, idPermiso))
                throw new ArgumentException("El usuario no tiene asignado ese permiso.");

            await _permisoRepo.Quitar(idUsuario, idPermiso);
        }
    }
}
