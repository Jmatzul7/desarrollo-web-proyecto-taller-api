/* ============================================================================
   TallerMecanico - Migración: nuevos permisos granulares
   Ejecutar UNA vez sobre la base de datos EXISTENTE (no borra nada).
   Es idempotente: si un permiso ya existe, no lo vuelve a insertar.
   ============================================================================ */
USE TallerMecanico;
GO

INSERT INTO Permiso (Nombre, Descripcion)
SELECT v.Nombre, v.Descripcion
FROM (VALUES
    (N'clientes.ver',           N'Ver el listado y el detalle de clientes'),
    (N'vehiculos.ver',          N'Ver el listado y el detalle de vehículos'),
    (N'repuestos.ver',          N'Ver el inventario de repuestos de la bodega'),
    (N'facturas.ver',           N'Ver el listado y el detalle de facturas'),
    (N'facturas.descargar_pdf', N'Descargar el PDF de una factura'),
    (N'usuarios.ver_mecanicos', N'Ver la lista de mecánicos para asignar órdenes de trabajo')
) AS v(Nombre, Descripcion)
WHERE NOT EXISTS (SELECT 1 FROM Permiso p WHERE p.Nombre = v.Nombre);
GO

SELECT IdPermiso, Nombre, Descripcion FROM Permiso ORDER BY IdPermiso;
GO
