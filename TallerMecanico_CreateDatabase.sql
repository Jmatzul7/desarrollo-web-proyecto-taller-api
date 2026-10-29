/* ============================================================
   PROYECTO II - Sistema de Gestión de Taller Mecánico
   Script de creación de base de datos para SQL Server Management Studio
   ============================================================
   Ejecutar completo (F5) en una nueva conexión de SSMS.
   ============================================================ */

IF DB_ID('TallerMecanico') IS NOT NULL
BEGIN
    ALTER DATABASE TallerMecanico SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE TallerMecanico;
END
GO

CREATE DATABASE TallerMecanico;
GO

USE TallerMecanico;
GO

/* ============================================================
   1. SEGURIDAD: ROLES, USUARIOS Y PERMISOS
   ============================================================ */

CREATE TABLE Rol (
    IdRol        INT IDENTITY(1,1) PRIMARY KEY,
    Nombre       NVARCHAR(50)  NOT NULL UNIQUE,
    Descripcion  NVARCHAR(200) NULL,
    Activo       BIT           NOT NULL DEFAULT 1
);
GO

CREATE TABLE Usuario (
    IdUsuario       INT IDENTITY(1,1) PRIMARY KEY,
    NombreUsuario   NVARCHAR(50)  NOT NULL UNIQUE,
    PasswordHash    NVARCHAR(256) NOT NULL,
    NombreCompleto  NVARCHAR(150) NOT NULL,
    Email           NVARCHAR(150) NULL,
    IdRol           INT           NOT NULL,
    Activo          BIT           NOT NULL DEFAULT 1,
    FechaCreacion   DATETIME2     NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Usuario_Rol FOREIGN KEY (IdRol) REFERENCES Rol(IdRol)
);
GO

-- Permisos individuales para usuarios cuyo caso no se ajusta a un rol predefinido
CREATE TABLE Permiso (
    IdPermiso    INT IDENTITY(1,1) PRIMARY KEY,
    Nombre       NVARCHAR(100) NOT NULL UNIQUE,
    Descripcion  NVARCHAR(250) NULL
);
GO

CREATE TABLE UsuarioPermiso (
    IdUsuario  INT NOT NULL,
    IdPermiso  INT NOT NULL,
    CONSTRAINT PK_UsuarioPermiso PRIMARY KEY (IdUsuario, IdPermiso),
    CONSTRAINT FK_UsuarioPermiso_Usuario FOREIGN KEY (IdUsuario) REFERENCES Usuario(IdUsuario),
    CONSTRAINT FK_UsuarioPermiso_Permiso FOREIGN KEY (IdPermiso) REFERENCES Permiso(IdPermiso)
);
GO

-- Permisos asignados a un ROL completo (los heredan todos sus usuarios).
CREATE TABLE RolPermiso (
    IdRol      INT NOT NULL,
    IdPermiso  INT NOT NULL,
    CONSTRAINT PK_RolPermiso PRIMARY KEY (IdRol, IdPermiso),
    CONSTRAINT FK_RolPermiso_Rol FOREIGN KEY (IdRol) REFERENCES Rol(IdRol),
    CONSTRAINT FK_RolPermiso_Permiso FOREIGN KEY (IdPermiso) REFERENCES Permiso(IdPermiso)
);
GO

/* ============================================================
   2. CLIENTES Y VEHÍCULOS
   ============================================================ */

CREATE TABLE Cliente (
    IdCliente   INT IDENTITY(1,1) PRIMARY KEY,
    Nombre      NVARCHAR(150) NOT NULL,
    Telefono    NVARCHAR(20)  NULL,
    Email       NVARCHAR(150) NULL,
    Direccion   NVARCHAR(250) NULL,
    Activo      BIT           NOT NULL DEFAULT 1
);
GO

