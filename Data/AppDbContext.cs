using Microsoft.EntityFrameworkCore;
using TallerMecanicoAPI.Models;

namespace TallerMecanicoAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Rol> Roles { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Permiso> Permisos { get; set; }
        public DbSet<UsuarioPermiso> UsuarioPermisos { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Vehiculo> Vehiculos { get; set; }
        public DbSet<EstadoOrden> EstadosOrden { get; set; }
        public DbSet<OrdenTrabajo> OrdenesTrabajo { get; set; }
        public DbSet<Diagnostico> Diagnosticos { get; set; }
        public DbSet<EstadoReparacion> EstadosReparacion { get; set; }
        public DbSet<Reparacion> Reparaciones { get; set; }
        public DbSet<Repuesto> Repuestos { get; set; }
        public DbSet<ReparacionRepuesto> ReparacionRepuestos { get; set; }
        public DbSet<Factura> Facturas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Rol>(e =>
            {
                e.ToTable("Rol");
                e.HasKey(x => x.IdRol);
                e.HasIndex(x => x.Nombre).IsUnique();
            });

            modelBuilder.Entity<Usuario>(e =>
            {
                e.ToTable("Usuario");
                e.HasKey(x => x.IdUsuario);
                e.HasIndex(x => x.NombreUsuario).IsUnique();
                e.HasOne(x => x.Rol).WithMany().HasForeignKey(x => x.IdRol);
            });

            modelBuilder.Entity<Permiso>(e =>
            {
                e.ToTable("Permiso");
                e.HasKey(x => x.IdPermiso);
            });

            modelBuilder.Entity<UsuarioPermiso>(e =>
            {
                e.ToTable("UsuarioPermiso");
                e.HasKey(x => new { x.IdUsuario, x.IdPermiso });
            });

            modelBuilder.Entity<Cliente>(e =>
            {
                e.ToTable("Cliente");
                e.HasKey(x => x.IdCliente);
            });

            modelBuilder.Entity<Vehiculo>(e =>
            {
                e.ToTable("Vehiculo");
                e.HasKey(x => x.IdVehiculo);
                e.HasIndex(x => x.Placa).IsUnique();
                e.HasOne(x => x.Cliente).WithMany().HasForeignKey(x => x.IdCliente);
            });

            modelBuilder.Entity<EstadoOrden>(e =>
            {
                e.ToTable("EstadoOrden");
                e.HasKey(x => x.IdEstadoOrden);
            });

            modelBuilder.Entity<OrdenTrabajo>(e =>
            {
                e.ToTable("OrdenTrabajo");
                e.HasKey(x => x.IdOrden);
                e.HasOne(x => x.Vehiculo).WithMany().HasForeignKey(x => x.IdVehiculo);
                e.HasOne(x => x.Mecanico).WithMany().HasForeignKey(x => x.IdMecanico);
                e.HasOne(x => x.Estado).WithMany().HasForeignKey(x => x.IdEstadoOrden);
            });

            modelBuilder.Entity<Diagnostico>(e =>
            {
                e.ToTable("Diagnostico");
                e.HasKey(x => x.IdDiagnostico);
                e.HasOne(x => x.Orden).WithMany().HasForeignKey(x => x.IdOrden);
                e.HasOne(x => x.Mecanico).WithMany().HasForeignKey(x => x.IdMecanico);
            });

            modelBuilder.Entity<EstadoReparacion>(e =>
            {
                e.ToTable("EstadoReparacion");
                e.HasKey(x => x.IdEstadoReparacion);
            });

            modelBuilder.Entity<Reparacion>(e =>
            {
                e.ToTable("Reparacion");
                e.HasKey(x => x.IdReparacion);
                e.HasOne(x => x.Diagnostico).WithMany().HasForeignKey(x => x.IdDiagnostico);
                e.HasOne(x => x.Estado).WithMany().HasForeignKey(x => x.IdEstadoReparacion);
                e.HasOne(x => x.AutorizadoPor).WithMany().HasForeignKey(x => x.IdAutorizadoPor);
                e.HasOne<Reparacion>().WithMany().HasForeignKey(x => x.IdReparacionPrevia).OnDelete(DeleteBehavior.NoAction);
                e.Property(x => x.CostoManoObra).HasColumnType("decimal(10,2)");
            });

            modelBuilder.Entity<Repuesto>(e =>
            {
                e.ToTable("Repuesto");
                e.HasKey(x => x.IdRepuesto);
                e.HasIndex(x => x.Codigo).IsUnique();
                e.Property(x => x.PrecioUnitario).HasColumnType("decimal(10,2)");
            });

            modelBuilder.Entity<ReparacionRepuesto>(e =>
            {
                e.ToTable("ReparacionRepuesto");
                e.HasKey(x => x.IdReparacionRepuesto);
                e.HasOne<Reparacion>().WithMany().HasForeignKey(x => x.IdReparacion);
                e.HasOne(x => x.Repuesto).WithMany().HasForeignKey(x => x.IdRepuesto);
                e.HasOne<Usuario>().WithMany().HasForeignKey(x => x.IdAutorizadoPor);
                e.Property(x => x.PrecioUnitario).HasColumnType("decimal(10,2)");
            });

            modelBuilder.Entity<Factura>(e =>
            {
                e.ToTable("Factura");
                e.HasKey(x => x.IdFactura);
                e.HasIndex(x => x.IdOrden).IsUnique();
                e.HasOne(x => x.Orden).WithMany().HasForeignKey(x => x.IdOrden);
                e.Property(x => x.CostoManoObra).HasColumnType("decimal(10,2)");
                e.Property(x => x.CostoRepuestos).HasColumnType("decimal(10,2)");
                e.Property(x => x.Total)
                    .HasColumnType("decimal(10,2)")
                    .HasComputedColumnSql("[CostoManoObra] + [CostoRepuestos]", stored: true);
            });
        }
    }
}
