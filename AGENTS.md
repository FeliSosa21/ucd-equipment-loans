# AGENTS.md — UCD (Obligatorio DA1, ORT 2026)

Instrucciones para agentes de desarrollo (Claude, Copilot, Codex, etc.) que trabajan en este repositorio. Siguen las recomendaciones de https://agents.md/. Las instrucciones explícitas del integrante que pide la tarea tienen prioridad sobre este archivo; un `AGENTS.md` en una subcarpeta tiene prioridad sobre este para los archivos de esa carpeta.

## Proyecto

Sistema de préstamo de equipamiento de la Universidad (UCD). Solución .NET 9 en `UCD/UCD.sln`:

| Proyecto | Responsabilidad | Puede depender de |
|---|---|---|
| `Dominio` | Entidades y reglas de negocio (`DominioException`). | — |
| `Data` | Persistencia: `SqlContext` (EF Core), configuraciones (`Data/Configuraciones`), repositorios y migraciones. | `Dominio` |
| `DTOs` | Datos que cruzan entre servicios y UI. | — |
| `Servicios` | Casos de uso; traducen errores a `ServicioException` y devuelven DTOs. | `Dominio`, `Data`, `DTOs` |
| `UI` | Blazor Server; páginas en `Components/Pages`. No conoce el dominio. | `Servicios`, `DTOs` |

Cada proyecto tiene su proyecto de tests (`*.Tests`, MSTest).

## Comandos

Todos desde `UCD/`:

- Compilar: `dotnet build`
- Tests: `dotnet test`
- Base de datos (SQL Server en Docker, desde la raíz): `docker compose up -d`
- Nueva migración: `dotnet ef migrations add <Nombre> --project Data --startup-project UI`
- Aplicar migraciones: `dotnet ef database update --project Data --startup-project UI`
- Correr la app: `dotnet run --project UI`

## Forma de trabajo

1. **TDD siempre**, también en lo hecho por agentes (letra, pág. 8): un commit por paso, `test: ... (RED)`, `feat: ... (GREEN)`, `refactor: ... (REFACTOR)`. El RED compila y falla solo por el test nuevo; el GREEN es el código mínimo.
2. **Mostrar el código antes de cada commit** y explicar qué hace. Lo commitea el integrante, después de correr `dotnet test`; nunca se commitea con tests en rojo.
3. **Modo plan** para cambios grandes: proponer, ajustar con el integrante, recién ahí implementar.
4. **Ramas:** una por funcionalidad desde `develop` (`feature/...`, `fix/...`). Nada se mergea a `develop` sin PR revisado por otro integrante. No usar `amend`, `rebase` ni `push --force`.
5. **Migraciones:** una por vez y desde `develop` actualizado (el snapshot es un solo archivo). Revisar la migración antes de aplicarla: no puede crear ni borrar nada que no corresponda al cambio.
6. **Commit antes de usar una skill**, por si cambia código de forma inesperada.

## Partes que el agente NO escribe

La letra (pág. 6) pide que cada integrante haga **a mano, sin agentes y con TDD** un repositorio con Entity Framework. Ver `UCD/Data/AGENTS.md`. Sobre esos archivos el agente solo puede explicar, dar ejemplos análogos con otras entidades y revisar.

## Estilo de código

- Nombres en español y sin tildes; clases y métodos en PascalCase; campos privados `_camelCase`; tipos explícitos (no `var`).
- Sin comentarios que expliquen el código: el nombre tiene que alcanzar. Solo se comenta el porqué de una decisión no obvia.
- Métodos cortos y con hasta 3 parámetros; si hacen falta más, extraer una clase o un método de fábrica.
- Cada regla vive en la clase dueña del dato (Experto, GRASP). Preguntar en vez de comparar (`usuario.TienePermiso(p)`, no `usuario.Tipo == ...`).
- Estáticos solo si no tienen estado, no tienen dependencias y no pertenecen al dominio.
- Errores: el dominio lanza `DominioException(campo, mensaje)`; los servicios la traducen con `TraduccionDeErrores` a `ServicioException`; la UI muestra el error junto al campo (`ErrorDeCampo`).
- Sin interfaces para los repositorios: los tests usan el repositorio real sobre `Memoria` o sobre el proveedor InMemory de EF (una base con nombre único por test).
- Repositorios: nada de "traer todo"; paginar y contar (`ObtenerPagina...`, `Contar...`), salvo exportaciones.
- La UI no tiene reglas de negocio: llama a un servicio y muestra el resultado.

## Tests

- MSTest, nombres `Metodo_Escenario_Resultado`, estructura preparar / actuar / verificar separada por una línea en blanco.
- Persistencia con EF: guardar con un contexto y leer con otro, para no recibir el objeto que quedó en memoria.
- Sin `DateTime.Now` en tests ni en el dominio: usar `TimeProvider` (`RelojFijo` en tests).

## Skills del repositorio (`.claude/skills/`)

- `csharp-code-quality-review`: revisión de Clean Code, convenciones C#, robustez y usabilidad. Correrla antes de abrir un PR y sobre toda clase nueva o modificada. Sobre código manual solo reporta; no cambia nada.
- `revision-cohesion-acoplamiento`: cohesión, acoplamiento y GRASP. Correrla sobre servicios y clases nuevas antes del PR.

De cada revisión se aplica solo lo que la skill marca, con su propio ciclo de TDD, y se registra qué se aplicó y qué no (con el motivo).

## Documentación del uso de IA

Cada integrante registra herramienta, modelo, propósito y qué hizo el agente en cada tarea. Si el agente participó en un archivo manual (ejemplo, revisión o cambio de formato), se declara.
