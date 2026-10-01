namespace TallerMecanicoAPI.Models
{
    public class ReparacionRepuesto
    {
        public int IdReparacionRepuesto { get; set; }
        public int IdReparacion { get; set; }
        public int IdRepuesto { get; set; }
        public Repuesto? Repuesto { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public string Justificacion { get; set; }
        public int IdAutorizadoPor { get; set; }
        public DateTime FechaUso { get; set; } = DateTime.Now;
    }
}
