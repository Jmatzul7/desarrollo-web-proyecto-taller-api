namespace TallerMecanicoAPI.DTOs
{
    public class LoginResponseDTO
    {
        public string Token { get; set; }
        public DateTime Expira { get; set; }
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; }
        public string NombreCompleto { get; set; }
        public string Rol { get; set; }
    }
}
