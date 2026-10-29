using TallerMecanicoAPI.Models;

namespace TallerMecanicoAPI.Helpers
{
    // Reglas del flujo Orden de trabajo -> Diagnóstico -> Reparación -> Factura.
    // Centraliza los estados (según el seed de EstadoOrden / EstadoReparacion) y las
    // validaciones, para que todos los servicios apliquen las mismas reglas.
    public static class FlujoTrabajo
    {
        // EstadoOrden
        public const int OrdenRecibido = 1;
        public const int OrdenEnDiagnostico = 2;
        public const int OrdenEnReparacion = 3;
        public const int OrdenFinalizada = 4;
        public const int OrdenFacturada = 5;
        public const int OrdenCancelada = 6;

        // EstadoReparacion
        public const int RepPendiente = 1;
        public const int RepAutorizada = 2;
        public const int RepRechazada = 3;
        public const int RepEnProceso = 4;
        public const int RepFinalizada = 5;

        public static bool OrdenCerrada(int idEstadoOrden) =>
            idEstadoOrden == OrdenFacturada || idEstadoOrden == OrdenCancelada;

        /// <summary>Una orden Facturada o Cancelada ya no admite cambios en su trabajo.</summary>
        public static void ExigirOrdenAbierta(OrdenTrabajo orden, string accion)
        {
            if (orden.IdEstadoOrden == OrdenFacturada)
                throw new InvalidOperationException($"No se puede {accion}: la orden ya fue facturada.");
            if (orden.IdEstadoOrden == OrdenCancelada)
                throw new InvalidOperationException($"No se puede {accion}: la orden está cancelada.");
        }

        /// <summary>
        /// Transiciones de reparación permitidas al cambiar de estado:
        /// Autorizada -> En Proceso -> Finalizada. (Pendiente -> Autorizada/Rechazada se hace
        /// únicamente con la acción de autorizar; Rechazada y Finalizada son estados finales.)
        /// </summary>
        public static bool TransicionReparacionValida(int desde, int hasta) =>
            (desde == RepAutorizada && hasta == RepEnProceso) ||
            (desde == RepEnProceso && hasta == RepFinalizada);

        /// <summary>
        /// Estado que le corresponde a la orden según sus reparaciones, o null si no debe cambiar.
        /// Las reparaciones rechazadas no cuentan. Todas finalizadas -> Finalizada; alguna iniciada
        /// o terminada -> En Reparación; si se quedó sin trabajo iniciado vuelve a En Diagnóstico.
        /// </summary>
        public static int? EstadoOrdenCalculado(int estadoActual, IEnumerable<int> estadosReparaciones)
        {
            if (OrdenCerrada(estadoActual)) return null;

            var activas = estadosReparaciones.Where(e => e != RepRechazada).ToList();
            int? nuevo = null;

            if (activas.Count > 0 && activas.All(e => e == RepFinalizada))
                nuevo = OrdenFinalizada;
            else if (activas.Any(e => e == RepEnProceso || e == RepFinalizada))
                nuevo = OrdenEnReparacion;
            else if (estadoActual == OrdenEnReparacion || estadoActual == OrdenFinalizada)
                nuevo = OrdenEnDiagnostico;

            return nuevo.HasValue && nuevo.Value != estadoActual ? nuevo : null;
        }

        /// <summary>Motivo por el que no se puede facturar, o null si sí se puede.</summary>
        public static string? MotivoNoFacturable(int estadoOrden, IReadOnlyCollection<int> estadosReparaciones)
        {
            if (estadoOrden == OrdenFacturada) return "Esta orden ya fue facturada.";
            if (estadoOrden == OrdenCancelada) return "No se puede facturar una orden cancelada.";

            var sinFinalizar = estadosReparaciones.Count(e =>
                e == RepPendiente || e == RepAutorizada || e == RepEnProceso);
            if (sinFinalizar > 0)
                return $"No se puede facturar: hay {sinFinalizar} reparación(es) sin finalizar " +
                       "(pendientes, autorizadas o en proceso). Finalízalas o recházalas antes de facturar.";

            if (!estadosReparaciones.Any(e => e == RepFinalizada))
                return "No hay reparaciones finalizadas para esta orden; no se puede facturar aún.";

            return null;
        }
    }
}
