namespace TallerMecanicoAPI.DTOs
{
    public class FacturaDetalleDTO
    {
        public int IdFactura { get; set; }
        public DateTime FechaEmision { get; set; }
        public string Estado { get; set; } = "";
        public decimal CostoManoObra { get; set; }
        public decimal CostoRepuestos { get; set; }
        public decimal Total { get; set; }

        public int IdOrden { get; set; }
        public DateTime FechaIngreso { get; set; }

        public string ClienteNombre { get; set; } = "";
        public string? ClienteTelefono { get; set; }
        public string? ClienteEmail { get; set; }
        public string? ClienteDireccion { get; set; }

        public string VehiculoPlaca { get; set; } = "";
        public string VehiculoMarca { get; set; } = "";
        public string VehiculoModelo { get; set; } = "";
        public short? VehiculoAnio { get; set; }

        public string MecanicoNombre { get; set; } = "";

        public List<FacturaDetalleReparacionDTO> Reparaciones { get; set; } = new();
    }

    public class FacturaDetalleReparacionDTO
    {
        public string Descripcion { get; set; } = "";
        public decimal CostoManoObra { get; set; }
        public List<FacturaDetalleRepuestoDTO> Repuestos { get; set; } = new();
    }

    public class FacturaDetalleRepuestoDTO
    {
        public string Nombre { get; set; } = "";
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal => Cantidad * PrecioUnitario;
    }
}
