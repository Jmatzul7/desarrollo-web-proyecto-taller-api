namespace TallerMecanicoAPI.DTOs
{
    public class UsuarioDTOCreate
    {
        public string NombreUsuario { get; set; }
        public string Password { get; set; }
        public string NombreCompleto { get; set; }
        public string? Email { get; set; }
        public int IdRol { get; set; }
    }
}