CREATE TABLE Vehiculo (
    IdVehiculo  INT IDENTITY(1,1) PRIMARY KEY,
    Placa       NVARCHAR(15)  NOT NULL UNIQUE,
    Marca       NVARCHAR(50)  NOT NULL,
    Modelo      NVARCHAR(50)  NOT NULL,
    Anio        SMALLINT      NULL,
    Color       NVARCHAR(30)  NULL,
    IdCliente   INT           NOT NULL,
    Activo      BIT           NOT NULL DEFAULT 1,
    CONSTRAINT FK_Vehiculo_Cliente FOREIGN KEY (IdCliente) REFERENCES Cliente(IdCliente)
);
GO

/* ============================================================
   3. ÓRDENES DE TRABAJO Y DIAGNÓSTICOS
   ============================================================ */

CREATE TABLE EstadoOrden (
    IdEstadoOrden  INT IDENTITY(1,1) PRIMARY KEY,
    Nombre         NVARCHAR(50) NOT NULL UNIQUE
);
GO

CREATE TABLE OrdenTrabajo (
    IdOrden        INT IDENTITY(1,1) PRIMARY KEY,
    IdVehiculo     INT       NOT NULL,
    IdMecanico     INT       NOT NULL,   -- Usuario con rol Mecánico asignado como responsable
    FechaIngreso   DATETIME2 NOT NULL DEFAULT GETDATE(),
    FechaSalida    DATETIME2 NULL,
    IdEstadoOrden  INT       NOT NULL DEFAULT 1,
    Observaciones  NVARCHAR(500) NULL,
    CONSTRAINT FK_Orden_Vehiculo  FOREIGN KEY (IdVehiculo)    REFERENCES Vehiculo(IdVehiculo),
    CONSTRAINT FK_Orden_Mecanico  FOREIGN KEY (IdMecanico)    REFERENCES Usuario(IdUsuario),
    CONSTRAINT FK_Orden_Estado    FOREIGN KEY (IdEstadoOrden) REFERENCES EstadoOrden(IdEstadoOrden)
);
GO

CREATE TABLE Diagnostico (
    IdDiagnostico     INT IDENTITY(1,1) PRIMARY KEY,
    IdOrden           INT NOT NULL,
    IdMecanico        INT NOT NULL,
    DescripcionFalla  NVARCHAR(500) NOT NULL,
    FechaDiagnostico  DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Diagnostico_Orden    FOREIGN KEY (IdOrden)    REFERENCES OrdenTrabajo(IdOrden),
    CONSTRAINT FK_Diagnostico_Mecanico FOREIGN KEY (IdMecanico) REFERENCES Usuario(IdUsuario)
);
GO

/* ============================================================
   4. REPARACIONES (con autorización y dependencias entre etapas)
   ============================================================ */

CREATE TABLE EstadoReparacion (
    IdEstadoReparacion  INT IDENTITY(1,1) PRIMARY KEY,
    Nombre              NVARCHAR(50) NOT NULL UNIQUE
);
GO

CREATE TABLE Reparacion (
    IdReparacion         INT IDENTITY(1,1) PRIMARY KEY,
    IdDiagnostico        INT NOT NULL,
    Descripcion          NVARCHAR(500) NOT NULL,
    IdEstadoReparacion   INT NOT NULL DEFAULT 1,       -- 1 = Pendiente
    IdAutorizadoPor      INT NULL,                     -- Usuario que autoriza la reparación
    FechaAutorizacion    DATETIME2 NULL,
    FechaInicio          DATETIME2 NULL,
    FechaFin             DATETIME2 NULL,
    Orden                INT NOT NULL DEFAULT 1,       -- posición dentro de la secuencia de trabajo
    IdReparacionPrevia   INT NULL,                     -- dependencia: reparación que debe finalizar antes
    CostoManoObra        DECIMAL(10,2) NULL,
    CONSTRAINT FK_Reparacion_Diagnostico FOREIGN KEY (IdDiagnostico)      REFERENCES Diagnostico(IdDiagnostico),
    CONSTRAINT FK_Reparacion_Estado      FOREIGN KEY (IdEstadoReparacion) REFERENCES EstadoReparacion(IdEstadoReparacion),
    CONSTRAINT FK_Reparacion_Autoriza    FOREIGN KEY (IdAutorizadoPor)    REFERENCES Usuario(IdUsuario),
    CONSTRAINT FK_Reparacion_Previa      FOREIGN KEY (IdReparacionPrevia) REFERENCES Reparacion(IdReparacion)
);
GO

