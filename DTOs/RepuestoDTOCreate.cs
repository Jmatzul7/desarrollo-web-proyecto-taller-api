namespace TallerMecanicoAPI.DTOs
{
    public class RepuestoDTOCreate
    {
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string? Descripcion { get; set; }
        public int Existencia { get; set; }
        public decimal PrecioUnitario { get; set; }
    }
}
