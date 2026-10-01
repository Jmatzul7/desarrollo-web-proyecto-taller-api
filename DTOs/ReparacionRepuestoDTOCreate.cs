namespace TallerMecanicoAPI.DTOs
{
    // Registrar uso/salida de un repuesto en una reparación: exige justificación
    public class ReparacionRepuestoDTOCreate
    {
        public int IdRepuesto { get; set; }
        public int Cantidad { get; set; }
        public string Justificacion { get; set; }
    }
}
