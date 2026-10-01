namespace TallerMecanicoAPI.Models
{
    public class Diagnostico
    {
        public int IdDiagnostico { get; set; }
        public int IdOrden { get; set; }
        public OrdenTrabajo? Orden { get; set; }
        public int IdMecanico { get; set; }
        public Usuario? Mecanico { get; set; }
        public string DescripcionFalla { get; set; }
        public DateTime FechaDiagnostico { get; set; } = DateTime.Now;
    }
}
