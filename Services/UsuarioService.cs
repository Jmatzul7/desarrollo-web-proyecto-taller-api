using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using TallerMecanicoAPI.DTOs;
using TallerMecanicoAPI.Helpers;
using TallerMecanicoAPI.Models;
using TallerMecanicoAPI.Repositories;

namespace TallerMecanicoAPI.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _repo;
        private readonly ICatalogoRepository _catalogoRepo;

        public UsuarioService(IUsuarioRepository repo, ICatalogoRepository catalogoRepo)
        {
            _repo = repo;
            _catalogoRepo = catalogoRepo;
        }

        private static UsuarioDTOResponse ToDto(Usuario u) => new()
        {
            IdUsuario = u.IdUsuario,
            NombreUsuario = u.NombreUsuario,
            NombreCompleto = u.NombreCompleto,
            Email = u.Email,
            IdRol = u.IdRol,
            Rol = u.Rol?.Nombre,
            Activo = u.Activo,
            FechaCreacion = u.FechaCreacion
        };

        public async Task<List<UsuarioDTOResponse>> GetAll() =>
            (await _repo.GetAll()).Select(ToDto).ToList();

        public async Task<ResultadoPaginado<UsuarioDTOResponse>> GetPaginado(ConsultaPaginada consulta) =>
            (await _repo.GetPaginado(consulta)).Convertir(ToDto);

        public async Task<UsuarioDTOResponse?> GetById(int id)
        {
            var usuario = await _repo.GetById(id);
            return usuario == null ? null : ToDto(usuario);
        }

        public async Task Add(UsuarioDTOCreate dto) => await Crear(dto, activo: true, permitirAdministrador: true);

        // Autoregistro público (sin token): no puede auto-asignarse el rol Administrador
        // y la cuenta se crea inactiva hasta que un Administrador la active con Update.
        public async Task RegistrarPublico(UsuarioDTOCreate dto) => await Crear(dto, activo: false, permitirAdministrador: false);

        private async Task Crear(UsuarioDTOCreate dto, bool activo, bool permitirAdministrador)
        {
            if (string.IsNullOrWhiteSpace(dto.NombreUsuario) || string.IsNullOrWhiteSpace(dto.Password))
                throw new ArgumentException("El nombre de usuario y la contraseña son obligatorios.");

            if (string.IsNullOrWhiteSpace(dto.NombreCompleto))
                throw new ArgumentException("El nombre completo es obligatorio.");

            if (!await _catalogoRepo.RolExiste(dto.IdRol))
                throw new ArgumentException("El rol indicado no existe.");

            if (!permitirAdministrador && dto.IdRol == 1) // 1 = Administrador (ver seed de Rol)
                throw new ArgumentException("No es posible autoregistrarse con el rol Administrador.");

            if (await _repo.GetByNombreUsuario(dto.NombreUsuario) != null)
                throw new ArgumentException("Ya existe un usuario con ese nombre.");

            var usuario = new Usuario
            {
                NombreUsuario = dto.NombreUsuario,
                PasswordHash = PasswordHasher.Hash(dto.Password),
                NombreCompleto = dto.NombreCompleto,
                Email = dto.Email,
                IdRol = dto.IdRol,
                Activo = activo
            };

            await _repo.Add(usuario);
        }

        public async Task Update(int id, UsuarioDTOUpdate dto)
        {
            var usuario = await _repo.GetById(id)
                ?? throw new ArgumentException("El usuario no fue encontrado.");

            if (string.IsNullOrWhiteSpace(dto.NombreCompleto))
                throw new ArgumentException("El nombre completo es obligatorio.");

            if (!await _catalogoRepo.RolExiste(dto.IdRol))
                throw new ArgumentException("El rol indicado no existe.");

            usuario.NombreCompleto = dto.NombreCompleto;
            usuario.Email = dto.Email;
            usuario.IdRol = dto.IdRol;
            usuario.Activo = dto.Activo;

            await _repo.Update(usuario);
        }

        // ---------------------------------------------------------------------
        // Perfil propio
        // ---------------------------------------------------------------------

        private const int LongitudMinimaPassword = 6;
        private static readonly Regex PatronNombreUsuario = new(@"^[A-Za-z0-9._-]{3,50}$", RegexOptions.Compiled);

        public async Task<UsuarioDTOResponse?> GetPerfil(int idUsuario) => await GetById(idUsuario);

        public async Task<bool> NombreUsuarioDisponible(string nombreUsuario, int idUsuarioActual)
        {
            var existente = await _repo.GetByNombreUsuario((nombreUsuario ?? "").Trim());
            return existente == null || existente.IdUsuario == idUsuarioActual;
        }

        public async Task ActualizarPerfil(int idUsuario, PerfilDTOUpdate dto)
        {
            var usuario = await _repo.GetById(idUsuario)
                ?? throw new ArgumentException("El usuario no fue encontrado.");

            var nombreUsuario = (dto.NombreUsuario ?? "").Trim();
            var nombreCompleto = (dto.NombreCompleto ?? "").Trim();
            var email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim();

            if (!PatronNombreUsuario.IsMatch(nombreUsuario))
                throw new ArgumentException("El nombre de usuario debe tener entre 3 y 50 caracteres: letras, números, punto, guion o guion bajo.");

            if (nombreCompleto.Length == 0)
                throw new ArgumentException("El nombre completo es obligatorio.");

            if (email != null && !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email))
                throw new ArgumentException("El correo electrónico no es válido.");

            // Solo se consulta si el nombre de usuario realmente cambió.
            if (!string.Equals(usuario.NombreUsuario, nombreUsuario, StringComparison.Ordinal) &&
                !await NombreUsuarioDisponible(nombreUsuario, idUsuario))
                throw new ArgumentException("Ya existe otro usuario con ese nombre de usuario.");

            usuario.NombreUsuario = nombreUsuario;
            usuario.NombreCompleto = nombreCompleto;
            usuario.Email = email;

            try
            {
                await _repo.Update(usuario);
            }
            catch (DbUpdateException)
            {
                // Otra cuenta tomó el mismo nombre justo entre la validación y el guardado
                // (el índice UNIQUE de la tabla Usuario lo impide).
                throw new ArgumentException("Ya existe otro usuario con ese nombre de usuario.");
            }
        }

        public async Task CambiarPassword(int idUsuario, CambiarPasswordDTO dto)
        {
            var usuario = await _repo.GetById(idUsuario)
                ?? throw new ArgumentException("El usuario no fue encontrado.");

            if (string.IsNullOrEmpty(dto.PasswordActual) || string.IsNullOrEmpty(dto.PasswordNueva))
                throw new ArgumentException("Debes indicar la contraseña actual y la nueva.");

            if (!PasswordHasher.Verify(dto.PasswordActual, usuario.PasswordHash))
                throw new ArgumentException("La contraseña actual no es correcta.");

            if (dto.PasswordNueva.Length < LongitudMinimaPassword)
                throw new ArgumentException($"La nueva contraseña debe tener al menos {LongitudMinimaPassword} caracteres.");

            if (dto.PasswordNueva == dto.PasswordActual)
                throw new ArgumentException("La nueva contraseña debe ser distinta de la actual.");

            // Aquí solo cambia el hash: el nombre de usuario no se lee del cliente ni se valida.
            usuario.PasswordHash = PasswordHasher.Hash(dto.PasswordNueva);
            await _repo.Update(usuario);
        }

        // "Eliminar" un usuario es desactivarlo (soft delete): conserva su historial
        // como mecánico/autorizador en órdenes y reparaciones pasadas, pero ya no
        // podrá iniciar sesión (AuthService rechaza usuarios inactivos).
        public async Task Delete(int id)
        {
            var usuario = await _repo.GetById(id)
                ?? throw new ArgumentException("El usuario no fue encontrado.");

            if (!usuario.Activo)
                throw new InvalidOperationException("El usuario ya está inactivo.");

            usuario.Activo = false;
            await _repo.Update(usuario);
        }
    }
}
