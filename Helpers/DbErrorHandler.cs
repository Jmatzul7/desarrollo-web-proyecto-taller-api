using Microsoft.EntityFrameworkCore;

namespace TallerMecanicoAPI.Helpers
{
    // Envuelve SaveChangesAsync para convertir una violación de llave foránea
    // (otro registro depende del que se intenta eliminar) en un mensaje de negocio
    // claro, en vez de dejar pasar la excepción cruda de SQL Server.
    public static class DbErrorHandler
    {
        public static async Task GuardarOManejarConflicto(Func<Task> guardar, string entidad)
        {
            try
            {
                await guardar();
            }
            catch (DbUpdateException)
            {
                throw new InvalidOperationException(
                    $"No se puede eliminar este {entidad} porque existen otros registros que dependen de él.");
            }
        }
    }
}