/* ============================================================
   5. BODEGA / REPUESTOS
   ============================================================ */

CREATE TABLE Repuesto (
    IdRepuesto      INT IDENTITY(1,1) PRIMARY KEY,
    Codigo          NVARCHAR(30)  NOT NULL UNIQUE,
    Nombre          NVARCHAR(150) NOT NULL,
    Descripcion     NVARCHAR(300) NULL,
    Existencia      INT           NOT NULL DEFAULT 0 CHECK (Existencia >= 0),
    PrecioUnitario  DECIMAL(10,2) NOT NULL,
    Activo          BIT           NOT NULL DEFAULT 1
);
GO

-- Detalle de repuestos usados en una reparación: exige justificación y autorización de bodega
CREATE TABLE ReparacionRepuesto (
    IdReparacionRepuesto  INT IDENTITY(1,1) PRIMARY KEY,
    IdReparacion          INT NOT NULL,
    IdRepuesto            INT NOT NULL,
    Cantidad              INT NOT NULL CHECK (Cantidad > 0),
    PrecioUnitario        DECIMAL(10,2) NOT NULL,     -- precio congelado al momento del uso
    Justificacion         NVARCHAR(300) NOT NULL,
    IdAutorizadoPor       INT NOT NULL,               -- usuario de bodega que autoriza la salida
    FechaUso              DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_RepRep_Reparacion FOREIGN KEY (IdReparacion)     REFERENCES Reparacion(IdReparacion),
    CONSTRAINT FK_RepRep_Repuesto   FOREIGN KEY (IdRepuesto)       REFERENCES Repuesto(IdRepuesto),
    CONSTRAINT FK_RepRep_Autoriza   FOREIGN KEY (IdAutorizadoPor)  REFERENCES Usuario(IdUsuario)
);
GO

/* ============================================================
   6. FACTURACIÓN
   ============================================================ */

CREATE TABLE Factura (
    IdFactura        INT IDENTITY(1,1) PRIMARY KEY,
    IdOrden          INT NOT NULL UNIQUE,     -- una factura por orden de trabajo
    FechaEmision     DATETIME2 NOT NULL DEFAULT GETDATE(),
    CostoManoObra    DECIMAL(10,2) NOT NULL DEFAULT 0,
    CostoRepuestos   DECIMAL(10,2) NOT NULL DEFAULT 0,
    Total            AS (CostoManoObra + CostoRepuestos) PERSISTED,
    Estado           NVARCHAR(20) NOT NULL DEFAULT 'Emitida',
    CONSTRAINT FK_Factura_Orden FOREIGN KEY (IdOrden) REFERENCES OrdenTrabajo(IdOrden)
);
GO

/* ============================================================
   7. DATOS INICIALES (catálogos y usuario administrador)
   ============================================================ */

INSERT INTO Rol (Nombre, Descripcion) VALUES
    (N'Administrador', N'Control total del sistema'),
    (N'Recepcionista', N'Registra clientes, vehículos y órdenes de trabajo'),
    (N'Mecanico',      N'Diagnostica y ejecuta reparaciones'),
    (N'Bodeguero',     N'Controla existencias y autoriza salida de repuestos');
GO

INSERT INTO EstadoOrden (Nombre) VALUES
    (N'Recibido'), (N'En Diagnostico'), (N'En Reparacion'), (N'Finalizada'), (N'Facturada'), (N'Cancelada');
GO

INSERT INTO EstadoReparacion (Nombre) VALUES
    (N'Pendiente'), (N'Autorizada'), (N'Rechazada'), (N'En Proceso'), (N'Finalizada');
GO

