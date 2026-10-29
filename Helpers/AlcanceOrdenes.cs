using System.Security.Claims;
using TallerMecanicoAPI.Services;

namespace TallerMecanicoAPI.Helpers
{
    // Alcance de visibilidad de las órdenes de trabajo (y de su diagnóstico / reparaciones).
    //
    // Por defecto un Mecánico solo ve las órdenes que tiene ASIGNADAS (OrdenTrabajo.IdMecanico).
    // Ven todas las órdenes: los roles de la lista RolesPorDefecto, o cualquier usuario que
    // tenga el permiso "ordenes.ver_todas" (asignado a él o a su rol, ver /api/permisos).
    public static class AlcanceOrdenes
    {
        public const string Permiso = "ordenes.ver_todas";
        public const string RolesPorDefecto = "Administrador,Recepcionista";

        public static bool PuedeVerTodas(ClaimsPrincipal usuario)
        {
            var roles = RolesPorDefecto.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return roles.Any(usuario.IsInRole) ||
                   usuario.Claims.Any(c => c.Type == "permiso" && c.Value == Permiso);
        }

        /// <summary>Id del mecánico al que se limita la vista, o null si el usuario ve todas las órdenes.</summary>
        public static int? IdMecanicoRestringido(ClaimsPrincipal usuario)
        {
            if (PuedeVerTodas(usuario)) return null;
            return int.TryParse(usuario.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : -1;
        }

        /// <summary>
        /// ¿Puede el usuario acceder a esta orden? Si la orden no existe devuelve true para que
        /// el servicio responda con su propio mensaje de "no encontrada".
        /// </summary>
        public static async Task<bool> PuedeAccederAsync(ClaimsPrincipal usuario, IOrdenTrabajoService ordenes, int idOrden)
        {
            var restringido = IdMecanicoRestringido(usuario);
            if (restringido == null) return true;

            var orden = await ordenes.GetById(idOrden);
            return orden == null || orden.IdMecanico == restringido;
        }
    }
}
