namespace TallerMecanicoAPI.DTOs
{
    public class UsuarioDTOUpdate
    {
        public string NombreCompleto { get; set; }
        public string? Email { get; set; }
        public int IdRol { get; set; }
        public bool Activo { get; set; }
    }
}