-- Usuario administrador inicial
-- Usuario: admin   /   Contraseña: Admin123!
-- (hash PBKDF2-SHA256 compatible con Helpers/PasswordHasher.cs de la API)
INSERT INTO Usuario (NombreUsuario, PasswordHash, NombreCompleto, Email, IdRol)
VALUES (N'admin', N'100000.KYRFLMZFTczCr37O+bTgaQ==.oCf2H0nRYdCIvnFKJTw73fVc3pivGzfHE/UngKRx/Bc=',
        N'Administrador del Sistema', N'admin@taller.com', 1);
GO

/* ============================================================
   8. DATOS DE PRUEBA (para todas las tablas relacionadas)
   Contraseña para todos los usuarios de prueba: Taller123!
   ============================================================ */

-- ---- Usuarios adicionales (uno por cada rol) ----
INSERT INTO Usuario (NombreUsuario, PasswordHash, NombreCompleto, Email, IdRol) VALUES
    (N'recepcion1', N'100000.nvUIFzetByUBxOlWg/xlVw==.jE0OjKghcHlEn/mEMW7BgeXC1aeSmdMSwllrAPaez+w=',
        N'María López', N'maria.lopez@taller.com', 2),
    (N'mecanico1',  N'100000.2xC6zGp8uqOowQ5NsTeEsA==.ALS8WwihCmYO+VGtgPX0jWhcb2gTKlVDq4MYRDaiXlI=',
        N'Carlos Ramírez', N'carlos.ramirez@taller.com', 3),
    (N'mecanico2',  N'100000.Hy/vPYj9zPegffXHlxhIOQ==.NZbp478R0ydnrma0GneftrwZDzCV4J6Ensep1j030CQ=',
        N'Luis Fernández', N'luis.fernandez@taller.com', 3),
    (N'bodega1',    N'100000.qC0LHXSQAEFDTDYnfajFNA==.o50f/L5eQ2iUfzI7ob8VUhzR8GYDBnZsL230kC0+5Cc=',
        N'Ana Gómez', N'ana.gomez@taller.com', 4);
GO
-- IdUsuario resultante: 1=admin, 2=recepcion1, 3=mecanico1, 4=mecanico2, 5=bodega1

-- ---- Permisos granulares ----
-- Catálogo de permisos finos, uno por cada acción protegida de la API (ver
-- Helpers/RequierePermisoAttribute.cs). Un usuario puede tener su rol normal
-- Y además alguno de estos permisos sueltos, para casos que no encajan en
-- los 4 roles fijos (Administrador, Recepcionista, Mecanico, Bodeguero).
INSERT INTO Permiso (Nombre, Descripcion) VALUES
    (N'clientes.crear',              N'Crear clientes'),
    (N'clientes.editar',             N'Editar clientes'),
    (N'clientes.eliminar',           N'Desactivar clientes'),
    (N'vehiculos.crear',             N'Crear vehículos'),
    (N'vehiculos.editar',            N'Editar vehículos'),
    (N'vehiculos.eliminar',          N'Desactivar vehículos'),
    (N'ordenes.crear',               N'Crear órdenes de trabajo'),
    (N'ordenes.cambiar_estado',      N'Cambiar el estado de una orden de trabajo'),
    (N'ordenes.eliminar',            N'Eliminar órdenes de trabajo'),
    (N'diagnosticos.crear',          N'Registrar diagnósticos'),
    (N'diagnosticos.eliminar',       N'Eliminar diagnósticos'),
    (N'reparaciones.crear',          N'Registrar reparaciones'),
    (N'reparaciones.autorizar',      N'Autorizar o rechazar una reparación pendiente'),
    (N'reparaciones.cambiar_estado', N'Avanzar el estado de una reparación'),
    (N'reparaciones.eliminar',       N'Eliminar reparaciones'),
    (N'reparaciones.asignar_repuesto', N'Asignar repuestos a una reparación'),
    (N'reparaciones.quitar_repuesto',  N'Quitar repuestos asignados a una reparación'),
    (N'repuestos.crear',             N'Crear repuestos en bodega'),
    (N'repuestos.editar',            N'Editar repuestos'),
    (N'repuestos.entrada_stock',     N'Registrar entrada de mercadería a bodega'),
    (N'repuestos.eliminar',          N'Desactivar repuestos'),
    (N'facturas.generar',            N'Generar la factura de una orden de trabajo'),
    -- Permisos de visibilidad / consulta (IdPermiso 23 en adelante)
    (N'clientes.ver',                N'Ver el listado y el detalle de clientes'),
    (N'vehiculos.ver',               N'Ver el listado y el detalle de vehículos'),
    (N'repuestos.ver',               N'Ver el inventario de repuestos de la bodega'),
    (N'facturas.ver',                N'Ver el listado y el detalle de facturas'),
    (N'facturas.descargar_pdf',      N'Descargar el PDF de una factura'),
    (N'usuarios.ver_mecanicos',      N'Ver la lista de mecánicos para asignar órdenes de trabajo'),
    -- Alcance de las órdenes: sin este permiso un Mecánico solo ve las que tiene asignadas
    (N'ordenes.ver_todas',           N'Ver todas las órdenes de trabajo (no solo las asignadas al usuario)'),
    (N'ordenes.reasignar',           N'Reasignar el mecánico de una orden de trabajo');
