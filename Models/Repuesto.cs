namespace TallerMecanicoAPI.Models
{
    public class Repuesto
    {
        public int IdRepuesto { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string? Descripcion { get; set; }
        public int Existencia { get; set; } = 0;
        public decimal PrecioUnitario { get; set; }
        public bool Activo { get; set; } = true;
    }
}
