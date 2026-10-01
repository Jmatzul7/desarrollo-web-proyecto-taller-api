namespace TallerMecanicoAPI.DTOs
{
    public class ClienteDTOUpdate
    {
        public string Nombre { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public string? Direccion { get; set; }
        public bool Activo { get; set; }
    }
}
