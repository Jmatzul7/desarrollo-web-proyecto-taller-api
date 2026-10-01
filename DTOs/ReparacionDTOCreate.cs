namespace TallerMecanicoAPI.DTOs
{
    public class ReparacionDTOCreate
    {
        public int IdDiagnostico { get; set; }
        public string Descripcion { get; set; }
        public int Orden { get; set; } = 1;
        public int? IdReparacionPrevia { get; set; }
        public decimal? CostoManoObra { get; set; }
    }
}
