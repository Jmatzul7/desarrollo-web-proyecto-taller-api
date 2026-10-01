namespace TallerMecanicoAPI.DTOs
{
    public class VehiculoDTOUpdate
    {
        public string Marca { get; set; }
        public string Modelo { get; set; }
        public short? Anio { get; set; }
        public string? Color { get; set; }
        public bool Activo { get; set; }
    }
}
