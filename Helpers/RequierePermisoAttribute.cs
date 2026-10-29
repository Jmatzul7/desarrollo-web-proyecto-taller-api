using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TallerMecanicoAPI.Helpers
{
    // Autoriza la acción si el usuario autenticado cumple AL MENOS UNA de dos condiciones:
    //   1) su Rol está dentro de "Roles" (igual que [Authorize(Roles = "...")]), o
    //   2) tiene el permiso granular "permiso", sin importar su rol: ya sea asignado
    //      a él (tabla UsuarioPermiso) o a su rol (tabla RolPermiso).
    //
    // Esto permite, por ejemplo, que un Recepcionista puntual autorice reparaciones
    // sin tener que convertirlo en Administrador: basta con asignarle el permiso
    // "reparaciones.autorizar" desde /api/permisos.
    //
    // Nota: los permisos viajan como claims dentro del JWT (se cargan en el login),
    // así que si le asignas o le quitas un permiso a un usuario que ya tiene una
    // sesión activa, el cambio se refleja hasta que vuelva a iniciar sesión.
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class RequierePermisoAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _permiso;
        public string Roles { get; set; } = "";

        // Nombre del permiso (se expone para poder listar los permisos por defecto de cada rol).
        public string Permiso => _permiso;

        public RequierePermisoAttribute(string permiso) => _permiso = permiso;

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var usuario = context.HttpContext.User;

            if (usuario.Identity == null || !usuario.Identity.IsAuthenticated)
            {
                context.Result = new Microsoft.AspNetCore.Mvc.UnauthorizedObjectResult(
                    new { mensaje = "No autenticado." });
                return;
            }

            var rolesPermitidos = Roles
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var tieneRolPermitido = rolesPermitidos.Length > 0 &&
                rolesPermitidos.Any(r => usuario.IsInRole(r));

            var tienePermisoGranular = usuario.Claims
                .Any(c => c.Type == "permiso" && c.Value == _permiso);

            if (!tieneRolPermitido && !tienePermisoGranular)
            {
                context.Result = new Microsoft.AspNetCore.Mvc.ObjectResult(
                    new { mensaje = $"No tienes permiso para realizar esta acción (se requiere el rol {Roles} o el permiso '{_permiso}')." })
                {
                    StatusCode = 403
                };
            }
        }
    }
}
