# TallerMecanicoAPI - Jonathan Matzul

API REST (.NET / C#) para la gestión de un taller mecánico — Proyecto II.

Cubre todo el ciclo de un vehículo que entra al taller: recepción, diagnóstico,
reparaciones (con autorización y dependencias entre etapas), control de repuestos
en bodega, facturación y generación de la factura en PDF — todo protegido con
autenticación JWT y roles.

---

## 1. Arquitectura

Capas, siguiendo el mismo patrón en todo el proyecto:

```
Controllers/   -> Endpoints HTTP (reciben el DTO, llaman al Service, devuelven el código HTTP)
Services/      -> Lógica de negocio y validaciones (interfaz + implementación)
Repositories/  -> Acceso a datos con Entity Framework (interfaz + implementación EF)
Data/          -> AppDbContext (mapeo de entidades a tablas)
Models/        -> Entidades de dominio (reflejan las tablas de la BD)
DTOs/          -> Objetos de entrada/salida de la API (lo que viaja en el JSON)
Helpers/       -> JWT, hashing de contraseñas, manejo de errores de BD, generación de PDF
```

NOTA: un Controller nunca toca `AppDbContext` ni una tabla directamente
(salvo `CatalogosController`, que solo lee catálogos). Toda regla de negocio vive
en el Service correspondiente.

---

## 2. Entidades y para qué sirve cada una

| Tabla | Para qué sirve |
|---|---|
| `Rol` | Catálogo: Administrador, Recepcionista, Mecanico, Bodeguero |
| `Usuario` | Cuentas del sistema (login), cada una con un `Rol` |
| `Permiso` / `UsuarioPermiso` | Permisos especiales sueltos, para un usuario cuyo caso no encaja en su rol |
| `Cliente` | Dueño de uno o más vehículos |
| `Vehiculo` | Pertenece a un `Cliente` |
| `EstadoOrden` | Catálogo: Recibido, En Diagnostico, En Reparacion, Finalizada, Facturada, Cancelada |
| `OrdenTrabajo` | El "caso" de un vehículo en el taller: vehículo + mecánico asignado + estado |
| `Diagnostico` | Falla encontrada en una `OrdenTrabajo`, registrada por un mecánico |
| `EstadoReparacion` | Catálogo: Pendiente, Autorizada, Rechazada, En Proceso, Finalizada |
| `Reparacion` | Una acción correctiva sobre un `Diagnostico`. Requiere autorización y puede depender de otra `Reparacion` (`IdReparacionPrevia`) |
| `Repuesto` | Inventario de bodega (código, existencia, precio) |
| `ReparacionRepuesto` | Repuesto usado en una `Reparacion`: exige justificación y descuenta stock |
| `Factura` | Se genera a partir de una `OrdenTrabajo` ya con reparaciones finalizadas |

Diagrama de relaciones (generado con el script `TallerMecanico_CreateDatabase.sql`):

```
Cliente 1---N Vehiculo 1---N OrdenTrabajo N---1 Usuario(Mecanico)
                                  |                    |
                                  1                    N (autoriza)
                                  N                     
                             Diagnostico 1---N Reparacion --(previa)--> Reparacion
                                                   |
                                                   N
                                          ReparacionRepuesto N---1 Repuesto
OrdenTrabajo 1---1 Factura
Usuario N---1 Rol
Usuario N---N Permiso (vía UsuarioPermiso)
```

---

## 3. Roles y qué puede hacer cada uno

| Rol | Puede hacer |
|---|---|
| **Administrador** | Todo: gestionar usuarios, autorizar/rechazar reparaciones, eliminar cualquier registro |
| **Recepcionista** | Clientes, vehículos, crear/actualizar-estado de órdenes de trabajo, generar facturas |
| **Mecanico** | Diagnósticos, crear reparaciones y avanzar su estado (Pendiente → En Proceso → Finalizada) |
| **Bodeguero** | Gestionar repuestos (alta, entrada de stock) y autorizar su salida hacia una reparación |

Todo endpoint sin `[AllowAnonymous]` requiere `Authorization: Bearer {token}`.
Los `GET` de clientes, vehículos, repuestos y facturas están protegidos por
permisos de **visibilidad** (`*.ver`, ver 3.2); el resto de los `GET` (órdenes,
diagnósticos, reparaciones, catálogos) siguen abiertos a cualquier usuario
autenticado. Los `POST`/`PUT`/`PATCH`/`DELETE` están restringidos según la tabla
de arriba (ver el detalle en cada endpoint más abajo).

### 3.1 Permisos granulares (además del rol)

Cada acción protegida tiene, además de su lista de roles, un **permiso granular**
con nombre `recurso.accion` (ej. `reparaciones.autorizar`, `clientes.crear`). Un
usuario puede desbloquear una acción puntual sin cambiarle el rol, asignándole
ese permiso individual — útil para casos que no encajan en los 4 roles fijos
(ej. "que esta Recepcionista en particular también pueda autorizar
reparaciones").

Se implementa con el atributo `[RequierePermiso("permiso", Roles = "...")]`
(`Helpers/RequierePermisoAttribute.cs`), que reemplaza a `[Authorize(Roles=...)]`
en los endpoints de escritura: pasa si el usuario tiene uno de los roles **o**
el permiso específico. La gestión de usuarios (`UsuariosController`) queda
deliberadamente fuera de este esquema — sigue siendo 100% exclusiva de
Administrador, para que nadie pueda auto-otorgarse más acceso.

Los permisos del usuario se cargan como claims (`"permiso"`) dentro del JWT en
el momento del login — si le asignas o le quitas un permiso a alguien que ya
tiene sesión activa, el cambio aplica hasta que vuelva a loguearse.

### 3.2 Permisos de visibilidad y refresco de la sesión

Además de los permisos de acción (`crear`, `editar`, ...), hay permisos de **ver**
que controlan qué secciones de la aplicación aparecen. Se aplican igual que los
demás: pasa quien tenga uno de los roles **o** el permiso.

| Permiso | Roles que lo tienen por defecto | Qué habilita |
|---|---|---|
| `clientes.ver` | Administrador, Recepcionista | `GET /api/clientes` y `/{id}` (menú Clientes) |
| `vehiculos.ver` | Administrador, Recepcionista | `GET /api/vehiculos` y `/{id}` (menú Vehículos) |
| `repuestos.ver` | Administrador, Bodeguero, Mecanico | `GET /api/repuestos` y `/{id}` (menú Repuestos / Bodega) |
| `facturas.ver` | Administrador, Recepcionista | `GET /api/facturas` y `/{id}` (menú Facturas) |
| `facturas.descargar_pdf` | Administrador, Recepcionista | `GET /api/facturas/{id}/pdf` (botón "Descargar PDF") |
| `usuarios.ver_mecanicos` | Administrador, Recepcionista, Mecanico | `GET /api/catalogos/mecanicos` (lista para asignar mecánicos) |

Ejemplo: a `bodega1` (Bodeguero) no le aparece el menú Clientes. Si un Administrador
le asigna `clientes.ver`, en pocos segundos le aparece el menú y puede listar clientes.

**Refresco de permisos sin cerrar sesión:** `POST /api/auth/refrescar` (requiere
token) vuelve a leer el rol y los permisos desde la base de datos y devuelve un
token nuevo con la misma forma que el login. El frontend lo llama cada ~20 segundos
y al volver a la pestaña, y avisa al usuario cuando le asignan o le quitan un
permiso. Si la cuenta fue desactivada responde `401`.

**Importante al migrar:** ejecutar `Migracion_PermisosNuevos.sql` una vez sobre la
base existente para crear los 6 permisos nuevos.

**Administrar permisos** (`PermisosController`, `/api/permisos`):

| Método | Ruta | Rol | Descripción |
|---|---|---|---|
| GET | `/` | cualquier autenticado | Catálogo completo de permisos |
| GET | `/usuario/{idUsuario}` | Administrador | Permisos asignados a ese usuario |
| POST | `/usuario/{idUsuario}` | Administrador | Asigna un permiso (`{ "idPermiso": N }`) |
| DELETE | `/usuario/{idUsuario}/{idPermiso}` | Administrador | Quita un permiso |

El script SQL siembra el catálogo completo de permisos (uno por cada acción
de la tabla de abajo) y un ejemplo real: a `recepcion1` se le asigna
`reparaciones.autorizar`, así que puede autorizar reparaciones sin ser
Administrador.

---

## 4. Todos los endpoints

### AuthController — `/api/auth`
| Método | Ruta | Rol | Descripción |
|---|---|---|---|
| POST | `/login` | público | Devuelve el JWT |
| POST | `/refrescar` | cualquier autenticado | Reemite el JWT con el rol y los permisos vigentes (ver 3.2) |
| POST | `/registro` | público | Autoregistro; no permite rol Administrador; la cuenta queda `Activo=false` hasta que un Administrador la active |

### UsuariosController — `/api/usuarios` (todo restringido a **Administrador**)
| Método | Ruta | Descripción |
|---|---|---|
| GET | `/` | Lista todos los usuarios |
| GET | `/{id}` | Un usuario |
| POST | `/` | Crea usuario, ya activo (a diferencia de `/api/auth/registro`) |
| PUT | `/{id}` | Actualiza nombre, email, rol, activo |
| DELETE | `/{id}` | Soft delete: pone `Activo=false` (no borra la fila) |

### ClientesController — `/api/clientes`
| Método | Ruta | Rol | Descripción |
|---|---|---|---|
| GET | `/`, `/{id}` | Administrador, Recepcionista (o permiso `clientes.ver`) | Listar / obtener |
| POST | `/` | Administrador, Recepcionista | Crear |
| PUT | `/{id}` | Administrador, Recepcionista | Actualizar |
| DELETE | `/{id}` | Administrador | Soft delete (`Activo=false`) |

### VehiculosController — `/api/vehiculos`
Igual patrón que Clientes (`GET` con permiso `vehiculos.ver`). `POST` valida que `IdCliente` exista y que la `Placa` sea única.

### OrdenesTrabajoController — `/api/ordenestrabajo`
| Método | Ruta | Rol | Descripción |
|---|---|---|---|
| GET | `/`, `/{id}` | cualquiera | Listar / obtener |
| POST | `/` | Administrador, Recepcionista | Crea la orden en estado `Recibido` (1). Valida que el `IdMecanico` tenga rol Mecanico |
| PATCH | `/{id}/estado` | Administrador, Recepcionista, Mecanico | Cambia `IdEstadoOrden` (valida que el id exista en el catálogo) |
| DELETE | `/{id}` | Administrador | Hard delete (falla con `409` si tiene diagnósticos o factura asociada) |

### DiagnosticosController — `/api/diagnosticos`
| Método | Ruta | Rol | Descripción |
|---|---|---|---|
| GET | `/`, `/{id}`, `/orden/{idOrden}` | cualquiera | Listar / obtener / por orden |
| POST | `/` | Administrador, Mecanico | Crea el diagnóstico y pasa la orden a `En Diagnostico` (2). Valida que el mecánico exista y tenga rol Mecanico |
| DELETE | `/{id}` | Administrador | Hard delete (falla con `409` si tiene reparaciones asociadas) |

### ReparacionesController — `/api/reparaciones`
| Método | Ruta | Rol | Descripción |
|---|---|---|---|
| GET | `/`, `/{id}` | cualquiera | Listar / obtener |
| POST | `/` | Administrador, Mecanico | Crea la reparación en estado `Pendiente` (1) |
| PATCH | `/{id}/autorizar` | **Administrador** | Aprueba o rechaza una reparación `Pendiente` → pasa a `Autorizada` (2) o `Rechazada` (3) |
| PATCH | `/{id}/estado` | Administrador, Mecanico | Avanza el estado. Bloquea pasar a `En Proceso` (4) si no está `Autorizada`, o si su `IdReparacionPrevia` no está `Finalizada` (5) |
| DELETE | `/{id}` | Administrador | Hard delete (falla con `409` si tiene repuestos asignados u otra reparación depende de ella) |
| GET | `/{id}/repuestos` | cualquiera | Repuestos usados en esa reparación |
| POST | `/{id}/repuestos` | Administrador, Bodeguero | Registra uso de un repuesto (exige `justificacion`, valida stock y estado de la reparación, descuenta existencia) |
| DELETE | `/repuestos/{idDetalle}` | Administrador, Bodeguero | Revierte el uso y restituye el stock |

### RepuestosController — `/api/repuestos`
| Método | Ruta | Rol | Descripción |
|---|---|---|---|
| GET | `/`, `/{id}` | Administrador, Bodeguero, Mecanico (o permiso `repuestos.ver`) | Listar / obtener |
| POST | `/` | Administrador, Bodeguero | Crea repuesto (valida `Codigo` único, `Existencia >= 0`, `PrecioUnitario > 0`) |
| PUT | `/{id}` | Administrador, Bodeguero | Actualiza nombre/descripción/precio/activo |
| PATCH | `/{id}/entrada` | Administrador, Bodeguero | Suma existencia (entrada de mercadería a bodega) |
| DELETE | `/{id}` | Administrador | Soft delete (`Activo=false`) |

### FacturasController — `/api/facturas`
| Método | Ruta | Rol | Descripción |
|---|---|---|---|
| GET | `/`, `/{id}` | Administrador, Recepcionista (o permiso `facturas.ver`) | Listar / obtener |
| POST | `/` | Administrador, Recepcionista | Genera la factura de una orden (suma mano de obra + repuestos de las reparaciones **Finalizadas**). Falla si la orden ya tiene factura o si no hay reparaciones finalizadas |
| GET | `/{id}/pdf` | Administrador, Recepcionista (o permiso `facturas.descargar_pdf`) | Descarga el PDF de la factura |

No tiene `DELETE` — una factura no debe poder borrarse.

### CatalogosController — `/api/catalogos` (solo lectura, cualquier autenticado)
`GET /roles`, `GET /estados-orden`, `GET /estados-reparacion` — para llenar
listas desplegables en el frontend.

`GET /mecanicos` — mecánicos activos (`idUsuario`, `nombreUsuario`, `nombreCompleto`)
para asignar órdenes sin exponer `/api/usuarios`. Requiere Administrador,
Recepcionista, Mecanico o el permiso `usuarios.ver_mecanicos`.

### PermisosController — `/api/permisos`
Ver el detalle completo en la sección **3.1 Permisos granulares** más arriba.

---

## 5. Flujo end-to-end

Este es el recorrido completo, en el orden en que un caso real avanza por el
sistema. Cada paso indica qué endpoint llamar y qué depende de qué:

1. **Login** — `POST /api/auth/login` con un usuario ya activo (el `admin`, o uno creado por un Administrador). Guardar el `token` y enviarlo
   en `Authorization: Bearer {token}` en todo lo siguiente.

2. **Catálogos (opcional, para UI)** — `GET /api/catalogos/roles`,
   `/estados-orden`, `/estados-reparacion` para poblar selects.

3. **Cliente** — `POST /api/clientes`. Guarda el `idCliente` devuelto.

4. **
1. Vehículo** — `POST /api/vehiculos` con ese `idCliente`. Guarda `idVehiculo`.
   (La placa debe ser única.)

5. **Usuario mecánico** — si no existe uno todavía, un Administrador lo crea con
   `POST /api/usuarios` e `idRol` = Mecanico, o el mecánico se autoregistra con
   `POST /api/auth/registro` y luego un Administrador lo activa con
   `PUT /api/usuarios/{id}` (`activo: true`).

6. **Orden de trabajo** — `POST /api/ordenestrabajo` con `idVehiculo` +
   `idMecanico` (debe tener rol Mecanico). Queda en estado `Recibido`.
   Guarda `idOrden`.

7. **Diagnóstico** — `POST /api/diagnosticos` con `idOrden` + `idMecanico` +
   `descripcionFalla`. La orden pasa automáticamente a `En Diagnostico`.
   Guarda `idDiagnostico`.

8. **Reparación(es)** — `POST /api/reparaciones` con `idDiagnostico` +
   `descripcion` + `costoManoObra`. Si una reparación solo puede hacerse
   después de otra, manda `idReparacionPrevia` con el id de esa otra reparación.
   Queda en estado `Pendiente`. Guarda `idReparacion`.

9. **Autorización** — un **Administrador** llama
   `PATCH /api/reparaciones/{id}/autorizar` con `{ "aprobar": true }`.
   Sin este paso, la reparación no puede avanzar a `En Proceso`.

10. **Iniciar la reparación** — `PATCH /api/reparaciones/{id}/estado` con
    `{ "idEstadoReparacion": 4 }` (En Proceso). Esto falla si:
    - la reparación no está `Autorizada`, o
    - tiene `idReparacionPrevia` y esa reparación previa no está `Finalizada`.

11. **Repuestos (bodega)** — si la reparación necesita repuestos:
    - Asegúrate de que el `Repuesto` exista: `POST /api/repuestos` (si no existe
      aún) o `PATCH /api/repuestos/{id}/entrada` para sumar stock.
    - Un Administrador o Bodeguero llama
      `POST /api/reparaciones/{id}/repuestos` con `idRepuesto`, `cantidad` y
      `justificacion`. Esto valida stock suficiente y lo descuenta.

12. **Finalizar la reparación** — `PATCH /api/reparaciones/{id}/estado` con
    `{ "idEstadoReparacion": 5 }` (Finalizada).

13. **Repetir 8–12** para cada reparación que tenga la orden.

14. **Facturar** — cuando todas las reparaciones que se van a cobrar están
    `Finalizada`, un Administrador o Recepcionista llama
    `POST /api/facturas` con `{ "idOrden": ... }`. Esto:
    - suma `CostoManoObra` de las reparaciones finalizadas,
    - suma `cantidad * precioUnitario` de los repuestos usados en ellas,
    - pasa la orden a estado `Facturada`.
    Falla si la orden ya tiene factura, o si no hay ninguna reparación
    finalizada todavía.

15. **PDF** — `GET /api/facturas/{id}/pdf` descarga el comprobante ya armado
    con cliente, vehículo, detalle de reparaciones/repuestos y el total.

### Diagrama del flujo

```
Cliente → Vehiculo → OrdenTrabajo (Recibido)
                          │
                          ▼
                     Diagnostico  (orden → En Diagnostico)
                          │
                          ▼
                     Reparacion (Pendiente)
                          │
              Administrador autoriza ──► Autorizada / Rechazada
                          │
           (si tiene previa, la previa debe estar Finalizada)
                          │
                          ▼
                     En Proceso ──► se le asignan Repuestos (descuenta stock)
                          │
                          ▼
                     Finalizada
                          │
              (repetir por cada reparación de la orden)
                          │
                          ▼
                     Factura (POST)  ──►  orden pasa a Facturada
                          │
                          ▼
                     PDF (GET /pdf)
```

### Estados — códigos de catálogo (fijos por el seed del script SQL)

**EstadoOrden**: 1 Recibido · 2 En Diagnostico · 3 En Reparacion · 4 Finalizada · 5 Facturada · 6 Cancelada

**EstadoReparacion**: 1 Pendiente · 2 Autorizada · 3 Rechazada · 4 En Proceso · 5 Finalizada

**Rol**: 1 Administrador · 2 Recepcionista · 3 Mecanico · 4 Bodeguero

---

## 6. Qué hay que actualizar para que todo funcione

- **Base de datos**: ejecutar `database_taller.sql` en SSMS.
  Crea la BD `TallerMecanico`, todas las tablas, los catálogos y un usuario
  `admin` (contraseña `Admin123!`).
- **`appsettings.json`**:
  - `ConnectionStrings:DefaultConnection` → apuntar a la instancia real de SQL Server.
  - `Jwt:Key` → cambiar por una llave secreta antes de producción.
- **Restaurar paquetes NuGet** (`dotnet restore` o desde Visual Studio): EF Core,
  SQL Server provider, JWT Bearer, Swashbuckle y **QuestPDF** (para el PDF de
  factura — licencia Community, gratuita).
- **Nada de esto se toca en el código** salvo que cambien las reglas del
  negocio: los códigos de estado (1–6 y 1–5) están hardcodeados en los
  `Services` porque coinciden con el orden en que el script SQL los inserta.
  Si alguna vez se necesita reordenar o agregar estados en el script, hay que revisar
  `OrdenTrabajoService`, `ReparacionService` y `FacturaService`.
- **Si agregas un endpoint de escritura nuevo**: además de `[RequierePermiso("recurso.accion", Roles = "...")]`
  en el controller, hay que sembrar esa fila nueva en `INSERT INTO Permiso` del
  script SQL — si no existe en la tabla, nunca se le podrá asignar a nadie desde
  `/api/permisos`.

## 7. Cómo probar

- **Swagger**:  disponible en la raíz
  (`http://localhost:5000/`). Botón **Authorize** para pegar el Bearer token.
- **`TallerMecanicoAPI.http`**: incluye el flujo completo de ejemplo (login →
  cliente → vehículo → orden → diagnóstico → reparación → repuestos → factura)
  listo para ejecutar desde Visual Studio / VS Code.

