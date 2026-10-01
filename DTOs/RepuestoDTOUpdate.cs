namespace TallerMecanicoAPI.DTOs
{
    public class RepuestoDTOUpdate
    {
        public string Nombre { get; set; }
        public string? Descripcion { get; set; }
        public decimal PrecioUnitario { get; set; }
        public bool Activo { get; set; }
    }
}
