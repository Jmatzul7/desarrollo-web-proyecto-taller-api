namespace TallerMecanicoAPI.Models
{
    public class Vehiculo
    {
        public int IdVehiculo { get; set; }
        public string Placa { get; set; }
        public string Marca { get; set; }
        public string Modelo { get; set; }
        public short? Anio { get; set; }
        public string? Color { get; set; }
        public int IdCliente { get; set; }
        public Cliente? Cliente { get; set; }
        public bool Activo { get; set; } = true;
    }
}
