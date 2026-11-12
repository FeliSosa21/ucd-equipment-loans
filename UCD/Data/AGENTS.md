# AGENTS.md — UCD/Data (persistencia)

Complementa el `AGENTS.md` de la raíz para los archivos de esta carpeta y de `UCD/Data.Tests`.

## Repositorios manuales (letra, pág. 6)

Cada integrante desarrolla **a mano, sin agentes y con TDD** un repositorio con Entity Framework, y su configuración de EF también es manual (profesor, 26/10). El agente **no escribe ni modifica** estos archivos:

| Entidad | Repositorio | Configuración | Tests |
|---|---|---|---|
| Usuario | `Repositorios/RepositorioUsuarioSql.cs` | `Configuraciones/UsuarioConfiguracion.cs` | `Data.Tests/Repositorios/RepositorioUsuarioSqlTests.cs` |
| Préstamo | `Repositorios/RepositorioPrestamoSql.cs` | `Configuraciones/ConfiguracionPrestamo.cs` | `Data.Tests/Repositorios/RepositorioPrestamoSqlTests.cs` |
| Catálogo | `Repositorios/RepositorioCatalogoSql.cs` | `Configuraciones/ConfiguracionCatalogo.cs`, `ConfiguracionTipoEquipamiento.cs` y `ConfiguracionUnidadFisica.cs` | `Data.Tests/Repositorios/RepositorioCatalogoSqlTests.cs` |

Sobre ellos el agente puede: explicar conceptos, mostrar ejemplos análogos con otra entidad inventada (por ejemplo `Producto`), revisar un intento y señalar errores, y correr la skill de calidad en modo solo lectura. Las migraciones de estos cambios las genera el integrante.

El resto (por ejemplo `MovimientoConfiguracion`, `TipoUsuarioConfiguracion`, `RepositorioMovimientoSql`, `RepositorioTipoUsuarioSql`) se puede hacer con agente, con TDD (profesor, 2/11).

## Convenciones de EF Core

- Una clase `IEntityTypeConfiguration<T>` por entidad en `Configuraciones/`; el dominio no usa atributos de EF. `SqlContext` las carga con `ApplyConfigurationsFromAssembly`.
- Claves `int Id` generadas por la base; las relaciones usan FK sombra (`HasForeignKey("TipoUsuarioId")`) y `OnDelete(DeleteBehavior.Restrict)`.
- Datos privados agrupados (por ejemplo `_registro`, `presentacion`, `topes`) se mapean con `OwnsOne` sobre el campo.
- Sin lazy loading: las consultas que devuelven entidades con relaciones usan `Include`.
- Comparaciones de texto sin distinguir mayúsculas con `ToLower()`; `string.Equals(..., StringComparison)` no se traduce a SQL.
- `OrderBy` con desempate (por `Id` u otro campo único).
- Los filtros por fechas usan `FiltroPorCatalogoYFechas`: `Desde` incluido y `Hasta` excluido.
- Las consultas para la exportación (req. 2.7), `ObtenerDevoluciones(filtro)` y `ObtenerPrestamos(filtro)`, devuelven todo el rango filtrado aunque el filtro no tenga criterios. Su firma fue acordada con el equipo: no la cambies aunque una revisión lo sugiera.
- Los seis tipos de usuario y los demás datos de instalación se cargan mediante scripts; no van embebidos en C# ni con `HasData` (foro, d=155847 y d=155880). El root es la excepción permitida por código: debe usar el Administrador ya persistido, sin crear tipos como reparación implícita. Los tests preparan sus propios datos.

## Tests de persistencia

- Proveedor InMemory de EF como test double, con una base de nombre único por test (`Guid.NewGuid().ToString()`).
- Guardar con un contexto y leer con otro (`_opciones` guardadas en `[TestInitialize]`).
- InMemory no valida todo lo de SQL Server (largos, índices únicos, FK): revisar la migración generada.
