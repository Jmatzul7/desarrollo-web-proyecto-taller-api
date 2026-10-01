namespace TallerMecanicoAPI.Models
{
    public class Factura
    {
        public int IdFactura { get; set; }
        public int IdOrden { get; set; }
        public OrdenTrabajo? Orden { get; set; }
        public DateTime FechaEmision { get; set; } = DateTime.Now;
        public decimal CostoManoObra { get; set; }
        public decimal CostoRepuestos { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; } = "Emitida";
    }
}
