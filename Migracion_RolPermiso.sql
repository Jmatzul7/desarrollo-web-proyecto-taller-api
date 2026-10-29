/* ============================================================================
   TallerMecanico - Migración: permisos asignados a un ROL
   Ejecutar UNA vez sobre la base de datos EXISTENTE (no borra nada).
   Es idempotente: si la tabla ya existe, no la vuelve a crear.
   ============================================================================ */
USE TallerMecanico;
GO

IF OBJECT_ID(N'dbo.RolPermiso', N'U') IS NULL
BEGIN
    CREATE TABLE RolPermiso (
        IdRol     INT NOT NULL,
        IdPermiso INT NOT NULL,
        CONSTRAINT PK_RolPermiso PRIMARY KEY (IdRol, IdPermiso),
        CONSTRAINT FK_RolPermiso_Rol FOREIGN KEY (IdRol) REFERENCES Rol(IdRol),
        CONSTRAINT FK_RolPermiso_Permiso FOREIGN KEY (IdPermiso) REFERENCES Permiso(IdPermiso)
    );
END
GO

SELECT r.Nombre AS Rol, p.Nombre AS Permiso
FROM RolPermiso rp
JOIN Rol r ON r.IdRol = rp.IdRol
JOIN Permiso p ON p.IdPermiso = rp.IdPermiso;
GO
