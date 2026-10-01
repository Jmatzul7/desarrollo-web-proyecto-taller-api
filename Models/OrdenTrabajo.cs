namespace TallerMecanicoAPI.Models
{
    public class OrdenTrabajo
    {
        public int IdOrden { get; set; }
        public int IdVehiculo { get; set; }
        public Vehiculo? Vehiculo { get; set; }
        public int IdMecanico { get; set; }
        public Usuario? Mecanico { get; set; }
        public DateTime FechaIngreso { get; set; } = DateTime.Now;
        public DateTime? FechaSalida { get; set; }
        public int IdEstadoOrden { get; set; } = 1;
        public EstadoOrden? Estado { get; set; }
        public string? Observaciones { get; set; }
    }
}
