namespace TallerMecanicoAPI.Models
{
    public class Reparacion
    {
        public int IdReparacion { get; set; }
        public int IdDiagnostico { get; set; }
        public Diagnostico? Diagnostico { get; set; }
        public string Descripcion { get; set; }
        public int IdEstadoReparacion { get; set; } = 1;
        public EstadoReparacion? Estado { get; set; }
        public int? IdAutorizadoPor { get; set; }
        public Usuario? AutorizadoPor { get; set; }
        public DateTime? FechaAutorizacion { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public int Orden { get; set; } = 1;
        public int? IdReparacionPrevia { get; set; }
        public decimal? CostoManoObra { get; set; }
    }
}
