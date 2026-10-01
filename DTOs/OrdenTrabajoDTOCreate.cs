namespace TallerMecanicoAPI.DTOs
{
    public class OrdenTrabajoDTOCreate
    {
        public int IdVehiculo { get; set; }
        public int IdMecanico { get; set; }
        public string? Observaciones { get; set; }
    }
}
