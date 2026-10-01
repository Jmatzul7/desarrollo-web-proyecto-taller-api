namespace TallerMecanicoAPI.DTOs
{
    public class DiagnosticoDTOCreate
    {
        public int IdOrden { get; set; }
        public int IdMecanico { get; set; }
        public string DescripcionFalla { get; set; }
    }
}
