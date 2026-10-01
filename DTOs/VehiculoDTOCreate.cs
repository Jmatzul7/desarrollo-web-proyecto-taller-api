namespace TallerMecanicoAPI.DTOs
{
    public class VehiculoDTOCreate
    {
        public string Placa { get; set; }
        public string Marca { get; set; }
        public string Modelo { get; set; }
        public short? Anio { get; set; }
        public string? Color { get; set; }
        public int IdCliente { get; set; }
    }
}
