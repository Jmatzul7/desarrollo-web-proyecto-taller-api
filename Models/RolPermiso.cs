namespace TallerMecanicoAPI.Models
{
    // Permiso asignado a un ROL completo: lo heredan todos los usuarios de ese rol.
    public class RolPermiso
    {
        public int IdRol { get; set; }
        public int IdPermiso { get; set; }
    }
}
