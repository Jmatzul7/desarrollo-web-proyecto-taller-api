using System.Text.Json.Serialization;

namespace TallerMecanicoAPI.Models
{
    public class Usuario
    {
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; }

        [JsonIgnore]
        public string PasswordHash { get; set; }
        public string NombreCompleto { get; set; }
        public string? Email { get; set; }
        public int IdRol { get; set; }
        public Rol? Rol { get; set; }
        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
    }
}
