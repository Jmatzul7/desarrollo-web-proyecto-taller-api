/* ============================================================================
   TallerMecanico - Migración: permiso "ordenes.ver_todas"
   Ejecutar UNA vez sobre la base de datos EXISTENTE (no borra nada).
   Es idempotente: si el permiso ya existe, no lo vuelve a insertar.
   ============================================================================ */
USE TallerMecanico;
GO

INSERT INTO Permiso (Nombre, Descripcion)
SELECT N'ordenes.ver_todas', N'Ver todas las órdenes de trabajo (no solo las asignadas al usuario)'
WHERE NOT EXISTS (SELECT 1 FROM Permiso WHERE Nombre = N'ordenes.ver_todas');
GO

INSERT INTO Permiso (Nombre, Descripcion)
SELECT N'ordenes.reasignar', N'Reasignar el mecánico de una orden de trabajo'
WHERE NOT EXISTS (SELECT 1 FROM Permiso WHERE Nombre = N'ordenes.reasignar');
GO

SELECT IdPermiso, Nombre, Descripcion FROM Permiso WHERE Nombre IN (N'ordenes.ver_todas', N'ordenes.reasignar');
GO
