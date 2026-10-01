namespace TallerMecanicoAPI.DTOs
{
    // No expone PasswordHash
    public class UsuarioDTOResponse
    {
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; }
        public string NombreCompleto { get; set; }
        public string? Email { get; set; }
        public int IdRol { get; set; }
        public string? Rol { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
