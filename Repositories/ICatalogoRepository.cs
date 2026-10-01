namespace TallerMecanicoAPI.Repositories
{
    // Validaciones livianas de existencia contra las tablas de catálogo
    // (Rol, EstadoOrden, EstadoReparacion), usadas por los Services antes de
    // insertar/actualizar para devolver un 400 claro en vez de un error de FK.
    public interface ICatalogoRepository
    {
        Task<bool> RolExiste(int idRol);
        Task<bool> EstadoOrdenExiste(int idEstadoOrden);
        Task<bool> EstadoReparacionExiste(int idEstadoReparacion);
    }
}
