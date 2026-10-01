using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TallerMecanicoAPI.DTOs;

namespace TallerMecanicoAPI.Helpers
{
    public static class FacturaPdfGenerator
    {
        public static byte[] Generar(FacturaDetalleDTO f)
        {
            var documento = Document.Create(container =>
            {
                container.Page(page =>
                {
                    // =========================================================
                    // CONFIGURACIÓN DE PÁGINA
                    // =========================================================

                    page.Size(PageSizes.A4);
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);

                    page.DefaultTextStyle(text =>
                        text.FontSize(9)
                            .FontColor(Colors.Grey.Darken3));

                    // =========================================================
                    // HEADER
                    // =========================================================

                    page.Header()
                        .Column(header =>
                        {
                            header.Item()
                                .Row(row =>
                                {
                                    // Nombre del taller
                                    row.RelativeItem()
                                        .Column(info =>
                                        {
                                            info.Item()
                                                .Text("TALLER MECÁNICO")
                                                .FontSize(22)
                                                .Bold()
                                                .FontColor(Colors.Blue.Darken3);

                                            info.Item()
                                                .PaddingTop(3)
                                                .Text("Servicio y mantenimiento automotriz")
                                                .FontSize(9)
                                                .FontColor(Colors.Grey.Darken1);
                                        });

                                    // Datos de factura
                                    row.ConstantItem(190)
                                        .AlignRight()
                                        .Column(info =>
                                        {
                                            info.Item()
                                                .Text("FACTURA")
                                                .FontSize(20)
                                                .Bold()
                                                .FontColor(Colors.Blue.Darken3);

                                            info.Item()
                                                .PaddingTop(3)
                                                .Text($"No. {f.IdFactura:D6}")
                                                .FontSize(11)
                                                .SemiBold();

                                            info.Item()
                                                .PaddingTop(2)
                                                .Text(
                                                    $"Fecha: {f.FechaEmision:dd/MM/yyyy HH:mm}"
                                                )
                                                .FontSize(8);

                                            var estado =
                                                string.IsNullOrWhiteSpace(f.Estado)
                                                    ? "PENDIENTE"
                                                    : f.Estado.ToUpper();

                                            var colorEstado =
                                                estado == "PAGADA"
                                                    ? Colors.Green.Lighten3
                                                    : Colors.Orange.Lighten3;

                                            info.Item()
                                                .PaddingTop(4)
                                                .AlignRight()
                                                .Background(colorEstado)
                                                .PaddingHorizontal(8)
                                                .PaddingVertical(4)
                                                .Text(estado)
                                                .Bold()
                                                .FontSize(8);
                                        });
                                });

                            header.Item()
                                .PaddingTop(10)
                                .LineHorizontal(2)
                                .LineColor(Colors.Blue.Darken3);
                        });

                    // =========================================================
                    // CONTENT
                    // =========================================================

                    page.Content()
                        .PaddingTop(15)
                        .Column(content =>
                        {
                            content.Spacing(10);

                            // =================================================
                            // CLIENTE / VEHÍCULO
                            // =================================================

                            content.Item()
                                .Row(row =>
                                {
                                    // -------------------------
                                    // CLIENTE
                                    // -------------------------

                                    row.RelativeItem()
                                        .Background(Colors.Grey.Lighten4)
                                        .Padding(8)
                                        .Column(cliente =>
                                        {
                                            cliente.Item()
                                                .Text("DATOS DEL CLIENTE")
                                                .FontSize(10)
                                                .Bold()
                                                .FontColor(Colors.Blue.Darken3);

                                            cliente.Item()
                                                .PaddingTop(5)
                                                .Text($"Nombre: {f.ClienteNombre}")
                                                .SemiBold();

                                            if (!string.IsNullOrWhiteSpace(
                                                    f.ClienteTelefono))
                                            {
                                                cliente.Item()
                                                    .PaddingTop(2)
                                                    .Text(
                                                        $"Teléfono: {f.ClienteTelefono}"
                                                    );
                                            }

                                            if (!string.IsNullOrWhiteSpace(
                                                    f.ClienteEmail))
                                            {
                                                cliente.Item()
                                                    .PaddingTop(2)
                                                    .Text(
                                                        $"Email: {f.ClienteEmail}"
                                                    );
                                            }

                                            if (!string.IsNullOrWhiteSpace(
                                                    f.ClienteDireccion))
                                            {
                                                cliente.Item()
                                                    .PaddingTop(2)
                                                    .Text(
                                                        $"Dirección: {f.ClienteDireccion}"
                                                    );
                                            }
                                        });

                                    row.ConstantItem(10);

                                    // -------------------------
                                    // VEHÍCULO
                                    // -------------------------

                                    row.RelativeItem()
                                        .Background(Colors.Grey.Lighten4)
                                        .Padding(8)
                                        .Column(vehiculo =>
                                        {
                                            vehiculo.Item()
                                                .Text("DATOS DEL VEHÍCULO")
                                                .FontSize(10)
                                                .Bold()
                                                .FontColor(Colors.Blue.Darken3);

                                            vehiculo.Item()
                                                .PaddingTop(5)
                                                .Text($"Placa: {f.VehiculoPlaca}")
                                                .SemiBold();

                                            var vehiculoTexto =
                                                $"Vehículo: {f.VehiculoMarca} {f.VehiculoModelo}";

                                            if (f.VehiculoAnio.HasValue)
                                            {
                                                vehiculoTexto +=
                                                    $" ({f.VehiculoAnio.Value})";
                                            }

                                            vehiculo.Item()
                                                .PaddingTop(2)
                                                .Text(vehiculoTexto);

                                            vehiculo.Item()
                                                .PaddingTop(2)
                                                .Text(
                                                    $"Mecánico: {f.MecanicoNombre}"
                                                );

                                            vehiculo.Item()
                                                .PaddingTop(2)
                                                .Text(
                                                    $"Orden de trabajo: #{f.IdOrden}"
                                                );

                                            vehiculo.Item()
                                                .PaddingTop(2)
                                                .Text(
                                                    $"Ingreso: {f.FechaIngreso:dd/MM/yyyy}"
                                                );
                                        });
                                });

                            // =================================================
                            // DETALLE
                            // =================================================

                            content.Item()
                                .PaddingTop(5)
                                .Text("DETALLE DE SERVICIOS Y REPUESTOS")
                                .FontSize(12)
                                .Bold()
                                .FontColor(Colors.Blue.Darken3);

                            content.Item()
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(4);
                                        columns.RelativeColumn(0.8f);
                                        columns.RelativeColumn(1.4f);
                                        columns.RelativeColumn(1.4f);
                                    });

                                    // -------------------------
                                    // HEADER DE TABLA
                                    // -------------------------

                                    table.Header(header =>
                                    {
                                        header.Cell()
                                            .Background(Colors.Blue.Darken3)
                                            .Padding(6)
                                            .Text("Descripción")
                                            .Bold()
                                            .FontColor(Colors.White);

                                        header.Cell()
                                            .Background(Colors.Blue.Darken3)
                                            .Padding(6)
                                            .AlignCenter()
                                            .Text("Cant.")
                                            .Bold()
                                            .FontColor(Colors.White);

                                        header.Cell()
                                            .Background(Colors.Blue.Darken3)
                                            .Padding(6)
                                            .AlignRight()
                                            .Text("Precio Unit.")
                                            .Bold()
                                            .FontColor(Colors.White);

                                        header.Cell()
                                            .Background(Colors.Blue.Darken3)
                                            .Padding(6)
                                            .AlignRight()
                                            .Text("Subtotal")
                                            .Bold()
                                            .FontColor(Colors.White);
                                    });

                                    // -------------------------
                                    // REPARACIONES
                                    // -------------------------

                                    foreach (var reparacion in f.Reparaciones)
                                    {
                                        // Mano de obra
                                        table.Cell()
                                            .Background(Colors.Grey.Lighten3)
                                            .Padding(5)
                                            .Text(reparacion.Descripcion)
                                            .SemiBold();

                                        table.Cell()
                                            .Background(Colors.Grey.Lighten3)
                                            .Padding(5)
                                            .AlignCenter()
                                            .Text("1")
                                            .SemiBold();

                                        table.Cell()
                                            .Background(Colors.Grey.Lighten3)
                                            .Padding(5)
                                            .AlignRight()
                                            .Text(
                                                $"Q {reparacion.CostoManoObra:0.00}"
                                            )
                                            .SemiBold();

                                        table.Cell()
                                            .Background(Colors.Grey.Lighten3)
                                            .Padding(5)
                                            .AlignRight()
                                            .Text(
                                                $"Q {reparacion.CostoManoObra:0.00}"
                                            )
                                            .SemiBold();

                                        // -------------------------
                                        // REPUESTOS
                                        // -------------------------

                                        foreach (var repuesto in reparacion.Repuestos)
                                        {
                                            table.Cell()
                                                .PaddingLeft(14)
                                                .PaddingVertical(4)
                                                .Text(
                                                    $"• {repuesto.Nombre}"
                                                );

                                            table.Cell()
                                                .PaddingVertical(4)
                                                .AlignCenter()
                                                .Text(
                                                    repuesto.Cantidad.ToString()
                                                );

                                            table.Cell()
                                                .PaddingVertical(4)
                                                .AlignRight()
                                                .Text(
                                                    $"Q {repuesto.PrecioUnitario:0.00}"
                                                );

                                            table.Cell()
                                                .PaddingVertical(4)
                                                .AlignRight()
                                                .Text(
                                                    $"Q {repuesto.Subtotal:0.00}"
                                                );
                                        }
                                    }
                                });

                            // =================================================
                            // TOTALES
                            // =================================================

                            content.Item()
                                .PaddingTop(12)
                                .AlignRight()
                                .Column(totales =>
                                {
                                    totales.Item()
                                        .Width(250)
                                        .Row(row =>
                                        {
                                            row.RelativeItem()
                                                .Text("Mano de obra:")
                                                .SemiBold();

                                            row.ConstantItem(90)
                                                .AlignRight()
                                                .Text(
                                                    $"Q {f.CostoManoObra:0.00}"
                                                );
                                        });

                                    totales.Item()
                                        .Width(250)
                                        .PaddingTop(4)
                                        .Row(row =>
                                        {
                                            row.RelativeItem()
                                                .Text("Repuestos:")
                                                .SemiBold();

                                            row.ConstantItem(90)
                                                .AlignRight()
                                                .Text(
                                                    $"Q {f.CostoRepuestos:0.00}"
                                                );
                                        });

                                    totales.Item()
                                        .Width(250)
                                        .PaddingTop(7)
                                        .LineHorizontal(1)
                                        .LineColor(Colors.Grey.Medium);

                                    totales.Item()
                                        .Width(250)
                                        .PaddingTop(7)
                                        .Background(Colors.Blue.Darken3)
                                        .Padding(9)
                                        .Row(row =>
                                        {
                                            row.RelativeItem()
                                                .Text("TOTAL")
                                                .FontSize(13)
                                                .Bold()
                                                .FontColor(Colors.White);

                                            row.ConstantItem(110)
                                                .AlignRight()
                                                .Text(
                                                    $"Q {f.Total:0.00}"
                                                )
                                                .FontSize(13)
                                                .Bold()
                                                .FontColor(Colors.White);
                                        });
                                });

                            // =================================================
                            // MENSAJE
                            // =================================================

                            content.Item()
                                .PaddingTop(20)
                                .AlignCenter()
                                .Text("Gracias por confiar en nuestro taller.")
                                .Italic()
                                .FontSize(9)
                                .FontColor(Colors.Grey.Darken1);
                        });

                    // =========================================================
                    // FOOTER
                    // =========================================================

                    page.Footer()
                        .PaddingTop(8)
                        .Column(footer =>
                        {
                            footer.Item()
                                .LineHorizontal(1)
                                .LineColor(Colors.Grey.Lighten1);

                            footer.Item()
                                .PaddingTop(5)
                                .Row(row =>
                                {
                                    row.RelativeItem()
                                        .Text(
                                            "Taller Mecánico • Factura electrónica"
                                        )
                                        .FontSize(7)
                                        .FontColor(Colors.Grey.Darken1);

                                    row.RelativeItem()
                                        .AlignRight()
                                        .Text(text =>
                                        {
                                            text.Span("Página ")
                                                .FontSize(7);

                                            text.CurrentPageNumber()
                                                .FontSize(7);

                                            text.Span(" de ")
                                                .FontSize(7);

                                            text.TotalPages()
                                                .FontSize(7);
                                        });
                                });
                        });
                });
            });

            return documento.GeneratePdf();
        }
    }
}