GO

-- Ejemplo real del caso de uso: María (Recepcionista, IdUsuario=2) recibe el
-- permiso puntual "reparaciones.autorizar" (IdPermiso=13), que normalmente es
-- exclusivo de Administrador. Con esto puede autorizar reparaciones sin que
-- haya que cambiarle el rol.
INSERT INTO UsuarioPermiso (IdUsuario, IdPermiso) VALUES (2, 13);
GO

-- ---- Clientes ----
INSERT INTO Cliente (Nombre, Telefono, Email, Direccion) VALUES
    (N'Juan Pérez',     N'5555-1234', N'juan.perez@correo.com',    N'Zona 10, Ciudad de Guatemala'),
    (N'Sofía Castillo',  N'5555-5678', N'sofia.castillo@correo.com', N'Zona 15, Ciudad de Guatemala'),
    (N'Mario Díaz',      N'5555-9012', N'mario.diaz@correo.com',    N'Mixco, Guatemala');
GO
-- IdCliente resultante: 1=Juan, 2=Sofía, 3=Mario

-- ---- Vehículos ----
INSERT INTO Vehiculo (Placa, Marca, Modelo, Anio, Color, IdCliente) VALUES
    (N'P123ABC', N'Toyota', N'Corolla', 2020, N'Gris',  1),
    (N'P456DEF', N'Mazda',  N'3',       2019, N'Rojo',  2),
    (N'P789GHI', N'Honda',  N'Civic',   2021, N'Negro', 3);
GO
-- IdVehiculo resultante: 1, 2, 3 (mismo orden)

-- ---- Órdenes de trabajo ----
-- 1: ya facturada (flujo completo) | 2: en reparación (con dependencia entre etapas) | 3: recién recibida
INSERT INTO OrdenTrabajo (IdVehiculo, IdMecanico, IdEstadoOrden, Observaciones, FechaSalida) VALUES
    (1, 3, 5, N'El vehículo no enciende', GETDATE()),
    (2, 4, 3, N'Ruido metálico al frenar', NULL),
    (3, 3, 1, N'Cambio de aceite y revisión general', NULL);
GO
-- IdOrden resultante: 1, 2, 3

-- ---- Diagnósticos ----
INSERT INTO Diagnostico (IdOrden, IdMecanico, DescripcionFalla) VALUES
    (1, 3, N'Batería descargada y terminales oxidados'),
    (2, 4, N'Pastillas de freno desgastadas y disco delantero rayado'),
    (3, 3, N'Cambio de aceite y filtro según kilometraje programado');
GO
-- IdDiagnostico resultante: 1, 2, 3

