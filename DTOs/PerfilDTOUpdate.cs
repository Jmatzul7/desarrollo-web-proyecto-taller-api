namespace TallerMecanicoAPI.DTOs
{
    // Datos que el propio usuario puede cambiar de su perfil.
    // No incluye rol, estado activo ni contraseña: esos se manejan por otras vías.
    public class PerfilDTOUpdate
    {
        public string NombreUsuario { get; set; }
        public string NombreCompleto { get; set; }
        public string? Email { get; set; }
    }
}
