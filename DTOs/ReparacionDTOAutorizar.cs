namespace TallerMecanicoAPI.DTOs
{
    // El IdAutorizadoPor se toma del usuario autenticado (token), no del body
    public class ReparacionDTOAutorizar
    {
        public bool Aprobar { get; set; }
    }
}