-- ---- Reparaciones ----
-- Reparacion 1 (diagnostico 1): finalizada y ya autorizada por el admin
INSERT INTO Reparacion (IdDiagnostico, Descripcion, IdEstadoReparacion, IdAutorizadoPor, FechaAutorizacion, FechaInicio, FechaFin, Orden, CostoManoObra)
VALUES (1, N'Cambio de batería y limpieza de terminales', 5, 1, DATEADD(DAY,-3,GETDATE()), DATEADD(DAY,-3,GETDATE()), DATEADD(DAY,-2,GETDATE()), 1, 150.00);
GO

-- Reparacion 2 (diagnostico 2): primera etapa, ya finalizada
INSERT INTO Reparacion (IdDiagnostico, Descripcion, IdEstadoReparacion, IdAutorizadoPor, FechaAutorizacion, FechaInicio, FechaFin, Orden, CostoManoObra)
VALUES (2, N'Cambio de pastillas de freno delanteras', 5, 1, DATEADD(DAY,-2,GETDATE()), DATEADD(DAY,-2,GETDATE()), DATEADD(DAY,-1,GETDATE()), 1, 200.00);
GO

-- Reparacion 3 (diagnostico 2): depende de la Reparacion 2 (IdReparacionPrevia). Ya autorizada, en espera de iniciar.
INSERT INTO Reparacion (IdDiagnostico, Descripcion, IdEstadoReparacion, IdAutorizadoPor, FechaAutorizacion, Orden, IdReparacionPrevia, CostoManoObra)
VALUES (2, N'Rectificado del disco de freno delantero', 2, 1, DATEADD(DAY,-1,GETDATE()), 2, 2, 120.00);
GO

-- Reparacion 4 (diagnostico 3): pendiente de autorización
INSERT INTO Reparacion (IdDiagnostico, Descripcion, IdEstadoReparacion, Orden, CostoManoObra)
VALUES (3, N'Cambio de aceite de motor y filtro', 1, 1, 80.00);
GO
-- IdReparacion resultante: 1, 2, 3, 4

-- ---- Repuestos (bodega) ----
-- La Existencia ya refleja el stock restante después del uso registrado abajo
INSERT INTO Repuesto (Codigo, Nombre, Descripcion, Existencia, PrecioUnitario) VALUES
    (N'BAT-001',  N'Batería 12V 45Ah',              N'Batería de arranque estándar',        9,  450.00),
    (N'PAST-002', N'Pastillas de freno delanteras',  N'Juego de pastillas cerámicas',        14, 180.00),
    (N'DISC-005', N'Disco de freno delantero',       N'Disco ventilado',                     7,  320.00),
    (N'ACE-003',  N'Aceite de motor 5W-30 (litro)',  N'Aceite sintético',                    30, 65.00),
    (N'FIL-004',  N'Filtro de aceite',               N'Filtro estándar',                     20, 35.00);
GO
-- IdRepuesto resultante: 1=Batería, 2=Pastillas, 3=Disco, 4=Aceite, 5=Filtro

-- ---- Repuestos usados en cada reparación (requieren justificación y autorización de bodega) ----
INSERT INTO ReparacionRepuesto (IdReparacion, IdRepuesto, Cantidad, PrecioUnitario, Justificacion, IdAutorizadoPor) VALUES
    (1, 1, 1, 450.00, N'Batería agotada según diagnóstico, se reemplaza por una nueva', 5),
    (2, 2, 1, 180.00, N'Pastillas por debajo del espesor mínimo de seguridad', 5);
GO
-- Nota: la Reparacion 3 (rectificado de disco) aún no tiene repuesto asignado porque
-- está esperando a que finalice su reparación previa (dependencia entre etapas).

-- ---- Factura ----
-- Solo la Orden 1 tiene todas sus reparaciones finalizadas, por lo que ya se facturó.
INSERT INTO Factura (IdOrden, CostoManoObra, CostoRepuestos, Estado) VALUES
    (1, 150.00, 450.00, N'Emitida');
GO

PRINT 'Base de datos TallerMecanico creada y poblada correctamente.';
GO
